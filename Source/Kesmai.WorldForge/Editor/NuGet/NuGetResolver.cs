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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class NuGetResolver
{
    /// <summary>
    /// Download the latest (listed) version of <paramref name="packageId"/> (optionally prerelease),
    /// resolve dependencies, and return compile-time references from ref/ for the nearest TFM.
    /// Packages are installed in the global packages folder using PackagePathResolver.
    /// </summary>
    public static async Task<IReadOnlyList<MetadataReference>> Resolve(string packageId, string targetFrameworkMoniker, CancellationToken ct = default)
    {
        var tfm = NuGetFramework.ParseFolder(targetFrameworkMoniker);
        var settings = Settings.LoadDefaultSettings(root: null);
        var sourceProvider = new PackageSourceProvider(settings);
        
        // grab enabled sources, ensure nuget.org is present
        var sources = sourceProvider.LoadPackageSources().Where(s => s.IsEnabled).ToList();
        
        if (sources.All(s => !s.Source.Contains("nuget.org", StringComparison.OrdinalIgnoreCase)))
            sources.Add(new PackageSource("https://api.nuget.org/v3/index.json"));

        // create repositories
        var repos = sources.Select(Repository.Factory.GetCoreV3).ToList();
        var globalPackages = SettingsUtility.GetGlobalPackagesFolder(settings);

        var cache = new SourceCacheContext();
        var reducer = new FrameworkReducer();

        // find latest version (not prerelease)
        var latest = await GetLatestVersion(repos, packageId, false, cache, ct)
                     ?? throw new InvalidOperationException($"Package '{packageId}' not found on configured sources.");

        return await ResolvePackages(new PackageIdentity(packageId, latest),
            tfm, repos, globalPackages, cache, reducer, ct);
    }

    private static async Task<IReadOnlyList<MetadataReference>> ResolvePackages(PackageIdentity root, NuGetFramework targetFx, IReadOnlyList<SourceRepository> repos, string globalPackagesFolder, SourceCacheContext cache, FrameworkReducer reducer, CancellationToken ct)
    {
        var visited = new HashSet<PackageIdentity>(PackageIdentityComparer.Default);
        var references = new ConcurrentDictionary<string, MetadataReference>(StringComparer.OrdinalIgnoreCase);

        async Task Recurse(PackageIdentity identity)
        {
            if (!visited.Add(identity))
                return;

            var resolver = new PackagePathResolver(globalPackagesFolder);
            
            var installedPath = resolver.GetInstalledPath(identity);

            if (installedPath is null)
            {
                await Download(repos, identity, resolver, cache, globalPackagesFolder, ct);
                
                installedPath = resolver.GetInstalledPath(identity);
            }

            using var reader = new PackageFolderReader(installedPath);
            
            foreach (var dll in GetNearestAssemblies(reader, installedPath, targetFx, reducer))
                references.TryAdd(dll, MetadataReference.CreateFromFile(dll));
    
            // Note: skipping dependency resolution for now, as it can lead to conflicts and bloat.
            /*
            var dependencyGroups = await reader.GetPackageDependenciesAsync(ct) ?? [];
            var nearestFx = reducer.GetNearest(targetFx, dependencyGroups.Select(g => g.TargetFramework)) 
                            ?? NuGetFramework.AnyFramework;
            
            var group = dependencyGroups.FirstOrDefault(g => FxEqual(g.TargetFramework, nearestFx));
            
            if (group is null)
                return;

            foreach (var dep in group.Packages)
            {
                var depIdentity = await ResolveDependency(repos, dep, cache, ct);
                
                if (depIdentity is not null)
                    await Recurse(depIdentity);
            }
            */
        }

        await Recurse(root);
        
        return references.Values.ToList();
    }

    private static IEnumerable<string> GetNearestAssemblies(PackageReaderBase reader, string installedPath, NuGetFramework targetFx, FrameworkReducer reducer)
    {
        // Try NuGet's ref groups
        var refGroups = reader.GetReferenceItemsAsync(CancellationToken.None)
                              .GetAwaiter().GetResult() ?? new List<FrameworkSpecificGroup>();

        if (refGroups.Any())
        {
            var pick = PickNearestGroup(refGroups, targetFx, reducer);
            foreach (var p in EnumerateDllsFromGroup(pick, installedPath))
                yield return p;
            yield break;
        }

        // Fallback: scan <installedPath>/ref/<tfm>/*.dll
        var refRoot = Path.Combine(installedPath, "ref");
        
        if (!Directory.Exists(refRoot)) 
            yield break;

        var groups = new List<(NuGetFramework TFM, string Directory)>();
        
        foreach (var folder in Directory.EnumerateDirectories(refRoot))
        {
            var tfmName = Path.GetFileName(folder);
            
            if (string.IsNullOrWhiteSpace(tfmName)) 
                continue;
            
            groups.Add((NuGetFramework.ParseFolder(tfmName), folder));
        }
        
        if (groups.Count is 0) 
            yield break;

        var nearest = reducer.GetNearest(targetFx, groups.Select(g => g.TFM)) ?? NuGetFramework.AnyFramework;
        var chosen = groups.FirstOrDefault(g => string.Equals(g.TFM.GetShortFolderName(), nearest.GetShortFolderName(), StringComparison.OrdinalIgnoreCase));
        
        if (string.IsNullOrEmpty(chosen.Directory)) 
            yield break;

        foreach (var dll in Directory.EnumerateFiles(chosen.Directory, "*.dll", SearchOption.TopDirectoryOnly))
            yield return dll;
    }

    private static FrameworkSpecificGroup? PickNearestGroup(IEnumerable<FrameworkSpecificGroup> groups, NuGetFramework targetFx, FrameworkReducer reducer)
    {
        if (!groups.Any())
            return null;

        var nearest = reducer.GetNearest(targetFx, groups.Select(g => g.TargetFramework))
                      ?? NuGetFramework.AnyFramework;

        var pick = groups.FirstOrDefault(g => string.Equals(g.TargetFramework.GetShortFolderName(), 
            nearest.GetShortFolderName(), StringComparison.OrdinalIgnoreCase));

        // Optional: fallback to same framework+version ignoring platform (e.g., net8.0 -> net8.0-windows8.0)
        if (pick is null)
        {
            pick = groups.FirstOrDefault(g =>
                string.Equals(g.TargetFramework.Framework, targetFx.Framework, StringComparison.OrdinalIgnoreCase)
                && g.TargetFramework.Version == targetFx.Version);
        }
        
        return pick;
    }

    private static IEnumerable<string> EnumerateDllsFromGroup(FrameworkSpecificGroup? group, string installedPath)
    {
        if (group is null) yield break;

        foreach (var item in group.Items)
        {
            if (item.EndsWith("/_._", StringComparison.Ordinal)) continue;
            if (!item.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) continue;

            var full = Path.Combine(installedPath, item.Replace(
                Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
            
            if (File.Exists(full))
                yield return full;
        }
    }

    private static async Task<PackageIdentity> ResolveDependency(IReadOnlyList<SourceRepository> repos, PackageDependency dep, SourceCacheContext cache, CancellationToken ct)
    {
        IEnumerable<NuGetVersion> versions = [];
        
        foreach (var r in repos)
        {
            var byId = await r.GetResourceAsync<FindPackageByIdResource>(ct);
            var avail = await byId.GetAllVersionsAsync(dep.Id, cache, NullLogger.Instance, ct);
            
            if (avail is not null) 
                versions = versions.Concat(avail);
        }

        var best = dep.VersionRange.FindBestMatch(versions.OrderByDescending(v => v));
        
        return best is null ? null : new PackageIdentity(dep.Id, best);
    }

    private static async Task<NuGetVersion?> GetLatestVersion(IReadOnlyList<SourceRepository> repos, string packageId, bool includePrerelease, SourceCacheContext cache, CancellationToken ct)
    {
        NuGetVersion? best = null;
        
        foreach (var r in repos)
        {
            var meta = await r.GetResourceAsync<PackageMetadataResource>(ct);
            var items = await meta.GetMetadataAsync(packageId, includePrerelease, includeUnlisted: false, cache, NullLogger.Instance, ct);
            var candidate = items?.Select(m => m.Identity.Version).OrderByDescending(v => v).FirstOrDefault();
           
            if (candidate is null) 
                continue;
            
            if (best is null || candidate > best) 
                best = candidate;
        }
        
        return best;
    }

    private static async Task Download(IReadOnlyList<SourceRepository> repos, PackageIdentity identity, PackagePathResolver resolver, SourceCacheContext cache, string globalPackagesFolder, CancellationToken ct)
    {
        foreach (var r in repos)
        {
            var download = await r.GetResourceAsync<DownloadResource>(ct);
            var result = await download.GetDownloadResourceResultAsync(
                identity, new PackageDownloadContext(cache),
                globalPackagesFolder,
                NullLogger.Instance, ct);

            if (result.Status != DownloadResourceResultStatus.Available) 
                continue;
            
            var extractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv3, XmlDocFileSaveMode.Skip,
                ClientPolicyContext.GetClientPolicy(Settings.LoadDefaultSettings(null), NullLogger.Instance),
                NullLogger.Instance);

            if (result.PackageStream is not null)
                await PackageExtractor.ExtractPackageAsync(result.PackageSource, result.PackageStream, resolver, extractionContext, ct);
            else if (result.PackageReader is not null)
                await PackageExtractor.ExtractPackageAsync(result.PackageSource, result.PackageReader, resolver, extractionContext, ct);
            else
                throw new InvalidOperationException("DownloadResourceResult has neither stream nor reader.");
            
            return;
        }

        throw new InvalidOperationException($"Failed to download {identity.Id} {identity.Version} from configured sources.");
    }

    private static bool FxEqual(NuGetFramework a, NuGetFramework b) =>
        string.Equals(a.GetShortFolderName(), b.GetShortFolderName(), StringComparison.OrdinalIgnoreCase);
}
