using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Kesmai.WorldForge.Editor.Assets;

/// <summary>The body an entity's OnSpawn sets, and its picture from the repo's body list.</summary>
public static class EntityBody
{
	// "Body = 431" in the creature's braces wins over the CreatureDTO's "bodyId: 431", as it does on the server
	private static readonly Regex _bodyAssignment = new(@"\bBody\s*=\s*(\d+)");
	private static readonly Regex _bodyArgument = new(@"\bbodyId\s*:\s*(\d+)");

	/// <summary>The body id the entity's OnSpawn sets, or null when it sets none.</summary>
	public static int? IdOf(SegmentEntity entity)
	{
		var script = entity?.Scripts.FirstOrDefault(s => s.Name == "OnSpawn")?.Body ?? String.Empty;
		var match = _bodyAssignment.Matches(script).LastOrDefault() ?? _bodyArgument.Matches(script).LastOrDefault();

		return match is not null && Int32.TryParse(match.Groups[1].Value, out var id) ? id : null;
	}

	/// <summary>
	/// The picture of the entity's body; null when it sets none, or while the body pictures are still loading (asking
	/// starts the loading, so they are there a moment later).
	/// </summary>
	public static BitmapSource PictureOf(SegmentEntity entity)
	{
		var loading = AssetCatalog.LoadAsync();

		if (!loading.IsCompletedSuccessfully || IdOf(entity) is not { } id)
			return null;

		return loading.Result.Bodies.FirstOrDefault(b => b.Id == id)?.Image;
	}
}
