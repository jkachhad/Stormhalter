using CommonServiceLocator;
using DigitalRune.Storages;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kesmai.WorldForge
{
	public class TerrainManager : Dictionary<int, Terrain>
	{
		public TerrainManager()
		{
			var storage = ServiceLocator.Current.GetInstance<IStorage>();
			var document = XDocument.Load(storage.OpenFile(@"Data\Terrain.xml"));

			foreach (var terrainElement in document.Root.Elements("terrain"))
			{
				var terrain = new Terrain(terrainElement);

				if (terrain != null)
					Add(terrain.ID, terrain);
			}

			try
			{
				document = XDocument.Load(storage.OpenFile(@"Data\Terrain-External.xml"));
			}
			catch (System.IO.FileNotFoundException) { return; }

			foreach (var terrainElement in document.Root.Elements("terrain"))
			{
				var terrain = new Terrain(terrainElement);

				if (terrain != null)
					Add(terrain.ID, terrain);
			}
		}

		public Terrain FromLom(int lomID)
		{
			return Values.FirstOrDefault(terrain => terrain.LOM.HasValue && terrain.LOM == lomID);
		}

		public Terrain FromTIF(int tifID)
		{
			return Values.FirstOrDefault(terrain => terrain.TIF.Contains(tifID));
		}
	}
}
