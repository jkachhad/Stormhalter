using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge
{
	public class Terrain : List<TerrainLayer>
	{
		public int ID { get; protected set; }
		public int? LOM { get; private set; }
		public List<int> TIF { get; private set; }

		public Terrain(params TerrainLayer[] layers)
		{
			AddRange(layers);
		}

		public Terrain(XElement element)
		{
			var idAttribute = element.Attribute("id");
			var lomAttribute = element.Attribute("lom");
			var tifAttribute = element.Attribute("tif");

			if (idAttribute != null)
				ID = (int)idAttribute;

			if (lomAttribute != null)
				LOM = (int)lomAttribute;

			TIF = new List<int>();
			
			if (tifAttribute != null)
			{
				var tifList = ((string)tifAttribute).Split(',');

				if (tifList.Any())
				{
					foreach (var tifID in tifList)
						TIF.Add(int.Parse(tifID));
				}
			}

			foreach (var spriteElement in element.Elements("sprite"))
			{
				var layer = new TerrainLayer()
				{
					Sprite = new GameSprite(spriteElement),
				};

				var orderAttribute = spriteElement.Attribute("order");
				var underpileAttribute = spriteElement.Attribute("underpile");

				if (orderAttribute != null)
					layer.Order = (int)orderAttribute;

				if (underpileAttribute != null)
					layer.Underpile = (bool)underpileAttribute;

				Add(layer);
			}
		}
	}

	public class TerrainLayer
	{
		public GameSprite Sprite { get; set; }
		public int Order { get; set; }

		private bool _underpile;

		public bool Underpile
		{
			get { return _underpile || Order <= 3; }
			set
			{
				if (Order > 3)
					_underpile = value;
			}
		}
	}

	public class ComponentRender
	{
		public Terrain Terrain { get; set; }
		
		public Color Color { get; set; }
		
		public ComponentRender(Terrain terrain, Color color)
		{
			Terrain = terrain;
			Color = color;
		}
	}

	public class TerrainRender
	{
		public TerrainLayer Layer { get; set; }
		public Color Color { get; set; }

		public TerrainRender(TerrainLayer layer, Color color)
		{
			Layer = layer;
			Color = color;
		}
	}
}