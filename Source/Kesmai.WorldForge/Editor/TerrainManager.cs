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

namespace Kesmai.WorldForge;

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
		missingTextures = missingTextures.Select(texture => texture.Replace("\\", "/")).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList(); // normalize slashes and ignore case
		if (missingTextures.Count > 0)
			System.Windows.MessageBox.Show($"The following texture files are missing:\n{string.Join("\n", missingTextures)}", "Custom Terrain-External.xml failed to load", System.Windows.MessageBoxButton.OK);
		if (Overrides.Count > 0)
			System.Windows.MessageBox.Show($"The following Terrain IDs are already in use, but also specified in [{customTerrainPath}]:\n{string.Join(", ",Overrides.ToArray())}", "Custom Art IDs conflict", System.Windows.MessageBoxButton.OK);

		//Process external textures against the mgcb file and add to it.
		var externalTextures = document.Root.Descendants("texture").Select(e => (string)e).Distinct().Select(texture=>texture.Replace("\\", "/")); // get all the texture filenames, normalize slashes to match the mgcb file
		var mgcbFile = $@"{Core.CustomArtPath}\Content.Stormhalter-External.mgcb";
		if (File.Exists(mgcbFile))
		{
			var mgcb = File.ReadAllLines(mgcbFile).ToList();
			var existingTextures = mgcb.Where(line => line.StartsWith("#begin ")).Select(line => line.Substring(7,line.Length-11)); // get just the texture filename, exclude the header (7) and extension(4)
			var newTextures = externalTextures.Except(existingTextures, StringComparer.InvariantCultureIgnoreCase).Except(missingTextures, StringComparer.InvariantCultureIgnoreCase).ToList();
			newTextures = newTextures.Where(t => File.Exists($@"{Core.CustomArtPath}\{t}.png")).ToList(); // Only modify mgcb file if the new texture actually exists.
			if (newTextures.Count > 0)
			{
				foreach (var texture in newTextures)
				{
					mgcb.AddRange(new string[]{
						@$"#begin {texture}.png",
						"/importer:TextureImporter",
						"/processor:TextureProcessor",
						"/processorParam:ColorKeyColor=255,0,255,255",
						"/processorParam:ColorKeyEnabled=True",
						"/processorParam:GenerateMipmaps=False",
						"/processorParam:PremultiplyAlpha=True",
						"/processorParam:ResizeToPowerOfTwo=False",
						"/processorParam:MakeSquare=False",
						"/processorParam:TextureFormat=Color",
						@$"/build:{texture}.png",
						""
					});
				}
				File.WriteAllLines(mgcbFile, mgcb);
				System.Windows.MessageBox.Show($"The following textures were used in new Terrains:\n\n{string.Join("\n", newTextures)}\n\nContent.Stormhalter-External.mgcb was updated with these textures.", "Processed new textures", System.Windows.MessageBoxButton.OK);
			}
				
			//Can these entries be safely deleted without human intervention?
			var unusedTextures = existingTextures.Except(externalTextures, StringComparer.InvariantCultureIgnoreCase).Where(texture=>!File.Exists($@"{Core.CustomArtPath}\{texture}.png"));
			if (unusedTextures.Count() > 0)
				System.Windows.MessageBox.Show($"The following textures were found in Content.Stormhalter-External.mgcb,\nbut are unused in any sprite in Textures-External.xml and not\npresent in Content directory:\n\n{string.Join("\n", unusedTextures)}\n\nContent.Stormhalter-External.mgcb should have these entries removed.", "Found unused textures", System.Windows.MessageBoxButton.OK);
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