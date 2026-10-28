using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.UI.Documents;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge;

public class SegmentSpawnChanged(SegmentSpawner segmentSpawner) : ValueChangedMessage<SegmentSpawner>(segmentSpawner);
	
[Script("OnBeforeSpawn", "void OnBeforeSpawn(Spawner spawner)", "{", "}")]
[Script("OnAfterSpawn", "void OnAfterSpawn(Spawner spawner, MobileEntity spawn)", "{", "}")]
public abstract class SegmentSpawner : ObservableObject, ICloneable, ISegmentObject
{
	private string _name;
	private bool _enabled;

	private TimeSpan _minimumDelay;
	private TimeSpan _maximumDelay;

	private int _maximum;
		
	private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();

	[Category("Identity")]
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentSpawnChanged(this));
		}
	}

    public bool Enabled
	{
		get => _enabled;
		set => SetProperty(ref _enabled, value);
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

	// Spawn time: kept in the OnAfterSpawn script (see SpawnTimeWindow), shown here so it can be set without code. While
	// it is set to Always the script holds nothing, so the other choices are remembered here until a mode is picked.
	private SpawnTimeWindow _unsavedSpawnTime = new SpawnTimeWindow();

	private Script AfterSpawnScript => _scripts.First(s => s.Name == "OnAfterSpawn");

	private SpawnTimeWindow CurrentSpawnTime
	{
		get
		{
			var written = SpawnTimeWindow.Read(AfterSpawnScript);
			return written.IsAlways ? _unsavedSpawnTime : written;
		}
	}

	private void ChangeSpawnTime(Action<SpawnTimeWindow> change, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
	{
		var window = CurrentSpawnTime.Clone();

		change(window);
		window.Apply(AfterSpawnScript);

		if (window.IsAlways)
			_unsavedSpawnTime = window;

		OnPropertyChanged(propertyName);
		WeakReferenceMessenger.Default.Send(new SegmentSpawnChanged(this));
	}

	private bool PhaseAllowed(string phase) => CurrentSpawnTime.AllowedPhases.Contains(phase);

	private void SetPhase(string phase, bool allowed, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
		=> ChangeSpawnTime(w => { if (allowed) w.AllowedPhases.Add(phase); else w.AllowedPhases.Remove(phase); }, propertyName);

	// Each "Use" toggle is optional. With none on the spawner works as normal; with several on, a spawn needs all of them.

	[Category("Spawn time")]
	[DisplayName("Use light phases")]
	[PropertyOrder(0)]
	[Description("Optional. On: the spawner only spawns during the in-game light phases ticked below. Off: the time of day in the game doesn't matter. Can be combined with the other Use options; then every one that is on must allow the spawn.")]
	public bool UseLightPhases
	{
		get => CurrentSpawnTime.UsePhases;
		set => ChangeSpawnTime(w => w.UsePhases = value);
	}

	[Category("Spawn time")]
	[DisplayName("Phase: Dawn")]
	[PropertyOrder(1)]
	[Description("With Use light phases on: may spawn at dawn, the first light of the in-game day.")]
	public bool SpawnAtDawn { get => PhaseAllowed("Dawn"); set => SetPhase("Dawn", value); }

	[Category("Spawn time")]
	[DisplayName("Phase: Sunrise")]
	[PropertyOrder(2)]
	[Description("With Use light phases on: may spawn at sunrise, between dawn and daytime.")]
	public bool SpawnAtSunrise { get => PhaseAllowed("Sunrise"); set => SetPhase("Sunrise", value); }

	[Category("Spawn time")]
	[DisplayName("Phase: Daytime")]
	[PropertyOrder(3)]
	[Description("With Use light phases on: may spawn in full daytime.")]
	public bool SpawnAtDaytime { get => PhaseAllowed("Daytime"); set => SetPhase("Daytime", value); }

	[Category("Spawn time")]
	[DisplayName("Phase: Sunset")]
	[PropertyOrder(4)]
	[Description("With Use light phases on: may spawn at sunset, as daytime ends.")]
	public bool SpawnAtSunset { get => PhaseAllowed("Sunset"); set => SetPhase("Sunset", value); }

	[Category("Spawn time")]
	[DisplayName("Phase: Midnight")]
	[PropertyOrder(5)]
	[Description("With Use light phases on: may spawn at midnight, the darkest part of the in-game night.")]
	public bool SpawnAtMidnight { get => PhaseAllowed("Midnight"); set => SetPhase("Midnight", value); }

	[Category("Spawn time")]
	[DisplayName("Phase: Nighttime")]
	[PropertyOrder(6)]
	[Description("With Use light phases on: may spawn at night. Tick Midnight as well for the whole night.")]
	public bool SpawnAtNighttime { get => PhaseAllowed("Nighttime"); set => SetPhase("Nighttime", value); }

	[Category("Spawn time")]
	[DisplayName("Use server clock window")]
	[PropertyOrder(7)]
	[Description("Optional. On: the spawner only spawns between From and Until on the server's real clock (24-hour, the server's time zone). Off: the real time doesn't matter.")]
	public bool UseServerClockWindow
	{
		get => CurrentSpawnTime.UseClock;
		set => ChangeSpawnTime(w => w.UseClock = value);
	}

	[Category("Spawn time")]
	[DisplayName("Server clock: From")]
	[PropertyOrder(8)]
	[Description("With Use server clock window on: the time the window opens, typed as 20:00. Spawns from this time on.")]
	public string SpawnFrom
	{
		get => CurrentSpawnTime.From.ToString(@"hh\:mm");
		set
		{
			if (SpawnTimeWindow.ParseTime(value) is TimeSpan time)
				ChangeSpawnTime(w => w.From = time);
		}
	}

	[Category("Spawn time")]
	[DisplayName("Server clock: Until")]
	[PropertyOrder(9)]
	[Description("With Use server clock window on: the time the window closes, typed as 02:00. Earlier than From means the window runs past midnight (20:00 to 02:00); the same as From means all day.")]
	public string SpawnUntil
	{
		get => CurrentSpawnTime.Until.ToString(@"hh\:mm");
		set
		{
			if (SpawnTimeWindow.ParseTime(value) is TimeSpan time)
				ChangeSpawnTime(w => w.Until = time);
		}
	}

	[Category("Spawn time")]
	[DisplayName("Use server times")]
	[PropertyOrder(10)]
	[Description("Optional. On: the spawner spawns only just after each of the listed server times, once a day per time. Off: it isn't tied to set times.")]
	public bool UseServerTimes
	{
		get => CurrentSpawnTime.UseTimes;
		set => ChangeSpawnTime(w => w.UseTimes = value);
	}

	[Category("Spawn time")]
	[DisplayName("Server times")]
	[PropertyOrder(11)]
	[System.ComponentModel.Editor(typeof(Kesmai.WorldForge.UI.TimeListEditor), typeof(Kesmai.WorldForge.UI.TimeListEditor))]
	[Description("With Use server times on: the times of day it spawns at (24-hour, server time). + adds a time, − removes one; click a time to type a new one, e.g. 20:00.")]
	public string SpawnTimes
	{
		get => SpawnTimeWindow.FormatTimes(CurrentSpawnTime.Times);
		set => ChangeSpawnTime(w =>
		{
			w.Times.Clear();
			w.Times.AddRange(SpawnTimeWindow.ParseTimes(value));
		});
	}

	[Category("Spawn time")]
	[DisplayName("Server times: per time")]
	[PropertyOrder(12)]
	[Description("With Use server times on: how many creatures it may spawn for each listed time, e.g. 3 for a pack of three at 20:00.")]
	public int SpawnsPerTime
	{
		get => CurrentSpawnTime.PerTime;
		set => ChangeSpawnTime(w => w.PerTime = Math.Max(1, value));
	}

	[Category("Spawn time")]
	[DisplayName("Server times: catch window (min)")]
	[PropertyOrder(13)]
	[Description("With Use server times on: how many minutes after each time it may still spawn. The spawner only tries every MinimumDelay to MaximumDelay, so make this longer than MaximumDelay; it also covers a server restart.")]
	public int SpawnWindowMinutes
	{
		get => CurrentSpawnTime.WindowMinutes;
		set => ChangeSpawnTime(w => w.WindowMinutes = Math.Max(1, value));
	}

	[Category("Spawn time")]
	[DisplayName("Remove all outside the window")]
	[PropertyOrder(14)]
	[Description("Optional, with any Use option on. Ticked: when a spawn isn't allowed, everything this spawner made is removed. Unticked: only the new spawn is removed, and creatures already out stay until killed.")]
	public bool RemoveAllOutsideWindow
	{
		get => CurrentSpawnTime.RemoveAll;
		set => ChangeSpawnTime(w => w.RemoveAll = value);
	}

	// Broadcast on spawn: also kept in the OnAfterSpawn script (see SpawnBroadcast), remembered here while it is off.
	private SpawnBroadcast _unsavedBroadcast = new SpawnBroadcast();

	private SpawnBroadcast CurrentBroadcast
	{
		get
		{
			var written = SpawnBroadcast.Read(AfterSpawnScript);
			return written.Enabled ? written : _unsavedBroadcast;
		}
	}

	private void ChangeBroadcast(Action<SpawnBroadcast> change, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
	{
		var broadcast = CurrentBroadcast.Clone();

		change(broadcast);
		broadcast.Apply(AfterSpawnScript);

		if (!broadcast.Enabled)
			_unsavedBroadcast = broadcast;

		OnPropertyChanged(propertyName);
		WeakReferenceMessenger.Default.Send(new SegmentSpawnChanged(this));
	}

	[Category("Broadcast on spawn")]
	[DisplayName("Broadcast on spawn")]
	[PropertyOrder(0)]
	[Description("Optional. On: each time this spawner spawns a creature, the message below is sent to every player in the chosen area. Off: nothing is announced. With Spawn time options on, only spawns they allow are announced.")]
	public bool BroadcastOnSpawn
	{
		get => CurrentBroadcast.Enabled;
		set => ChangeBroadcast(b => b.Enabled = value);
	}

	[Category("Broadcast on spawn")]
	[DisplayName("Message")]
	[PropertyOrder(1)]
	[Description("What players see, in gold. {name} becomes the spawned creature's name, e.g. \"{name} has risen in the crypt!\".")]
	public string BroadcastMessage
	{
		get => CurrentBroadcast.Message;
		set => ChangeBroadcast(b => b.Message = value ?? String.Empty);
	}

	[Category("Broadcast on spawn")]
	[DisplayName("Who hears it")]
	[PropertyOrder(2)]
	[Description("Segment: every player in this segment. Region: every player in the region the creature spawned in. Subregion: only players inside the subregion picked below, in that region.")]
	public BroadcastScope BroadcastScope
	{
		get => CurrentBroadcast.Scope;
		set => ChangeBroadcast(b => b.Scope = value);
	}

	[Category("Broadcast on spawn")]
	[DisplayName("Subregion")]
	[PropertyOrder(3)]
	[System.ComponentModel.Editor(typeof(Kesmai.WorldForge.UI.SubregionNameEditor), typeof(Kesmai.WorldForge.UI.SubregionNameEditor))]
	[Description("With Who hears it set to Subregion: the subregion whose players hear the message. The list shows this segment's subregions; the subregion must be in the region the creature spawns in.")]
	public string BroadcastSubregion
	{
		get => CurrentBroadcast.Subregion;
		set => ChangeBroadcast(b => b.Subregion = value ?? String.Empty);
	}

	// Spawn after kills: part of the spawn time block, plus a counter (KillCountHook) on the enemy's OnDeath script while
	// any spawner in the segment uses that enemy.
	private void ChangeKills(Action<SpawnTimeWindow> change, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
	{
		var before = CurrentSpawnTime.UseKills ? CurrentSpawnTime.KillEnemy : null;

		ChangeSpawnTime(change, propertyName);

		var after = CurrentSpawnTime.UseKills ? CurrentSpawnTime.KillEnemy : null;

		foreach (var enemy in new[] { before, after }.Where(e => !String.IsNullOrWhiteSpace(e)).Distinct())
			SyncKillCounter(enemy);
	}

	/// <summary>Adds the counter to the enemy's OnDeath while a spawner in the segment needs it, and takes it out once none does.</summary>
	private void SyncKillCounter(string enemy)
	{
		var request = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
		var segment = request.HasReceivedResponse ? request.Response : null;
		var entity = segment?.Entities.FirstOrDefault(e => String.Equals(e.Name, enemy.Trim(), StringComparison.Ordinal));
		var onDeath = entity?.Scripts.FirstOrDefault(s => s.Name == "OnDeath");

		if (segment is null || entity is null || onDeath is null)
			return;

		var spawners = segment.Spawns.Location.Cast<SegmentSpawner>().Concat(segment.Spawns.Region).Append(this).Distinct();
		var needed = spawners.Any(s => s.CurrentSpawnTime is { UseKills: true } w && String.Equals(w.KillEnemy.Trim(), enemy.Trim(), StringComparison.Ordinal));
		var installed = KillCountHook.IsInstalled(onDeath);

		if (needed == installed)
			return;

		if (needed)
			KillCountHook.Install(onDeath, enemy);
		else
			KillCountHook.Remove(onDeath);

		WeakReferenceMessenger.Default.Send(new SegmentEntityChanged(entity));
	}

	[Category("Spawn after kills")]
	[DisplayName("Spawn after kills")]
	[PropertyOrder(0)]
	[Description("Optional. On: the spawner only spawns once players have killed the enemy below enough times since its last spawn. WorldForge adds a small kill counter to that enemy's OnDeath script, and takes it out again when no spawner uses that enemy. Works together with the Spawn time options: all that are on must allow the spawn.")]
	public bool SpawnAfterKills
	{
		get => CurrentSpawnTime.UseKills;
		set => ChangeKills(w => w.UseKills = value);
	}

	[Category("Spawn after kills")]
	[DisplayName("Enemy")]
	[PropertyOrder(1)]
	[System.ComponentModel.Editor(typeof(Kesmai.WorldForge.UI.EntityNameEditor), typeof(Kesmai.WorldForge.UI.EntityNameEditor))]
	[Description("The entity whose deaths count, picked from this segment's entities (a name can be typed too). Its OnDeath script gets the counter.")]
	public string KillEnemy
	{
		get => CurrentSpawnTime.KillEnemy;
		set => ChangeKills(w => w.KillEnemy = (value ?? String.Empty).Trim());
	}

	[Category("Spawn after kills")]
	[DisplayName("Kills needed")]
	[PropertyOrder(2)]
	[Description("How many of the enemy must die before each spawn. After a spawn the count starts again, so the next spawn needs this many more kills.")]
	public int KillsNeeded
	{
		get => CurrentSpawnTime.KillsNeeded;
		set => ChangeKills(w => w.KillsNeeded = Math.Max(1, value));
	}

	[Category("Spawn after kills")]
	[DisplayName("Count kills in")]
	[PropertyOrder(3)]
	[Description("Segment: deaths of the enemy anywhere in this segment count. Region: only deaths in the region this spawner spawns in count.")]
	public KillCountArea KillsCountedIn
	{
		get => CurrentSpawnTime.KillsInRegion ? KillCountArea.Region : KillCountArea.Segment;
		set => ChangeKills(w => w.KillsInRegion = value == KillCountArea.Region);
	}

	// Sound on spawn: also kept in the OnAfterSpawn script (see SpawnSound), remembered here while it is off.
	private SpawnSound _unsavedSound = new SpawnSound();

	private SpawnSound CurrentSound
	{
		get
		{
			var written = SpawnSound.Read(AfterSpawnScript);
			return written.Enabled ? written : _unsavedSound;
		}
	}

	private void ChangeSound(Action<SpawnSound> change, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
	{
		var sound = CurrentSound.Clone();

		change(sound);
		sound.Apply(AfterSpawnScript);

		if (!sound.Enabled)
			_unsavedSound = sound;

		OnPropertyChanged(propertyName);
		WeakReferenceMessenger.Default.Send(new SegmentSpawnChanged(this));
	}

	[Category("Sound on spawn")]
	[DisplayName("Sound on spawn")]
	[PropertyOrder(0)]
	[Description("Optional. On: each time this spawner spawns a creature, the sound below plays. Off: no sound. With Spawn time options on, only spawns they allow make the sound.")]
	public bool SoundOnSpawn
	{
		get => CurrentSound.Enabled;
		set => ChangeSound(s => s.Enabled = value);
	}

	[Category("Sound on spawn")]
	[DisplayName("Sound")]
	[PropertyOrder(1)]
	[System.ComponentModel.Editor(typeof(Kesmai.WorldForge.UI.SoundIdEditor), typeof(Kesmai.WorldForge.UI.SoundIdEditor))]
	[Description("The sound to play: pick one from the repo's sound list (Audio-External.xml) or type a sound id. ▶ plays it here when the repo has its file.")]
	public int SpawnSoundId
	{
		get => CurrentSound.SoundId;
		set => ChangeSound(s => s.SoundId = value);
	}

	[Category("Sound on spawn")]
	[DisplayName("Who hears it")]
	[PropertyOrder(2)]
	[Description("Nearby: anyone within Range tiles of the creature, the way creatures make their own sounds. Segment: every player in the segment. Region: every player in the region it spawned in. Subregion: only players inside the subregion picked below.")]
	public SoundScope SpawnSoundScope
	{
		get => CurrentSound.Scope;
		set => ChangeSound(s => s.Scope = value);
	}

	[Category("Sound on spawn")]
	[DisplayName("Range (tiles)")]
	[PropertyOrder(3)]
	[Description("With Who hears it set to Nearby: how many tiles away the sound can be heard. Creature scripts mostly use 3.")]
	public int SpawnSoundRange
	{
		get => CurrentSound.Range;
		set => ChangeSound(s => s.Range = Math.Max(0, value));
	}

	[Category("Sound on spawn")]
	[DisplayName("Volume")]
	[PropertyOrder(4)]
	[Description("How loud it plays, on the same scale scripts use with EmitSound; they mostly use 9.")]
	public int SpawnSoundVolume
	{
		get => CurrentSound.Volume;
		set => ChangeSound(s => s.Volume = Math.Clamp(value, 0, 255));
	}

	[Category("Sound on spawn")]
	[DisplayName("Subregion")]
	[PropertyOrder(5)]
	[System.ComponentModel.Editor(typeof(Kesmai.WorldForge.UI.SubregionNameEditor), typeof(Kesmai.WorldForge.UI.SubregionNameEditor))]
	[Description("With Who hears it set to Subregion: the subregion whose players hear the sound. It must be in the region the creature spawns in.")]
	public string SpawnSoundSubregion
	{
		get => CurrentSound.Subregion;
		set => ChangeSound(s => s.Subregion = value ?? String.Empty);
	}

	[Browsable(false)]
	public ObservableCollection<SpawnEntry> Entries { get; set; } = new ObservableCollection<SpawnEntry>();

	[Browsable(false)]
	public ObservableCollection<Script> Scripts
	{
		get => _scripts;
		set => SetProperty(ref _scripts, value);
	}

	protected SegmentSpawner(XElement element)
	{
		_name = (string)element.Attribute("name");
			
		if (Boolean.TryParse((string)element.Attribute("enabled"), out var enabled))
			_enabled = enabled;
		else
			_enabled = true;

		if (Int32.TryParse((string)element.Element("minimumDelay"), out var minSeconds))
			_minimumDelay = TimeSpan.FromSeconds(minSeconds);
		else
			_minimumDelay = TimeSpan.Zero;

		if (Int32.TryParse((string)element.Element("maximumDelay"), out var maxSeconds))
			_maximumDelay = TimeSpan.FromSeconds(maxSeconds);
		else
			_maximumDelay = TimeSpan.Zero;
			
		var maximumElement = element.Element("maximum");

		if (maximumElement != null)
			_maximum = (int)maximumElement;

		ValidateScripts(element);

		// spawn time blocks written by an older WorldForge are rewritten from their settings, so saving carries the current code
		var spawnTime = SpawnTimeWindow.Read(AfterSpawnScript);

		if (!spawnTime.IsAlways)
			spawnTime.Apply(AfterSpawnScript);
	}
	
	protected SegmentSpawner()
	{
		ValidateScripts();
	}

	private void ValidateScripts(XElement rootElement = default)
	{
		var attributes = GetType().GetCustomAttributes(typeof(ScriptAttribute), inherit: true)
			.Cast<ScriptAttribute>();

		var implementations = new Dictionary<string, Script>();

		if (rootElement != null)
		{
			foreach (var scriptElement in rootElement.Elements("script"))
			{
				var script = new Script(scriptElement);

				if (!implementations.TryAdd(script.Name, script))
					throw new InvalidOperationException($"Duplicate script name '{script.Name}'.");
			}
		}

		foreach (var attribute in attributes)
		{
			if (implementations.TryGetValue(attribute.Name, out var script))
			{
				script.Signature = attribute.Signature;
				script.Header = attribute.Header;
				script.Footer = attribute.Footer;
				
				_scripts.Add(script);
			}
			else
			{
				_scripts.Add(new Script
				{
					Name = attribute.Name,
					Signature = attribute.Signature,
					Header = attribute.Header,
					Body = attribute.Body,
					Footer = attribute.Footer
				});
			}
		}
	}
	
	public abstract void Present(ApplicationPresenter presenter);

	public virtual XElement GetSerializingElement()
	{
		var element = new XElement("spawn",
			new XAttribute("type", GetTypeAlias()),
			new XAttribute("name", _name),
			new XAttribute("enabled", _enabled),
			new XElement("minimumDelay", _minimumDelay.TotalSeconds),
			new XElement("maximumDelay", _maximumDelay.TotalSeconds));

		if (_maximum > 0)
			element.Add(new XElement("maximum", _maximum));
			
		foreach (var script in _scripts.Where(s => !s.IsEmpty))
			element.Add(script.GetSerializingElement());

		foreach (var entry in Entries)
		{
			if (entry.SegmentEntity != null)
				element.Add(entry.GetXElement());
		}

		return element;
	}
	
	public virtual XElement GetReferencingElement()
	{
		return new XElement("spawn",
			new XAttribute("name", _name));
	}
		
	protected virtual string GetTypeAlias()
	{
		return GetType().Name;
	}

	public override string ToString()
	{
		return Name;
	}

	public abstract void Copy(Segment segment);
	public abstract object Clone();
}

[Obfuscation(Exclude = true, ApplyToMembers = false)]
public class LocationSegmentSpawner : SegmentSpawner
{
	private int _x;
	private int _y;
	private int _region;
		
	[Category("Coordinates")]
	public int X
	{
		get => _x;
		set => SetProperty(ref _x, value);
	}

	[Category("Coordinates")]
	public int Y
	{
		get => _y;
		set => SetProperty(ref _y, value);
	}
		
	[Category("Coordinates")]
	public int Region
	{
		get => _region;
		set
		{
			if (SetProperty(ref _region, value))
				WeakReferenceMessenger.Default.Send(new SegmentSpawnChanged(this));
		}
	}

	public LocationSegmentSpawner() : base()
	{
	}

	public LocationSegmentSpawner(XElement element) : base(element)
	{
		var locationElement = element.Element("location");

		if (locationElement != null)
		{
			_x = (int)locationElement.Attribute("x");
			_y = (int)locationElement.Attribute("y");
			_region = (int)locationElement.Attribute("region");
		}
	}
	
	public override void Present(ApplicationPresenter presenter)
	{
		var locationSpawnViewModel = presenter.Documents.OfType<LocationSpawnViewModel>().FirstOrDefault();
		
		if (locationSpawnViewModel is null)
			presenter.Documents.Add(locationSpawnViewModel = new LocationSpawnViewModel());
		
		if (presenter.Segment is null)
			throw new InvalidOperationException("No segment is currently loaded.");

		locationSpawnViewModel.Region = presenter.Segment.GetRegion(Region);
		locationSpawnViewModel.Spawner = this;
		
		presenter.SetActiveDocument(locationSpawnViewModel, this);
	}
	
	public override void Copy(Segment target)
	{
		if (Clone() is LocationSegmentSpawner clonedSpawner)
			target.Spawns.Location.Add(clonedSpawner);
	}

	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		element.Add(new XElement("location",
			new XAttribute("x", _x),
			new XAttribute("y", _y),
			new XAttribute("region", _region))
		);
			
		return element;
	}

	public override object Clone()
	{
		return new LocationSegmentSpawner(GetSerializingElement())
		{
			Name = $"Copy of {Name}",
		};
	}
}

[Obfuscation(Exclude = true, ApplyToMembers = false)]
public class RegionSegmentSpawner : SegmentSpawner
{
	private int _region;
		
	[Category("Coordinates")]
	public int Region
	{
		get => _region;
		set
		{
			if (SetProperty(ref _region, value))
				WeakReferenceMessenger.Default.Send(new SegmentSpawnChanged(this));
		}
	}

	[Browsable(false)]
	public ObservableCollection<SegmentBounds> Inclusions { get; set; } = new ObservableCollection<SegmentBounds>();
	[Browsable(false)]
	public ObservableCollection<SegmentBounds> Exclusions { get; set; } = new ObservableCollection<SegmentBounds>();

	public RegionSegmentSpawner()
	{
		Inclusions.Add(new SegmentBounds());
		Exclusions.Add(new SegmentBounds());
	}

	public RegionSegmentSpawner(XElement element)  : base(element)
	{
		var boundsElement = element.Element("bounds");
			
		if (boundsElement != null)
		{
			var regionAttribute = boundsElement.Attribute("region");
				
			if (regionAttribute != null)
				_region = (int)regionAttribute;
				
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
	
	public override void Present(ApplicationPresenter presenter)
	{
		var regionSpawnViewModel = presenter.Documents.OfType<RegionSpawnViewModel>().FirstOrDefault();

		if (regionSpawnViewModel is null)
			presenter.Documents.Add(regionSpawnViewModel = new RegionSpawnViewModel());

		if (presenter.Segment is null)
			throw new InvalidOperationException("No segment is currently loaded.");

		regionSpawnViewModel.Region = presenter.Segment.GetRegion(Region);
		regionSpawnViewModel.Spawner = this;

		presenter.SetActiveDocument(regionSpawnViewModel, this);
	}
	
	public override void Copy(Segment target)
	{
		if (Clone() is RegionSegmentSpawner clonedSpawner)
			target.Spawns.Region.Add(clonedSpawner);
	}

	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();
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
	
	public override object Clone()
	{
		return new RegionSegmentSpawner(GetSerializingElement())
		{
			Name = $"Copy of {Name}",
		};
	}
}

public class SpawnEntry : ObservableObject
{
	private SegmentEntity _segmentEntity;
		
	private int _size;
	private int _minimum;
	private int _maximum;
		
	public SegmentEntity SegmentEntity
	{
		get => _segmentEntity;
		set => SetProperty(ref _segmentEntity, value);
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
			new XAttribute("entity", _segmentEntity.Name),
			new XAttribute("size", _size),
			new XAttribute("minimum", _minimum),
			new XAttribute("maximum", _maximum));
	}
}