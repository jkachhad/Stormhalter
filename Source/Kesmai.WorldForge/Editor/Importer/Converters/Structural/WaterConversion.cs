using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor
{
#if (CanImport)

	public class WaterConversion : ConversionPass
	{
		private List<int> _targets;
		private int _depth;

		/// <summary>
		/// Initializes a new instance of the <see cref="WaterConversion"/> class.
		/// </summary>
		public WaterConversion(List<int> targets, int depth = 3)
		{
			_targets = targets;
			_depth = depth;
		}

		/// <inheritdoc />
		public void Process(ISegmentImporter importer, IImportedRegion convertibleRegion, SegmentRegion convertedRegion)
		{
			/* Get all the tiles available. Certain areas of water are not deep enough to drown. */
			var matches = convertedRegion.GetTiles(tile => true);

			foreach (var tile in matches)
			{
				var staticComponents = tile.GetComponents<StaticComponent>(component => _targets.Contains(component.Static));

				if (staticComponents.Count() > 1)
					throw new Exception("Encountered multiple water candidates.");

				foreach (var component in staticComponents)
					tile.ReplaceComponent(component, new WaterComponent(component.Static, _depth));
			}
		}
	}

#endif
}
