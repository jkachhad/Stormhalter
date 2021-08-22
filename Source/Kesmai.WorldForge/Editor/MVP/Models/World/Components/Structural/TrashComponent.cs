using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models
{
	public class TrashComponent : StaticComponent
	{
		#region Static

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="TrashComponent"/> class.
		/// </summary>
		public TrashComponent(int trashId) : base(trashId)
		{
		}
		
		public TrashComponent(XElement element) : base(element)
		{
		}

		#endregion

		#region Methods

		public override TerrainComponent Clone()
		{
			return new TrashComponent(GetXElement());
		}

		#endregion
	}
}
