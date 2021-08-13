using System;
using System.IO;
using System.Xml.Linq;

using DigitalRune.Storages;

namespace Kesmai.WorldForge
{
	public class XDocumentReader : StorageReader
	{
		#region Static

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		#endregion

		#region Constructors and Cleanup

		#endregion

		#region Methods

		public override T Read<T>(Stream stream)
		{
			return (T)(object)XDocument.Load(stream);
		}

		#endregion
	}
}
