using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge
{
	public class SpawnerBeforeSpawnScriptTemplate : ScriptTemplate
	{
		public override IEnumerable<string> GetSegments()
		{
			yield return "#load \"WorldForge\"\nvoid OnBeforeSpawn(Spawner spawner)\n{";
			yield return "}";
		}
	}
	
	public class SpawnerAfterSpawnScriptTemplate : ScriptTemplate
	{
		public override IEnumerable<string> GetSegments()
		{
			yield return "#load \"WorldForge\"\nvoid OnAfterSpawn(Spawner spawner, MobileEntity spawn)\n{";
			yield return "}";
		}
	}
	
	[ScriptTemplate("OnBeforeSpawn", typeof(SpawnerBeforeSpawnScriptTemplate))]
	[ScriptTemplate("OnAfterSpawn", typeof(SpawnerAfterSpawnScriptTemplate))]
	public abstract class Spawner : ObservableObject
	{
		private string _name;
		
		private TimeSpan _minimumDelay;
		private TimeSpan _maximumDelay;

		private int _maximum;
		
		private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();

		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

        public override string ToString()
        {
			return Name;
        }
        public TimeSpan MinimumDelay
		{
			get => _minimumDelay;
			set => SetProperty(ref _minimumDelay, value);
		}

		public TimeSpan MaximumDelay
		{
			get => _maximumDelay;
			set => SetProperty(ref _maximumDelay, value);
		}

		public int Maximum
		{
			get => _maximum;
			set => SetProperty(ref _maximum, value);
		}

		public ObservableCollection<SpawnEntry> Entries { get; set; } = new ObservableCollection<SpawnEntry>();

		public ObservableCollection<Script> Scripts
		{
			get => _scripts;
			set => SetProperty(ref _scripts, value);
		}
		
		protected Spawner()
		{
			ValidateScripts();
		}

		protected Spawner(XElement element)
		{
			Name = (string)element.Attribute("name");

			if (Int32.TryParse((string)element.Element("minimumDelay"), out var minSeconds))
				MinimumDelay = TimeSpan.FromSeconds(minSeconds);
			else
				MinimumDelay = TimeSpan.Zero;

			if (Int32.TryParse((string)element.Element("maximumDelay"), out var maxSeconds))
				MaximumDelay = TimeSpan.FromSeconds(maxSeconds);
			else
				MaximumDelay = TimeSpan.Zero;
			
			var maximumElement = element.Element("maximum");

			if (maximumElement != null)
				Maximum = (int)maximumElement;
			
			foreach (var scriptElement in element.Elements("script"))
				_scripts.Add(new Script(scriptElement));
			
			ValidateScripts();
		}

		private void ValidateScripts()
		{
			if (_scripts.All(s => s.Name != "OnAfterSpawn"))
			{
				_scripts.Add(new Script("OnAfterSpawn", false,
					String.Empty,
					"\n\n",
					String.Empty
				));
			}
			
			if (_scripts.All(s => s.Name != "OnBeforeSpawn"))
			{
				_scripts.Add(new Script("OnBeforeSpawn", false,
					String.Empty,
					"\n\n",
					String.Empty
				));
			}
			
			var provider = ServiceLocator.Current.GetInstance<ScriptTemplateProvider>();
			var attributes = GetType().GetCustomAttributes(typeof(ScriptTemplateAttribute), true)
				.OfType<ScriptTemplateAttribute>().ToList();

			if (attributes.Any())
			{
				foreach (var script in _scripts)
				{
					var attr = attributes.FirstOrDefault(
						a => String.Equals(a.Name, script.Name, StringComparison.Ordinal));

					if (attr != null && provider.TryGetTemplate(attr.TemplateType, out var template))
						script.Template = template;
				}
			}
		}

		public virtual XElement GetXElement()
		{
			var element = new XElement("spawn",
				new XAttribute("type", GetTypeAlias()),
				new XAttribute("name", _name),
				new XElement("minimumDelay", _minimumDelay.TotalSeconds),
				new XElement("maximumDelay", _maximumDelay.TotalSeconds));

			if (_maximum > 0)
				element.Add(new XElement("maximum", _maximum));
			
			foreach (var script in _scripts)
				element.Add(script.GetXElement());

			foreach (var entry in Entries)
			{
				if (entry.Entity != null)
					element.Add(entry.GetXElement());
			}

			return element;
		}
		
		protected virtual string GetTypeAlias()
		{
			return GetType().Name;
		}
	}

	[Obfuscation(Exclude = true, ApplyToMembers = false)]
	public class LocationSpawner : Spawner
	{
		private int _x;
		private int _y;
		private int _region;
		
		public int X
		{
			get => _x;
			set => SetProperty(ref _x, value);
		}

		public int Y
		{
			get => _y;
			set => SetProperty(ref _y, value);
		}
		
		public int Region
		{
			get => _region;
			set => SetProperty(ref _region, value);
		}

		public LocationSpawner()
		{
		}

		public LocationSpawner(XElement element) : base(element)
		{
			var locationElement = element.Element("location");

			if (locationElement != null)
			{
				X = (int)locationElement.Attribute("x");
				Y = (int)locationElement.Attribute("y");
				Region = (int)locationElement.Attribute("region");
			}
		}

		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			element.Add(new XElement("location",
				new XAttribute("x", _x),
				new XAttribute("y", _y),
				new XAttribute("region", _region))
			);
			
			return element;
		}
	}

	[Obfuscation(Exclude = true, ApplyToMembers = false)]
	public class RegionSpawner : Spawner
	{
		private int _region;

		private double _averageMobs;
		private int? _totalXP;
		private int? _totalHP;
		private int _openFloortiles;
		private int? _threat;
		
		public int Region
		{
			get => _region;
			set => SetProperty(ref _region, value);
		}
		
		public ObservableCollection<SegmentBounds> Inclusions { get; set; } = new ObservableCollection<SegmentBounds>();
		public ObservableCollection<SegmentBounds> Exclusions { get; set; } = new ObservableCollection<SegmentBounds>();

		public RegionSpawner()
		{
			Inclusions.Add(new SegmentBounds());
			Exclusions.Add(new SegmentBounds());
		}

		public RegionSpawner(XElement element)  : base(element)
		{
			var boundsElement = element.Element("bounds");
			
			if (boundsElement != null)
			{
				var regionAttribute = boundsElement.Attribute("region");
				
				if (regionAttribute != null)
					Region = (int)regionAttribute;
				
				foreach (var rectangleElement in boundsElement.Elements("inclusion"))
				{
					Inclusions.Add(new SegmentBounds(
						(int)rectangleElement.Attribute("left"), 
						(int)rectangleElement.Attribute("top"), 
						(int)rectangleElement.Attribute("right"), 
						(int)rectangleElement.Attribute("bottom")));
				}
				
				foreach (var rectangleElement in boundsElement.Elements("exclusion"))
				{
					Exclusions.Add(new SegmentBounds(
						(int)rectangleElement.Attribute("left"), 
						(int)rectangleElement.Attribute("top"), 
						(int)rectangleElement.Attribute("right"), 
						(int)rectangleElement.Attribute("bottom")));
				}
			}
		}

		public void CalculateStats()
        {
			//Calculate Open Floor Tiles by finding all tiles that have a floor type component and not a structural component.
			_openFloortiles = 0;
			var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
			var segment = segmentRequest.Response;
			var region = segment.GetRegion(_region);
			if (region is not null)
			{
				IEnumerable<SegmentTile> includedTiles = Enumerable.Empty<SegmentTile>();
				foreach (var rect in Inclusions)
				{
					var rectTiles = region.GetTiles(t => rect.ToRectangle().Contains(t.X, t.Y) &&
									   t.Components.Any(floor => floor is Models.FloorComponent || floor is Models.IceComponent || floor is Models.WaterComponent) &&
									   !t.Components.Any(notfloor => notfloor is Models.WallComponent || notfloor is Models.ObstructionComponent));
					includedTiles = includedTiles.Union(rectTiles);
				}
				foreach (var rect in Exclusions)
				{
					if (rect is { Left: 0, Right: 0, Top: 0, Bottom: 0 })
						continue;
					includedTiles = includedTiles.Where(t => !rect.ToRectangle().Contains(t.X, t.Y));
				}

				_openFloortiles = includedTiles.Count();

				//Calculate stats that require itterating through entities: Average mobs, total xp, total hp, density.
				_averageMobs = 0;
				_totalXP = 0;
				_totalHP = 0;
				if (Entries.Count != 0)
				{
					double averageEntry = 0;
					double averageXP = 0;
					double averageHP = 0;
					double averageThreat = 0;
					int minimumMobs = 0;
					int minimumXP = 0;
					int minimumHP = 0;
					int minimumSlots = 0;
					int maximumMobs = 0;
					int maximumXP = 0;
					int maximumHP = 0;
					int maximumSlots = 0;
					foreach (var entry in Entries.Where(e=>e.Entity.HP is not null && e.Entity.XP is not null))
					{
						averageEntry += entry.Size;
						averageHP += entry.Size * (int)entry.Entity.HP;
						averageXP += entry.Size * (int)entry.Entity.XP;
						averageThreat += Math.Max((int)(entry.Entity.Threat.Item1 is null ? 0 : entry.Entity.Threat.Item1), (int)(entry.Entity.Threat.Item2 is null ? 0 : entry.Entity.Threat.Item2));
						minimumMobs += entry.Minimum * entry.Size;
						minimumHP += entry.Minimum * entry.Size * (int)entry.Entity.HP;
						minimumXP += entry.Minimum * entry.Size * (int)entry.Entity.XP;
						minimumSlots += entry.Minimum;
						maximumMobs += entry.Maximum * entry.Size;
						maximumHP += entry.Maximum * entry.Size * (int)entry.Entity.HP;
						maximumXP += entry.Maximum * entry.Size * (int)entry.Entity.XP;
						maximumSlots += entry.Maximum ==0 ? Maximum : entry.Maximum;

					}
					averageEntry = averageEntry / Entries.Count();
					averageHP = averageHP / Entries.Count();
					averageXP = averageXP / Entries.Count();
					averageThreat = averageThreat / Entries.Count();

					//For spawners that have a maximum of 0 or a Maximum higher than the alotted entries, there is no cap on mobs, and all entries will spawn their maximum
					if (Maximum == 0 || Maximum>=maximumSlots)
					{
						_averageMobs = maximumMobs;
						_totalHP = maximumHP;
						_totalXP = maximumXP;
					}
					//For spawners that have fewer slots available than needed to fill the minimums, average mobs is just slots times our average size.
					else if (Maximum < minimumSlots)
					{
						_averageMobs = averageEntry * Maximum;
						_totalHP = (int)(averageHP * Maximum);
						_totalXP = (int)(averageXP * Maximum);
					}
					//For other spawners, we need to first fill the minimums, then add remaining slots * our average.
					else
					{
						_averageMobs = minimumMobs + (Maximum - minimumSlots) * averageEntry;
						_totalHP = (int)(minimumHP + (Maximum - minimumSlots) * averageHP);
						_totalXP = (int)(minimumXP + (Maximum - minimumSlots) * averageXP);
					}
					_threat = Math.Max((int)(Density * 10 * averageThreat),(int)averageThreat);
				}
				//If any of the entities referenced have a null for HP or XP, invalidate that calculation
				if (Entries.Any(e => e.Entity.HP is null))
					_totalHP = null;
				if (Entries.Any(e => e.Entity.XP is null))
					_totalXP = null;
				//If the Spawner has a 0 Maximum AND any of the entities also have a 0 maximum. I think this spawner will spawn without cap. Show as broken
				if (Maximum == 0 && Entries.Any(e => e.Maximum == 0))
                {
					_averageMobs = 0;
					_totalHP = null;
					_totalXP = null;
                }
			}
		}
		public int OpenFloorTiles
		{
			get => _openFloortiles;
		}

		public double AverageMobs
        {
			get => _averageMobs;
        }

		[Description("Approximate mobs per open floor tile")]
		public double Density
        {
            get
            {
				return _averageMobs / _openFloortiles;
            }
        }

		[Description("Average XP for a full clear of the spawn")]
		public int? TotalXP
        {
			get => _totalXP;
        }

		[Description("Average damage dealt required to clear the spawn")]
		public int? TotalHP
        {
			get => _totalHP;
        }
		
		[Description("Damage output score for the spawn. Does not take into account custom damage values. Gets wonky when evaluating lairs.")]
		public int? Threat
        {
			get => _threat;
        }

		public override XElement GetXElement()
		{
			var element = base.GetXElement();
			var bounds = new XElement("bounds",
				new XAttribute("region", _region));
			
			foreach (var rectangle in Inclusions.Where(r => r.IsValid))
			{
				bounds.Add(new XElement("inclusion",
					new XAttribute("left", rectangle.Left),
					new XAttribute("top", rectangle.Top),
					new XAttribute("right", rectangle.Right),
					new XAttribute("bottom", rectangle.Bottom)));
			}
			
			foreach (var rectangle in Exclusions.Where(r => r.IsValid))
			{
				bounds.Add(new XElement("exclusion",
					new XAttribute("left", rectangle.Left),
					new XAttribute("top", rectangle.Top),
					new XAttribute("right", rectangle.Right),
					new XAttribute("bottom", rectangle.Bottom)));
			}

			element.Add(bounds);
			
			return element;
		}
	}

	public class SpawnEntry : ObservableObject
	{
		private Entity _entity;
		
		private int _size;
		private int _minimum;
		private int _maximum;
		
		public Entity Entity
		{
			get => _entity;
			set => SetProperty(ref _entity, value);
		}
		
		public int Size
		{
			get => _size;
			set => SetProperty(ref _size, value);
		}
		
		public int Minimum
		{
			get => _minimum;
			set => SetProperty(ref _minimum, value);
		}
		
		public int Maximum
		{
			get => _maximum;
			set => SetProperty( ref _maximum, value);
		}

		public SpawnEntry()
		{
			Size = 1;
		}

		public SpawnEntry(XElement element)
		{
			Size = (int)element.Attribute("size");
			
			var minimumAttribute = element.Attribute("minimum");
			var maximumAttribute = element.Attribute("maximum");

			if (minimumAttribute != null)
				Minimum = (int)minimumAttribute;
			
			if (maximumAttribute != null)
				Maximum = (int)maximumAttribute;
		}

		public XElement GetXElement()
		{
			return new XElement("entry",
				new XAttribute("entity", _entity.Name),
				new XAttribute("size", _size),
				new XAttribute("minimum", _minimum),
				new XAttribute("maximum", _maximum));
		}
	}
}