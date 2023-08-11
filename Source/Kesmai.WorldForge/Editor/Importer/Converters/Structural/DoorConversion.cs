using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;
#if (CanImport)

public class DoorConversion : ConversionPass
{
	private List<int> _targets;

	/// <summary>
	/// Initializes a new instance of the <see cref="DoorConversion"/> class.
	/// </summary>
	public DoorConversion(List<int> targets)
	{
		_targets = targets;
	}

	/// <inheritdoc />
	public void Process(ISegmentImporter importer, IImportedRegion convertibleRegion, SegmentRegion convertedRegion)
	{
		var matches = convertedRegion.GetTiles(tile => true);
			
		foreach (var tile in matches)
		{
			var staticComponents = tile.GetComponents<StaticComponent>(component => _targets.Contains(component.Static));

			if (staticComponents.Count() > 1)
				throw new Exception("Encountered multiple door candidates.");

			foreach (var component in staticComponents)
			{
				var closedId = component.Static;
				var openId = GetOpenId(closedId);
				var secretId = GetSecretId(tile, closedId);
				var destroyedId = GetDestroyedId(closedId);

				tile.ReplaceComponent(component, new DoorComponent(closedId, openId, secretId, destroyedId));
			}
		}
	}

	private int GetOpenId(int closedId)
	{
		if (closedId >= 62 && closedId <= 67)
			return closedId + 12;
			
		return closedId + 12;
	}

	private int GetSecretId(SegmentTile tile, int closedId)
	{
		if (closedId >= 62 && closedId <= 67)
			return 25 + (closedId - 62);
			
		return closedId;
	}
		
	private int GetDestroyedId(int closedId)
	{
		if (closedId >= 62 && closedId <= 67)
			return 80 + (closedId - 62);
			
		return closedId;
	}
}

#endif