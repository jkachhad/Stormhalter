using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models
{
	public class LockersComponent : StaticComponent
	{
		#region Static

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="LockersComponent"/> class.
		/// </summary>
		public LockersComponent(int lockerId = 119) : base(lockerId)
		{
		}
		
		public LockersComponent(XElement element) : base(element)
		{
		}


		#endregion

		#region Methods
		
		#endregion
	}
}
