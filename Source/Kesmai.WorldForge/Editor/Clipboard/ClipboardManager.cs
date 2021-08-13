﻿using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using CommonServiceLocator;
using Ionic.Zlib;
using XNA = Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Editor
{
	public class ClipboardManager
	{
		#region Static

		#endregion

		#region Fields
		
		#endregion

		#region Properties and Events

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="ClipboardManager"/> class.
		/// </summary>
		public ClipboardManager()
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Copies the first currently selected area.
		/// </summary>
		public void Copy(Selection selection)
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var document = presenter.ActiveDocument;

			if (document is SegmentRegion region)
			{
				var rootElement = new XElement("data");
				
				var areas = selection;
				var area = areas.FirstOrDefault<XNA.Rectangle>();

				var memoryStream = new MemoryStream();
				var binaryWriter = new BinaryWriter(memoryStream);
				
				if (area != default(XNA.Rectangle))
				{
					for (int x = area.Left; x < area.Right; x++)
					for (int y = area.Top; y < area.Bottom; y++)
					{
						binaryWriter.Write((int)x - area.Left);
						binaryWriter.Write((int)y - area.Top);

						var tile = region.GetTile(x, y);

						if (tile != null)
						{
							binaryWriter.Write((bool)true);
							binaryWriter.Write((string)tile.GetXElement().ToString());
						}
						else
						{
							binaryWriter.Write((bool)false);
						}
					}
				}

				memoryStream.Position = 0;
				
				Clipboard.SetDataObject(new DataObject("MemoryStream", memoryStream));
			}
		}

		/// <summary>
		/// Pastes this buffer at the first selected area.
		/// </summary>
		public void Paste(Selection selection)
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var document = presenter.ActiveDocument;

			if (document is SegmentRegion region)
			{
				var areas = selection;
				var area = areas.FirstOrDefault<XNA.Rectangle>();

				if (area != default(XNA.Rectangle))
				{
					if (Clipboard.GetDataObject() is DataObject data && data.GetDataPresent("MemoryStream")
					    && data.GetData("MemoryStream") is MemoryStream stream)
					{
						stream.Position = 0;

						using (var reader = new BinaryReader(stream))
						{
							var ox = area.Left;
							var oy = area.Top;

							while (stream.Position < stream.Length)
							{
								var x = reader.ReadInt32();
								var y = reader.ReadInt32();
								var valid = reader.ReadBoolean();

								if (!valid)
									continue;

								var tileData = reader.ReadString();
								var tileElement = XElement.Parse(tileData);

								var mx = ox + x;
								var my = oy + y;

								tileElement.Add(new XAttribute("x", mx));
								tileElement.Add(new XAttribute("y", my));

								region.SetTile(mx, my, new SegmentTile(tileElement));
							}
						}
					}
					
					region.UpdateTiles();
				}
			}
        }

		#endregion
	}
}
