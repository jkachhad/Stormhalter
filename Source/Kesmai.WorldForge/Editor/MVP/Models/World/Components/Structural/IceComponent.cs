using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models
{
	public class IceComponent : FloorComponent
	{
		#region Static

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="IceComponent"/> class.
		/// </summary>
		public IceComponent(int iceId) : base(iceId, 1)
		{
		}
		
		public IceComponent(XElement element) : base(element)
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
			return new IceComponent(GetXElement());
		}

		#endregion
	}
}
