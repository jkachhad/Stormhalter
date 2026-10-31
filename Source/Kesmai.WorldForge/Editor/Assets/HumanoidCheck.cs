#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kesmai.WorldForge.Editor.Assets;

/// <summary>
/// Whether a creature class is a Humanoid, the only kind with Shift+Right Click options (Gossips). The class is followed
/// through the segment's own source (classes like CreatureDefaultEntity live there) and then the server's classes.
/// </summary>
public static class HumanoidCheck
{
	private static string? _scannedFolder;
	private static DateTime _scannedAt;
	private static Dictionary<string, string> _segmentBases = new();

	/// <summary>True or false when the class could be followed to a server class; null when it couldn't be.</summary>
	public static bool? IsHumanoid(string className, string? segmentFolder, IReadOnlyDictionary<string, string> serverBases)
	{
		var segmentBases = SegmentBases(segmentFolder);
		var name = Simple(className);
		var seen = new HashSet<string>();

		while (name.Length > 0 && seen.Add(name))
		{
			if (name == "Humanoid")
				return true;

			if (segmentBases.TryGetValue(name, out var segmentBase))
				name = segmentBase;
			else if (serverBases.TryGetValue(name, out var serverBase))
				name = serverBase;
			else
				return name is "CreatureEntity" or "MobileEntity" or "WorldEntity" or "Object" ? false : null;
		}

		return null;
	}

	private static bool LooksLikeInterface(string name) => name.Length > 1 && name[0] == 'I' && Char.IsUpper(name[1]);

	/// <summary>"Cache.CreatureDefaultEntity" or "List&lt;T&gt;" down to the plain class name.</summary>
	private static string Simple(string typeName)
	{
		var name = typeName.Trim();
		var generic = name.IndexOf('<');

		if (generic >= 0)
			name = name.Substring(0, generic);

		return name.Substring(name.LastIndexOf('.') + 1).Trim();
	}

	/// <summary>Base classes declared in the segment's Source folder, read again when a file there changes.</summary>
	private static Dictionary<string, string> SegmentBases(string? segmentFolder)
	{
		var source = String.IsNullOrWhiteSpace(segmentFolder) ? null : Path.Combine(segmentFolder, "Source");

		if (source is null || !Directory.Exists(source))
			return new Dictionary<string, string>();

		var files = Directory.GetFiles(source, "*.cs", SearchOption.AllDirectories)
			.Where(f => !f.Contains(@"\bin\") && !f.Contains(@"\obj\"))
			.ToList();
		var newest = files.Select(File.GetLastWriteTimeUtc).DefaultIfEmpty(DateTime.MinValue).Max();

		if (String.Equals(source, _scannedFolder, StringComparison.OrdinalIgnoreCase) && newest <= _scannedAt)
			return _segmentBases;

		var bases = new Dictionary<string, string>();

		foreach (var file in files)
		{
			try
			{
				var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();

				// a partial class may name its base on only one of its parts
				foreach (var declaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
				{
					if (declaration.BaseList?.Types.FirstOrDefault()?.Type is not { } baseType)
						continue;

					// a part that lists only interfaces (IBossRegen) gives way to the part naming the class
					var name = declaration.Identifier.Text;
					var named = Simple(baseType.ToString());

					if (!bases.TryGetValue(name, out var known) || (LooksLikeInterface(known) && !LooksLikeInterface(named)))
						bases[name] = named;
				}
			}
			catch (IOException)
			{
			}
		}

		_scannedFolder = source;
		_scannedAt = newest;
		_segmentBases = bases;

		return bases;
	}
}
