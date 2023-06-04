using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;
#if (CanImport)

public class GroundConversion : ConversionPass
{
	private List<int> _targets;
	private int _movementCost;

	/// <summary>
	/// Initializes a new instance of the <see cref="GroundConversion"/> class.
	/// </summary>
	public GroundConversion(List<int> targets, int movementCost)
	{
		_targets = targets;
		_movementCost = movementCost;
	}

	/// <inheritdoc />
	public void Process(ISegmentImporter importer, IImportedRegion convertibleRegion, SegmentRegion convertedRegion)
	{
		var matches = convertedRegion.GetTiles(tile => true);
			
		foreach (var tile in matches)
		{
			var staticComponents = tile.GetComponents<StaticComponent>(component => _targets.Contains(component.Static));

			if (staticComponents.Count() > 1)
				continue;

			// TODO: Should we be able to have more than 1 floor tile? Is anything else just a decorator static?
			foreach (var component in staticComponents)
				tile.ReplaceComponent(component, new FloorComponent(component.Static, _movementCost));
		}
	}
}
	
#endif