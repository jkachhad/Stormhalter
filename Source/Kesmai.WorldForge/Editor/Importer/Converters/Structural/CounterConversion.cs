using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor;
#if (CanImport)

public class CounterConversion : ConversionPass
{
	private List<int> _targets;

	/// <summary>
	/// Initializes a new instance of the <see cref="CounterConversion"/> class.
	/// </summary>
	public CounterConversion(List<int> targets)
	{
		_targets = targets;
	}

	/// <inheritdoc />
	public void Process(ISegmentImporter importer, IImportedRegion convertibleRegion, SegmentRegion convertedRegion)
	{
		var matches = convertedRegion.GetTiles(tile => true);

		foreach (var tile in matches)
		{
			foreach (var direction in Direction.Cardinal)
			{
				var neighbor = convertedRegion.GetTile(tile, direction);

				if (neighbor != null)
				{
					var staticComponents = neighbor.GetComponents<StaticComponent>(component => _targets.Contains(component.Static));

					if (staticComponents.Count() > 1)
						throw new Exception("Encountered multiple counter candidates.");

					foreach (var component in staticComponents)
						neighbor.ReplaceComponent(component, new CounterComponent(component.Static, direction.Opposite));
				}
			}
		}
	}
}
	
#endif