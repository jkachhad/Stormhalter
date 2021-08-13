using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge.Models
{
	public class HiddenTeleporterComponent : TeleportComponent
	{
		private static ComponentRender _internal;
		
		[Browsable(false)]
		public new Color Color
		{
			get => base.Color;
			set => base.Color = value;
		}

		private List<Lightphase> _lightphases = new List<Lightphase>();
		private List<Moonphase> _moonphases = new List<Moonphase>();
		
		private List<ProfessionInfo> _professions = new List<ProfessionInfo>();
		private List<Alignment> _alignments = new List<Alignment>();
		
		private int _message;
		private int _level;
		
		private bool _allowNPC;
		
		[Browsable(true)]
		[ItemsSource(typeof(LightphaseItemsSource))]
		public List<Lightphase> Lightphases
		{
			get => _lightphases;
			set => _lightphases = value;
		}
		
		[Browsable(true)]
		[ItemsSource(typeof(MoonphaseItemsSource))]
		public List<Moonphase> Moonphases
		{
			get => _moonphases;
			set => _moonphases = value;
		}
		
		[Browsable(true)]
		[ItemsSource(typeof(ProfessionsItemsSource))]
		public List<ProfessionInfo> Professions
		{
			get => _professions;
			set => _professions = value;
		}
		
		[Browsable(true)]
		[ItemsSource(typeof(AlignmentItemsSource))]
		public List<Alignment> Alignments
		{
			get => _alignments;
			set => _alignments = value;
		}
		
		[Browsable(true)]
		public int Message
		{
			get => _message;
			set => _message = value;
		}
		
		[Browsable(true)]
		public int Level
		{
			get => _level;
			set => _level = value;
		}
		
		[Browsable(true)]
		public bool AllowNPC
		{
			get => _allowNPC;
			set => _allowNPC = value;
		}
		
		static HiddenTeleporterComponent()
		{
			var contentManager = ServiceLocator.Current.GetInstance<ContentManager>();
			
			var terrain = new Terrain(
				new TerrainLayer()
				{
					Sprite = new GameSprite(contentManager.Load<Texture2D>(@"WorldForge/Terrain/Teleporter")),
					Order = 9
				}
			);

			_internal = new ComponentRender(terrain, Color.FromNonPremultiplied(0, 255, 255, 150));
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="HiddenTeleporterComponent"/> class.
		/// </summary>
		public HiddenTeleporterComponent(int x, int y, int region) : base(x, y, region)
		{
			_lightphases.AddRange(Lightphase.All);
			_moonphases.AddRange(Moonphase.All);
			_professions.AddRange(ProfessionInfo.GetAll());
			_alignments.AddRange(Alignment.All);
		}
		
		public HiddenTeleporterComponent(XElement element) : base(element)
		{
			/* Load the lightphases under which this teleporter is active. */
			var lightphasesElement = element.Element("lightphases");

			if (lightphasesElement != null)
			{
				var selectAttribute = lightphasesElement.Attribute("select");

				if (selectAttribute != null)
				{
					var mode = (string)selectAttribute;

					switch (mode)
					{
						case "all": _lightphases.AddRange(Lightphase.All); break;
					}
				}
				else
				{
					var phases = ((string)lightphasesElement).Split(',');

					foreach (var phase in phases)
						_lightphases.Add(Lightphase.All.First(p => p.Name == phase));
				}
			}
			else
			{
				/* The element doesn't exist, we'll populate with all the phases as default behavior. */
				_lightphases.AddRange(Lightphase.All);
			}
			
			/* Load the Moonphase under which this teleporter is active. */
			var moonphasesElement = element.Element("moonphases");

			if (moonphasesElement != null)
			{
				var selectAttribute = moonphasesElement.Attribute("select");

				if (selectAttribute != null)
				{
					var mode = (string)selectAttribute;

					switch (mode)
					{
						case "all": _moonphases.AddRange(Moonphase.All); break;
					}
				}
				else
				{
					var phases = ((string)moonphasesElement).Split(',');

					foreach (var phase in phases)
						_moonphases.Add(Moonphase.All.First(p => p.Name == phase));
				}
			}
			else
			{
				/* The element doesn't exist, we'll populate with all the phases as default behavior. */
				_moonphases.AddRange(Moonphase.All);
			}
			
			/* Load the professions. */
			var professionsElement = element.Element("professions");
			var allProfessions = ProfessionInfo.GetAll();

			if (professionsElement != null)
			{
				var selectAttribute = professionsElement.Attribute("select");

				if (selectAttribute != null)
				{
					var mode = (string)selectAttribute;

					switch (mode)
					{
						case "all": _professions.AddRange(allProfessions); break;
					}
				}
				else
				{
					var names = ((string)professionsElement).Split(',');

					foreach (var name in names)
						_professions.Add(allProfessions.First(p => p.Name == name));
				}
			}
			else
			{
				/* The element doesn't exist, we'll populate with all the phases as default behavior. */
				_professions.AddRange(allProfessions);
			}
			
			/* Load the Alighments under which this teleporter is active. */
			var alignmentsElement = element.Element("alignments");

			if (alignmentsElement != null)
			{
				var selectAttribute = alignmentsElement.Attribute("select");

				if (selectAttribute != null)
				{
					var mode = (string)selectAttribute;

					switch (mode)
					{
						case "all": _alignments.AddRange(Alignment.All); break;
					}
				}
				else
				{
					var alignments = ((string)alignmentsElement).Split(',');

					foreach (var alignment in alignments)
						_alignments.Add(Alignment.All.First(p => p.Description == alignment));
				}
			}
			else
			{
				/* The element doesn't exist, we'll populate with all the phases as default behavior. */
				_alignments.AddRange(Alignment.All);
			}
			
			/* message when teleport is successful */
			var messageElement = element.Element("message");

			if (messageElement != null)
				_message = (int)messageElement;
			
			/* Level requirement */
			var levelElement = element.Element("level");

			if (levelElement != null)
				_level = (int)levelElement;
			
			/* NPCs */
			var allowNPCsElement = element.Element("allowNPC");

			if (allowNPCsElement != null)
				_allowNPC = (bool)allowNPCsElement;
		}
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			/**/
			var lightphasesElement = new XElement("lightphases");

			if (_lightphases.Any())
			{
				var allLightphases = Lightphase.All.ToList();
				var all = allLightphases.All(_lightphases.Contains) && allLightphases.Count == _lightphases.Count;

				if (all)
				{
					lightphasesElement.Add(new XAttribute("select", "all"));
				}
				else
				{
					lightphasesElement.Value = String.Join(",", _lightphases.Select(p => p.Name));
				}
			}
			else
			{
				lightphasesElement.Add(new XAttribute("select", "none"));
			}

			/**/
			var moonphasesElement = new XElement("moonphases");

			if (_moonphases.Any())
			{
				var allMoonphases = Moonphase.All.ToList();
				var all = allMoonphases.All(_moonphases.Contains) && allMoonphases.Count == _moonphases.Count;

				if (all)
				{
					moonphasesElement.Add(new XAttribute("select", "all"));
				}
				else
				{
					moonphasesElement.Value = String.Join(",", _moonphases.Select(p => p.Name));
				}
			}
			else
			{
				moonphasesElement.Add(new XAttribute("select", "none"));
			}
			
			/**/
			var professionsElement = new XElement("professions");

			if (_professions.Any())
			{
				var allProfessions = ProfessionInfo.GetAll();
				var all = allProfessions.All(_professions.Contains) && allProfessions.Count == _professions.Count;

				if (all)
				{
					professionsElement.Add(new XAttribute("select", "all"));
				}
				else
				{
					professionsElement.Value = String.Join(",", _professions.Select(p => p.Name));
				}
			}
			else
			{
				professionsElement.Add(new XAttribute("select", "none"));
			}
			
			/**/
			var alignmentsElement = new XElement("alignments");

			if (_alignments.Any())
			{
				var allAlignments = Alignment.All.ToList();
				var all = allAlignments.All(_alignments.Contains) && allAlignments.Count == _alignments.Count;

				if (all)
				{
					alignmentsElement.Add(new XAttribute("select", "all"));
				}
				else
				{
					alignmentsElement.Value = String.Join(",", _alignments.Select(p => p.Description));
				}
			}
			else
			{
				alignmentsElement.Add(new XAttribute("select", "none"));
			}
			
			element.Add(lightphasesElement);
			element.Add(moonphasesElement);
			element.Add(professionsElement);
			element.Add(alignmentsElement);
			
			if (_message > 0)
				element.Add(new XElement("message", _message));
			
			if (_level > 0)
				element.Add(new XElement("level", _level));

			if (_allowNPC)
				element.Add(new XElement("allowNPC", true));

			return element;
		}
		
		public override IEnumerable<ComponentRender> GetTerrain()
		{
			yield return _internal;
		}
		
		public override TerrainComponent Clone()
		{
			return new HiddenTeleporterComponent(GetXElement());
		}
	}
}