using CommonServiceLocator;
using DigitalRune.Storages;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
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

			var customTerrainPath = $@"{Core.CustomArtPath}\Data\Terrain-External.xml";
			document = XDocument.Load(storage.OpenFile(@"Data\Terrain-External.xml"));
			if (File.Exists(customTerrainPath))
            {
				try
				{
					document = XDocument.Load(customTerrainPath);
					document.Save(customTerrainPath); // pretty prints the document, should help keep tabs consistent and avoid ugly git commits
				}
				catch (Exception e)
                {
					System.Windows.MessageBox.Show(e.Message, "Custom Terrain-External.xml failed to load", System.Windows.MessageBoxButton.OK);
				}
			}
			
			var Overrides = new List<int>();
			var missingTextures = new List<string>();


			foreach (var terrainElement in document.Root.Elements("terrain"))
			{
				Terrain terrain = null;
				try
				{
					terrain = new Terrain(terrainElement);
				} 
				catch (MissingTextureException e)
                {
					missingTextures.Add(e.Message);
                }
				catch (Exception e)
                {
					System.Windows.MessageBox.Show($"Terrain failed to parse:\n{terrainElement}\n{e.Message}", "Custom Terrain-External.xml failed to load", System.Windows.MessageBoxButton.OK);
				}


				if (terrain != null)
				{
					if (ContainsKey(terrain.ID))
					{
						Overrides.Add(terrain.ID);
						Remove(terrain.ID);
					}
					Add(terrain.ID, terrain);
				}
			}
			if (missingTextures.Count > 0)
				System.Windows.MessageBox.Show($"The following texture files were missing:\n{string.Join("\n", missingTextures.Distinct())}", "Custom Terrain-External.xml failed to load", System.Windows.MessageBoxButton.OK);
			if (Overrides.Count > 0)
				System.Windows.MessageBox.Show($"The following Terrain IDs are already in use, but also specified in [{customTerrainPath}]:\n{string.Join(", ",Overrides.ToArray())}", "Custom Art IDs conflict", System.Windows.MessageBoxButton.OK);
			

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
