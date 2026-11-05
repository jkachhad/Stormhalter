using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kesmai.WorldForge.Roslyn;

/// <summary>
/// Test-compiles every script in a segment (entities, spawners, treasures) together with the segment's own code, against
/// the server's reference package, and reports the errors with the script and line they are in. Scripts are placed as
/// the server places them: an entity's as methods of Cache.Entities.Name, so code calling Entities.Name.OnSpawn() finds
/// them.
/// </summary>
public static class CompileCheck
{
	public sealed record Problem(string Owner, string Script, int Line, string Code, string Message, ISegmentObject Target);

	private sealed record Wrapped(string Owner, Script Script, ISegmentObject Target, int FirstLine);

	/// <summary>
	/// What the segment compiles against: the .NET reference assemblies, the server's reference package, and the
	/// packages it depends on that are already downloaded (DotNext and the rest).
	/// </summary>
	public static async Task<List<MetadataReference>> ReferencesAsync()
	{
		var references = new List<MetadataReference>();

		var package = await NuGetResolver.Resolve("Kesmai.Server.Reference");
		references.AddRange(await NuGetResolver.ResolveMetadataReferences(package));

		var packagesRoot = NuGet.Configuration.SettingsUtility.GetGlobalPackagesFolder(NuGet.Configuration.Settings.LoadDefaultSettings(null));
		var nuspec = await package.GetNuspecReaderAsync(default);

		foreach (var dependency in nuspec.GetDependencyGroups().SelectMany(g => g.Packages).DistinctBy(d => d.Id))
		{
			var folder = Path.Combine(packagesRoot, dependency.Id.ToLowerInvariant(), dependency.VersionRange.MinVersion?.ToNormalizedString() ?? "", "lib");

			if (!Directory.Exists(folder))
				continue;

			// the newest .NET the package has a build for
			var framework = Directory.GetDirectories(folder).OrderByDescending(FrameworkRank).FirstOrDefault();

			if (framework is not null)
				references.AddRange(Directory.GetFiles(framework, "*.dll").Select(f => MetadataReference.CreateFromFile(f)));
		}

		references.AddRange(FrameworkReferences());

		return references;
	}

	private static int FrameworkRank(string folder)
	{
		var name = Path.GetFileName(folder);

		if (name.StartsWith("net") && !name.StartsWith("netstandard") && !name.StartsWith("netcoreapp")
		    && double.TryParse(name[3..].Split('-')[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var version))
			return version < 10 && name.Length == 5 ? 0 : (int)(version * 10) + 100; // net48 is .NET Framework

		return name.StartsWith("netstandard2.1") ? 50 : name.StartsWith("netstandard2") ? 40 : 1;
	}

	/// <summary>The .NET reference assemblies from the SDK's packs, or the running .NET's own when there are none.</summary>
	private static IEnumerable<MetadataReference> FrameworkReferences()
	{
		var runtime = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
		var major = Environment.Version.Major;
		var packs = Path.GetFullPath(Path.Combine(runtime, "..", "..", "..", "packs", "Microsoft.NETCore.App.Ref"));

		if (Directory.Exists(packs))
		{
			var pack = Directory.GetDirectories(packs).Where(d => Path.GetFileName(d).StartsWith($"{major}.")).OrderByDescending(d => d).FirstOrDefault();
			var folder = pack is null ? null : Path.Combine(pack, "ref", $"net{major}.0");

			if (folder is not null && Directory.Exists(folder))
				return Directory.GetFiles(folder, "*.dll").Select(f => (MetadataReference)MetadataReference.CreateFromFile(f)).ToList();
		}

		return ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "").Split(Path.PathSeparator)
			.Where(p => p.StartsWith(runtime, StringComparison.OrdinalIgnoreCase))
			.Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)).ToList();
	}

	/// <summary>A name as a C# identifier: what isn't a letter, digit or underscore is dropped.</summary>
	private static string Identifier(string name)
	{
		var text = new string((name ?? "").Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

		return text.Length == 0 || char.IsDigit(text[0]) ? "_" + text : text;
	}

	public static List<Problem> Run(Segment segment, IEnumerable<MetadataReference> references)
	{
		var parse = new CSharpParseOptions(LanguageVersion.Latest);
		var trees = new List<SyntaxTree>();
		var wrapped = new Dictionary<SyntaxTree, Wrapped>();
		var className = Identifier(segment.Name);

		// the segment's own code
		if (!string.IsNullOrEmpty(segment.Directory) && Directory.Exists(segment.Directory))
		{
			foreach (var file in Directory.GetFiles(segment.Directory, "*.cs", SearchOption.AllDirectories)
				         .Where(p => !p.Contains(@"\obj\") && !p.Contains(@"\bin\")))
				trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(file), parse, file));
		}

		var classes = trees.SelectMany(t => t.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()).Select(c => c.Identifier.Text).ToHashSet();

		// what the usings name, and the editor's function for each entity and treasure, when the segment has none
		var stubs = new StringBuilder();

		stubs.AppendLine("using System; using Kesmai.Server.Game; using Kesmai.Server.Items;");
		stubs.AppendLine("namespace Kesmai.Server.Segments {");
		stubs.AppendLine("public static partial class Editor {");

		foreach (var name in segment.Treasures.Select(t => t.Name).Where(n => Identifier(n) == n).Distinct())
			stubs.AppendLine($"\tpublic static Func<MobileEntity, Container, ItemEntity> {name};");

		foreach (var name in segment.Entities.Select(e => e.Name).Where(n => Identifier(n) == n).Distinct())
			stubs.AppendLine($"\tpublic static Func<CreatureEntity> {name};");

		stubs.AppendLine("}");

		if (!classes.Contains(className))
			stubs.AppendLine($"public partial class {className} {{ }}");

		stubs.AppendLine("}");

		if (!classes.Contains("Cache"))
			stubs.AppendLine($"namespace Kesmai.Server.Internal.{className} {{ public partial class Cache {{ }} }}");

		trees.Add(CSharpSyntaxTree.ParseText(stubs.ToString(), parse, "Editor.g.cs"));

		// the usings a script has
		var usings = string.Join("\n", new[]
		{
			"System", "System.Collections.Generic", "System.Drawing", "System.Linq", "System.Text", "System.Threading.Tasks",
			"Kesmai.Server", "Kesmai.Server.Accounting", "Kesmai.Server.Engines.Commands", "Kesmai.Server.Engines.Interactions",
			"Kesmai.Server.Engines.Pathing", "Kesmai.Server.Engines.Quests", "Kesmai.Server.Engines.Spawners", "Kesmai.Server.Game",
			"Kesmai.Server.Items", "Kesmai.Server.Miscellaneous", "Kesmai.Server.Network", "Kesmai.Server.Spells", "Kesmai.Server.Targeting",
			$"static Kesmai.Server.Segments.{className}", "static Kesmai.Server.Segments.Editor",
			"SpanReader = DotNext.Buffers.SpanReader<byte>", "SpanWriter = DotNext.Buffers.PoolingArrayBufferWriter<byte>",
		}.Select(u => $"using {u};"));

		var index = 0;

		void Add(string owner, string holder, ISegmentObject target, IEnumerable<Script> scripts)
		{
			foreach (var script in scripts.Where(s => s.IsEnabled && !s.IsEmpty))
			{
				// each script in a class of its own inside Cache, as the server's code generator places them
				var prefix = $"{usings}\nnamespace Kesmai.Server.Internal.{className};\npublic partial class Cache {{\n{holder}\n"
					+ $"public static {script.Signature}\n{{\n";
				var suffix = "\n}\n" + new string('}', holder.Count(c => c == '{')) + "\n}\n";
				var tree = CSharpSyntaxTree.ParseText(prefix + script.Body + suffix, parse, $"{owner}.{script.Name}.cs");

				trees.Add(tree);
				wrapped[tree] = new Wrapped(owner, script, target, prefix.Count(c => c == '\n'));
			}
		}

		foreach (var entity in segment.Entities)
			Add($"entity {entity.Name}", $"public static partial class Entities {{ public static partial class {Identifier(entity.Name)} {{", entity, entity.Scripts);

		foreach (var spawner in segment.Spawns.Location.Cast<SegmentSpawner>().Concat(segment.Spawns.Region))
			Add($"spawner {spawner.Name}", $"public static partial class WorldForgeSpawner{index++} {{", spawner, spawner.Scripts);

		foreach (var treasure in segment.Treasures)
		{
			if (treasure is SegmentHoard hoard)
				Add($"hoard {treasure.Name}", $"public static partial class WorldForgeHoard{index++} {{", treasure, hoard.Scripts);

			foreach (var entry in treasure.Entries)
				Add($"treasure {treasure.Name}", $"public static partial class WorldForgeTreasure{index++} {{", treasure, entry.Scripts);
		}

		var compilation = CSharpCompilation.Create($"WorldForgeCheck.{className}", trees, references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Disable,
				allowUnsafe: true));

		var problems = new List<Problem>();

		foreach (var diagnostic in compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
		{
			var tree = diagnostic.Location.SourceTree;
			var line = diagnostic.Location.GetLineSpan().StartLinePosition.Line;

			if (tree is not null && wrapped.TryGetValue(tree, out var script))
				problems.Add(new Problem(script.Owner, script.Script.Name, Math.Max(1, line + 1 - script.FirstLine), diagnostic.Id,
					diagnostic.GetMessage(), script.Target));
			else
				problems.Add(new Problem(tree is null ? "segment" : Path.GetFileName(tree.FilePath), "", line + 1, diagnostic.Id,
					diagnostic.GetMessage(), null));
		}

		return problems;
	}
}
