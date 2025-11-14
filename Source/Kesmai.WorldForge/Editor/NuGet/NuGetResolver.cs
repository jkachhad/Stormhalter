using System;
using Microsoft.CodeAnalysis;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public static class NuGetResolver
{
    private sealed class PackageContext
    {
        public PackageContext(string rootPath, NuGetFramework targetFramework)
        {
            RootPath = rootPath;
            TargetFramework = targetFramework;
        }

        public string RootPath { get; }
        public NuGetFramework TargetFramework { get; }
    }

    private static readonly ConditionalWeakTable<PackageFolderReader, PackageContext> _packageContexts = new();

    public static async Task<PackageFolderReader> Resolve(string packageId, string targetFrameworkMoniker, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(packageId))
            throw new ArgumentException("Package identifier must be provided.", nameof(packageId));

        if (string.IsNullOrWhiteSpace(targetFrameworkMoniker))
            throw new ArgumentException("Target framework moniker must be provided.", nameof(targetFrameworkMoniker));

        ct.ThrowIfCancellationRequested();

        var targetFramework = NuGetFramework.ParseFolder(targetFrameworkMoniker);
        if (targetFramework.IsUnsupported)
            throw new ArgumentException($"Unsupported target framework '{targetFrameworkMoniker}'.", nameof(targetFrameworkMoniker));

        var settingsRoot = LocateSettingsRoot();
        var settings = Settings.LoadDefaultSettings(settingsRoot);

        var packageSourceProvider = new PackageSourceProvider(settings);
        var repositories = CreateRepositories(packageSourceProvider);

        var logger = NullLogger.Instance;

        using var cacheContext = new SourceCacheContext();

        var version = await ResolveVersionAsync(repositories, packageId, targetFramework, cacheContext, logger, ct).ConfigureAwait(false);
        if (version is null)
            throw new InvalidOperationException($"Unable to locate a compatible version of '{packageId}' for '{targetFrameworkMoniker}'.");

        var packagesRoot = GetPackagesRoot(settingsRoot, settings);
        Directory.CreateDirectory(packagesRoot);

        var packageIdentity = new PackageIdentity(packageId, version);
        var packagePathResolver = new PackagePathResolver(packagesRoot, useSideBySidePaths: true);
        var versionPathResolver = new VersionFolderPathResolver(packagesRoot, isLowercase: true);

        var existingReader = TryCreateInstalledReader(packagePathResolver, versionPathResolver, packageIdentity, targetFramework);
        if (existingReader is not null)
            return existingReader;

        var downloadContext = new PackageDownloadContext(cacheContext);
        var extractionContext = new PackageExtractionContext(
            PackageSaveMode.Defaultv3,
            XmlDocFileSaveMode.Skip,
            ClientPolicyContext.GetClientPolicy(settings, logger),
            logger);

        foreach (var repository in repositories)
        {
            ct.ThrowIfCancellationRequested();

            var downloadResource = await repository.GetResourceAsync<DownloadResource>(ct).ConfigureAwait(false);
            if (downloadResource is null)
                continue;

            using var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                packageIdentity,
                downloadContext,
                packagesRoot,
                logger,
                ct).ConfigureAwait(false);

            if (downloadResult.Status != DownloadResourceResultStatus.Available)
                continue;

            if (downloadResult.PackageReader is PackageFolderReader folderReader)
            {
                var reader = TryCreateInstalledReader(packagePathResolver, versionPathResolver, packageIdentity, targetFramework)
                             ?? TryRegisterDownloadReader(folderReader, targetFramework);
                if (reader is not null)
                    return reader;

                continue;
            }

            if (downloadResult.PackageReader is not null)
            {
                await PackageExtractor.ExtractPackageAsync(
                    repository.PackageSource.Source,
                    downloadResult.PackageReader,
                    packagePathResolver,
                    extractionContext,
                    ct,
                    Guid.Empty).ConfigureAwait(false);
            }
            else if (downloadResult.PackageStream is not null)
            {
                if (downloadResult.PackageStream.CanSeek)
                    downloadResult.PackageStream.Position = 0;

                await PackageExtractor.ExtractPackageAsync(
                    repository.PackageSource.Source,
                    downloadResult.PackageStream,
                    packagePathResolver,
                    extractionContext,
                    ct,
                    Guid.Empty).ConfigureAwait(false);
            }

            var installedReader = TryCreateInstalledReader(packagePathResolver, versionPathResolver, packageIdentity, targetFramework);
            if (installedReader is not null)
                return installedReader;
        }

        throw new InvalidOperationException($"Failed to download package '{packageId}' ({version}).");
    }

    public static async Task<IReadOnlyList<MetadataReference>> ResolveMetadataReferences(PackageFolderReader packageFolderReader, CancellationToken ct = default)
    {
        if (packageFolderReader is null)
            throw new ArgumentNullException(nameof(packageFolderReader));

        ct.ThrowIfCancellationRequested();

        var context = GetOrCreateContext(packageFolderReader);
        var targetFramework = context.TargetFramework;

        var referenceGroups = await packageFolderReader.GetReferenceItemsAsync(ct).ConfigureAwait(false);
        var selectedGroup = SelectNearestGroup(referenceGroups, targetFramework);

        if (selectedGroup is null || !selectedGroup.Items.Any())
        {
            var libGroups = await packageFolderReader.GetLibItemsAsync(ct).ConfigureAwait(false);
            selectedGroup = SelectNearestGroup(libGroups, targetFramework);
        }

        IReadOnlyList<MetadataReference> references;

        if (selectedGroup is not null && selectedGroup.Items.Any())
        {
            references = CreateMetadataReferences(context.RootPath, selectedGroup.Items, ct);
        }
        else
        {
            var allFiles = await packageFolderReader.GetFilesAsync(ct).ConfigureAwait(false);
            references = CreateMetadataReferences(context.RootPath, allFiles, ct);
        }

        return references;
    }

    public static async Task<Stream> ResolveStream(PackageFolderReader packageFolderReader, string path, CancellationToken ct = default)
    {
        if (packageFolderReader is null)
            throw new ArgumentNullException(nameof(packageFolderReader));

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Package-relative path must be provided.", nameof(path));

        ct.ThrowIfCancellationRequested();

        var context = GetOrCreateContext(packageFolderReader);
        var normalizedRelativePath = NormalizeRelativePath(path);
        var absolutePath = GetAbsolutePath(context.RootPath, normalizedRelativePath);

        if (File.Exists(absolutePath))
            return OpenRead(absolutePath);

        var canonicalEntryPath = NormalizePackageEntryPath(path);
        var stream = await packageFolderReader.GetStreamAsync(canonicalEntryPath, ct).ConfigureAwait(false);
        if (stream is not null)
            return stream;

        var allFiles = await packageFolderReader.GetFilesAsync(ct).ConfigureAwait(false);
        var matchingEntry = allFiles.FirstOrDefault(file =>
            string.Equals(NormalizeRelativePath(file), normalizedRelativePath, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(matchingEntry))
        {
            var candidatePath = GetAbsolutePath(context.RootPath, NormalizeRelativePath(matchingEntry));
            if (File.Exists(candidatePath))
                return OpenRead(candidatePath);

            stream = await packageFolderReader.GetStreamAsync(NormalizePackageEntryPath(matchingEntry), ct).ConfigureAwait(false);
            if (stream is not null)
                return stream;
        }

        throw new FileNotFoundException($"Unable to locate '{path}' within the package.", path);

        static FileStream OpenRead(string fullPath) =>
            new(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, FileOptions.Asynchronous);
    }

    private static IReadOnlyList<SourceRepository> CreateRepositories(PackageSourceProvider packageSourceProvider)
    {
        var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
        var packageSources = packageSourceProvider
            .LoadPackageSources()
            .Where(source => source.IsEnabled)
            .ToList();

        if (packageSources.Count == 0)
            throw new InvalidOperationException("No NuGet package sources are configured.");

        return packageSources
            .Select(sourceRepositoryProvider.CreateRepository)
            .ToList();
    }

    private static PackageFolderReader? TryCreateInstalledReader(
        PackagePathResolver packagePathResolver,
        VersionFolderPathResolver versionPathResolver,
        PackageIdentity packageIdentity,
        NuGetFramework targetFramework)
    {
        var installedPath = packagePathResolver.GetInstalledPath(packageIdentity);
        if (!string.IsNullOrEmpty(installedPath) && Directory.Exists(installedPath))
            return CreatePackageFolderReader(installedPath, targetFramework);

        var candidatePath = versionPathResolver.GetInstallPath(packageIdentity.Id, packageIdentity.Version);
        if (!string.IsNullOrEmpty(candidatePath) && Directory.Exists(candidatePath))
            return CreatePackageFolderReader(candidatePath, targetFramework);

        return null;
    }

    private static PackageFolderReader? TryRegisterDownloadReader(PackageFolderReader downloadReader, NuGetFramework targetFramework)
    {
        try
        {
            var root = GetReaderRootPath(downloadReader);
            RegisterPackageContext(downloadReader, root, targetFramework);
            return downloadReader;
        }
        catch
        {
            return null;
        }
    }

    private static PackageFolderReader CreatePackageFolderReader(string installedPath, NuGetFramework targetFramework)
    {
        var reader = new PackageFolderReader(installedPath);
        RegisterPackageContext(reader, installedPath, targetFramework);
        return reader;
    }

    private static PackageContext RegisterPackageContext(PackageFolderReader reader, string rootPath, NuGetFramework targetFramework)
    {
        if (reader is null)
            throw new ArgumentNullException(nameof(reader));

        _packageContexts.Remove(reader);
        var context = new PackageContext(Path.GetFullPath(rootPath), targetFramework);
        _packageContexts.Add(reader, context);
        return context;
    }

    private static PackageContext GetOrCreateContext(PackageFolderReader reader)
    {
        if (_packageContexts.TryGetValue(reader, out var context))
            return context;

        var root = GetReaderRootPath(reader);
        return RegisterPackageContext(reader, root, NuGetFramework.AnyFramework);
    }

    private static string LocateSettingsRoot()
    {
        var baseDirectory = AppContext.BaseDirectory;
        if (string.IsNullOrEmpty(baseDirectory))
            baseDirectory = Directory.GetCurrentDirectory();

        var current = new DirectoryInfo(baseDirectory);

        while (current is not null)
        {
            var nugetConfig = Path.Combine(current.FullName, "nuget.config");
            if (File.Exists(nugetConfig))
                return current.FullName;

            current = current.Parent;
        }

        return baseDirectory;
    }

    private static string GetPackagesRoot(string settingsRootPath, ISettings settings)
    {
        var path = SettingsUtility.GetGlobalPackagesFolder(settings)
                   ?? SettingsUtility.GetRepositoryPath(settings);

        if (string.IsNullOrEmpty(path))
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        }
        else if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(Path.Combine(settingsRootPath, path));
        }

        return path;
    }

    private static async Task<NuGetVersion?> ResolveVersionAsync(
        IEnumerable<SourceRepository> repositories,
        string packageId,
        NuGetFramework targetFramework,
        SourceCacheContext cacheContext,
        ILogger logger,
        CancellationToken token)
    {
        var compatibility = DefaultCompatibilityProvider.Instance;

        foreach (var repository in repositories)
        {
            token.ThrowIfCancellationRequested();

            var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>(token).ConfigureAwait(false);
            if (metadataResource is null)
                continue;

            var metadata = await metadataResource.GetMetadataAsync(
                    packageId,
                    includePrerelease: true,
                    includeUnlisted: false,
                    cacheContext,
                    logger,
                    token).ConfigureAwait(false);

            var orderedMetadata = metadata
                .Where(m => m is not null)
                .OrderByDescending(m => m.Identity.Version)
                .ToList();

            if (orderedMetadata.Count == 0)
                continue;

            var stableCandidate = SelectCompatible(orderedMetadata.Where(m => !m.Identity.Version.IsPrerelease));
            if (stableCandidate is not null)
                return stableCandidate.Identity.Version;

            var anyCandidate = SelectCompatible(orderedMetadata);
            if (anyCandidate is not null)
                return anyCandidate.Identity.Version;
        }

        return null;

        IPackageSearchMetadata? SelectCompatible(IEnumerable<IPackageSearchMetadata> candidates)
        {
            IPackageSearchMetadata? fallback = null;

            foreach (var metadata in candidates)
            {
                token.ThrowIfCancellationRequested();

                if (metadata is null)
                    continue;

                var dependencyGroups = metadata.DependencySets?.ToList();
                if (dependencyGroups is null || dependencyGroups.Count == 0)
                {
                    fallback ??= metadata;
                    continue;
                }

                if (dependencyGroups.Any(group =>
                        group is null ||
                        group.TargetFramework is null ||
                        group.TargetFramework.IsAny ||
                        compatibility.IsCompatible(targetFramework, group.TargetFramework)))
                {
                    return metadata;
                }

                fallback ??= metadata;
            }

            return fallback;
        }
    }

    private static FrameworkSpecificGroup? SelectNearestGroup(IEnumerable<FrameworkSpecificGroup>? groups, NuGetFramework targetFramework)
    {
        if (groups is null)
            return null;

        var materialized = groups
            .Where(group => group is not null && !group.HasEmptyFolder && group.Items is not null && group.Items.Any())
            .Select(group => new
            {
                Group = group,
                Framework = NormalizeFramework(group.TargetFramework)
            })
            .ToList();

        if (materialized.Count == 0)
            return null;

        var reducer = new FrameworkReducer();
        var frameworks = materialized.Select(m => m.Framework).ToList();

        FrameworkSpecificGroup? LocateByFramework(NuGetFramework framework) =>
            materialized.FirstOrDefault(m => NuGetFrameworkEqualityComparer.Equals(m.Framework, framework))?.Group;

        if (!IsAnyFramework(targetFramework))
        {
            var nearest = reducer.GetNearest(targetFramework, frameworks);
            if (nearest is not null)
            {
                var group = LocateByFramework(nearest);
                if (group is not null)
                    return group;
            }
        }

        var anyGroup = materialized.FirstOrDefault(m => IsAnyFramework(m.Framework))?.Group;
        if (anyGroup is not null)
            return anyGroup;

        return materialized
            .OrderByDescending(m => m.Framework.Version)
            .ThenBy(m => m.Framework.Framework, StringComparer.OrdinalIgnoreCase)
            .First()
            .Group;
    }

    private static IReadOnlyList<MetadataReference> CreateMetadataReferences(string rootPath, IEnumerable<string> relativePaths, CancellationToken ct)
    {
        if (relativePaths is null)
            return Array.Empty<MetadataReference>();

        var references = new List<MetadataReference>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var relativePath in relativePaths)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(relativePath))
                continue;

            if (!IsAssemblyPath(relativePath))
                continue;

            var normalizedPath = NormalizeRelativePath(relativePath);
            var absolutePath = GetAbsolutePath(rootPath, normalizedPath);

            if (!File.Exists(absolutePath))
                continue;

            if (seen.Add(absolutePath))
                references.Add(MetadataReference.CreateFromFile(absolutePath));
        }

        return references;
    }

    private static bool IsAssemblyPath(string path)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
            return false;

        return extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".winmd", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

    private static string NormalizeRelativePath(string path)
    {
        var normalized = NormalizePath(path);
        normalized = normalized.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return normalized;
    }

    private static string NormalizePackageEntryPath(string path) =>
        path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

    private static string GetAbsolutePath(string rootPath, string normalizedRelativePath)
    {
        var rootFullPath = Path.GetFullPath(rootPath);
        var combined = Path.Combine(rootFullPath, normalizedRelativePath);
        var fullPath = Path.GetFullPath(combined);

        if (!fullPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"The path '{normalizedRelativePath}' resolves outside the package root.");

        return fullPath;
    }

    private static string GetReaderRootPath(PackageFolderReader reader)
    {
        var field = typeof(PackageFolderReader).GetField("_root", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(reader) is DirectoryInfo directory)
            return directory.FullName;

        throw new InvalidOperationException("Unable to determine the package root directory.");
    }

    private static NuGetFramework NormalizeFramework(NuGetFramework? framework) =>
        framework ?? NuGetFramework.AnyFramework;

    private static bool IsAnyFramework(NuGetFramework framework) =>
        framework == null || framework.IsAny;

    private static class NuGetFrameworkEqualityComparer
    {
        private static readonly IEqualityComparer<NuGetFramework> Comparer = NuGetFrameworkFullComparer.Instance;

        public static bool Equals(NuGetFramework? left, NuGetFramework? right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return Comparer.Equals(left, right);
        }
    }
}
