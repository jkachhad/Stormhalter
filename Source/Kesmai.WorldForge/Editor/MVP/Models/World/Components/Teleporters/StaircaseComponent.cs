using System;
using System.IO;
using System.Xml.Linq;

using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Models
{
	public class StaircaseComponent : ActiveTeleporter
	{
		#region Static

		#endregion

		#region Fields
		

		#endregion

		#region Properties and Events
		
		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="StaircaseComponent"/> class.
		/// </summary>
		public StaircaseComponent(int staircaseId, int x, int y, int region, bool descends = false) : base(staircaseId, x, y, region)
		{
		}
		
		public StaircaseComponent(XElement element) : base(element)
		{
		}


		#endregion

		#region Methods
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();
			
			return element;
		}

		public override TerrainComponent Clone()
		{
			return new StaircaseComponent(GetXElement());
		}

		#endregion
	}
}
