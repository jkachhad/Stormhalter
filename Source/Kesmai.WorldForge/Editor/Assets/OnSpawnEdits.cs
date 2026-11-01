#nullable enable

using System;
using System.Text.RegularExpressions;

namespace Kesmai.WorldForge.Editor.Assets;

/// <summary>
/// Text edits for an OnSpawn body written in the usual shape:
/// <code>
/// var orc = new SuperSurfaceUrk()
/// {
///     Name = "Urk Hunter",
/// };
/// orc.Brain = new RangedAI(orc);
/// return orc;
/// </code>
/// Each edit replaces the matching line when one exists, otherwise adds a new line in the right place.
/// </summary>
public static class OnSpawnEdits
{
	private static readonly Regex _declaration = new(@"var\s+(?<name>\w+)\s*=\s*new\s+(?<type>[\w<>.]+)\s*\([^)]*\)\s*\{", RegexOptions.Compiled);

	/// <summary>The creature variable, or "creature" when the body has no declaration yet.</summary>
	public static string GetVariable(string body)
	{
		var match = _declaration.Match(body);
		return match.Success ? match.Groups["name"].Value : "creature";
	}

	/// <summary>Changes the class created by the declaration, or starts a new body when there is none.</summary>
	public static string SetCreatureType(string body, string type)
	{
		var declaration = _declaration.Match(body);

		if (declaration.Success)
		{
			var group = declaration.Groups["type"];
			return body.Remove(group.Index, group.Length).Insert(group.Index, type);
		}

		var nl = Environment.NewLine;
		var skeleton = $"\tvar creature = new {type}(){nl}\t{{{nl}\t}};{nl}{nl}\treturn creature;";

		return String.IsNullOrWhiteSpace(body) ? skeleton : skeleton + nl + body;
	}

	/// <summary>Sets <c>Property = value,</c> inside the object initializer.</summary>
	public static string SetInitializer(string body, string property, string value)
	{
		var line = new Regex($@"^(?<indent>[ \t]*){Regex.Escape(property)}\s*=\s*[^\r\n]*?,[ \t]*(?=\r?$)", RegexOptions.Multiline);
		var existing = line.Match(body);

		if (existing.Success)
			return body.Remove(existing.Index, existing.Length)
				.Insert(existing.Index, $"{existing.Groups["indent"].Value}{property} = {value},");

		var declaration = _declaration.Match(body);

		if (!declaration.Success)
			return AppendStatement(body, $"// {property} = {value},  (add a 'var creature = new ...() {{ }}' declaration first)");

		var indent = GetIndent(body, declaration.Index) + "\t";
		var insertAt = declaration.Index + declaration.Length;

		return body.Insert(insertAt, $"{Environment.NewLine}{indent}{property} = {value},");
	}

	/// <summary>Replaces a statement matched by <paramref name="pattern"/>, or adds it before the return.</summary>
	public static string SetStatement(string body, string pattern, string statement)
	{
		var line = new Regex($@"^(?<indent>[ \t]*){pattern}[^\r\n]*(?=\r?$)", RegexOptions.Multiline);
		var existing = line.Match(body);

		if (existing.Success)
			return body.Remove(existing.Index, existing.Length)
				.Insert(existing.Index, existing.Groups["indent"].Value + statement);

		return AppendStatement(body, statement);
	}

	/// <summary>Adds a statement just before <c>return</c>, or at the end of the body.</summary>
	public static string AppendStatement(string body, string statement)
	{
		var returnLine = new Regex(@"^(?<indent>[ \t]*)return\b", RegexOptions.Multiline).Match(body);

		if (returnLine.Success)
			return body.Insert(returnLine.Index, $"{returnLine.Groups["indent"].Value}{statement}{Environment.NewLine}");

		var separator = body.Length == 0 || body.EndsWith("\n") ? String.Empty : Environment.NewLine;

		return body + separator + "\t" + statement;
	}

	private static string GetIndent(string body, int index)
	{
		var lineStart = body.LastIndexOf('\n', Math.Max(0, index - 1)) + 1;
		var end = lineStart;

		while (end < body.Length && (body[end] == ' ' || body[end] == '\t'))
			end++;

		return body.Substring(lineStart, end - lineStart);
	}
}
