using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using CommonServiceLocator;

namespace Kesmai.WorldForge.Models
{
	public abstract class ActiveTeleporter : TeleportComponent
	{
		private int _teleporterId;

		[Browsable(true)]
		public int TeleporterId
		{
			get => _teleporterId;
			set => _teleporterId = value;
		}

		protected ActiveTeleporter(int teleporterId, int x, int y, int region) : base(x, y, region)
		{
			_teleporterId = teleporterId;
		}
		
		public ActiveTeleporter(XElement element) : base(element)
		{
			_teleporterId = (int)element.Element("teleporterId");
		}
		
		/// <inheritdoc />
		public override IEnumerable<ComponentRender> GetTerrain()
		{
			var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

			if (terrainManager.TryGetValue(_teleporterId, out Terrain terrain))
				yield return new ComponentRender(terrain, (IsValid() ? Color : Microsoft.Xna.Framework.Color.Red));
		}
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			element.Add(new XElement("teleporterId", _teleporterId));
	
			return element;
		}
	}
}