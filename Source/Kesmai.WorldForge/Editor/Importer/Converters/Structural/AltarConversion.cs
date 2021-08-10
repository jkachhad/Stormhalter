using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor
{
#if (CanImport)

	public class AltarConversion : ConversionPass
	{
		private List<int> _targets;

		/// <summary>
		/// Initializes a new instance of the <see cref="AltarConversion"/> class.
		/// </summary>
		public AltarConversion(List<int> targets)
		{
			_targets = targets;
		}

		/// <inheritdoc />
		public void Process(ISegmentImporter importer, IImportedRegion convertibleRegion, SegmentRegion convertedRegion)
		{
			var matches = convertedRegion.GetTiles(tile =>
				/* importer.HasDataFlag(tile, 10, 0x10)  Get those tiles which are marked with an altar flag. */

				/* 1. If the tile has a counter, we do not create the altar. */
				!tile.OfType<CounterComponent>().Any()
			);

			foreach (var tile in matches)
			{
				var staticComponents = tile.GetComponents<StaticComponent>(component => _targets.Contains(component.Static));

				if (staticComponents.Count() > 1)
					throw new Exception("Encountered multiple altar candidates.");

				foreach (var component in staticComponents)
					tile.ReplaceComponent(component, new AltarComponent(component.Static));
			}
		}
	}
	
#endif
}
