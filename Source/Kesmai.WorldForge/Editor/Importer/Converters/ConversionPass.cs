using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Editor
{
#if (CanImport)

	public interface ConversionPass
	{
		/// <summary>
		/// Processes the specified segments.
		/// </summary>
		void Process(ISegmentImporter importer, IImportedRegion convertibleRegion, SegmentRegion convertedRegion);
	}
	
#endif
}
