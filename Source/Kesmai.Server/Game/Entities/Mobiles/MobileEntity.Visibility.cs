using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public abstract partial class MobileEntity
{
	private static readonly Regex _filterTarget = new Regex(@"^@(\w*)(\[(.*?)\])?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private static readonly Dictionary<string, Func<MobileEntity, List<MobileEntity>, IEnumerable<MobileEntity>>> _basicFilters = new()
	{
		["hostile"] = (source, entities) => entities.Where(entity => source.IsHostile(entity)),
		["friendly"] = (source, entities) => entities.Where(entity => !source.IsHostile(entity)),
		
		["player"] = (source, entities) => entities.Where(entity => (entity is PlayerEntity)),
		["creature"] = (source, entities) => entities.Where(entity => (entity is CreatureEntity)),
		
		["injured"] = (source, entities) => entities.Where(entity => (entity.Health < entity.MaxHealth)),
		["healthy"] = (source, entities) => entities.Where(entity => (entity.Health >= entity.MaxHealth)),
		["deathly"] = (source, entities) => entities.Where(entity => (Combat.GetHealthState(entity) is 1)),
		
		["casting"] = (source, entities) => entities.Where(entity => (entity.Spell != null)), 

		["melee"] = (source, entities) => entities.Where(entity => (entity.GetWeapon() is MeleeWeapon)),
		["ranged"] = (source, entities) => entities.Where(entity => (entity.GetWeapon() is ProjectileWeapon)),
		
		["poisoned"] = (source, entities) => entities.Where(entity => entity.IsPoisoned),
		["feared"] = (source, entities) => entities.Where(entity => entity.IsFeared),
		["stunned"] = (source, entities) => entities.Where(entity => entity.IsStunned || entity.IsDazed),
		["blind"] = (source, entities) => entities.Where(entity => entity.IsBlind),
	};
	
	private static readonly Dictionary<Regex, Func<Match, MobileEntity, List<MobileEntity>, IEnumerable<MobileEntity>>> _advancedFilters = new()
	{
		// serial(value) - finds the entity by serial.
		[new Regex(@"^serial\((\w*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = (match, source, entities) =>
		{
			var filtered = new List<MobileEntity>();
			
			if (int.TryParse(match.Groups[1].Value, out int value))
			{
				var entity = entities.FirstOrDefault(e => e.Serial.Value == value);

				if (entity != null)
					filtered.Add(entity);
			}

			return filtered;
		},
		
		// index(value) - finds the entity by index or specifier.
		[new Regex(@"^index\((\w*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = (match, source, entities) =>
		{
			var indexValue = match.Groups[1].Value;

			var filtered = new List<MobileEntity>();
			var index = 1; // index is 1-based for the user to prevent confusion.
			
			if (!Int32.TryParse(indexValue, out index) && entities.Any())
			{
				if (indexValue.Matches("last"))
					index = entities.Count();
			}
			
			if (index > 0 && index <= entities.Count)
			{
				var entity = entities[index - 1];

				if (entity != null)
					filtered.Add(entity);
			}

			return filtered;
		}, 
	};
	
	/// <summary>
	/// Finds an entity with the specified name reference.
	/// </summary>
	public MobileEntity FindMobileByName(string name)
	{
		// get all the visible entities.
		var entities = GetBeheldInVisibility().SelectMany(g => g.Members)
			.OrderBy(m => m, new MobileDistanceComparer(this)).ToList();
			
		// the client sends reference in the form of "@name[filter]"
		if (_filterTarget.TryGetMatch(name, out var filterTargetMatch))
		{
			var targetName = filterTargetMatch.Groups[1].Value;
			var targetFilters = filterTargetMatch.Groups[3].Value.Split(":");
			
			// if a name is specified, filter the entities by name.
			if (!String.IsNullOrEmpty(targetName))
				entities = entities.Where(m => m.RespondsTo(targetName)).ToList();

			// apply filters.
			foreach (var filterName in targetFilters)
			{
				if (_basicFilters.TryGetValue(filterName.ToLower(), out var basicFilter))
				{
					entities = basicFilter(this, entities).ToList();
					continue;
				}

				foreach (var (filterRegex, function) in _advancedFilters)
				{
					if (!filterRegex.TryGetMatch(filterName.ToLower(), out var advancedFilters)) 
						continue;
					
					entities = function(advancedFilters, this, entities).ToList();
					break;
				}
			}

			// apply the "last" filter at the end. If last is not found, we default to the filtered.
			if (targetFilters.Contains("last"))
			{
				var lastTarget = FindMostRecentTarget();
				
				if (lastTarget != null && entities.Contains(lastTarget))
					entities = [lastTarget];
			}
		}
		else
		{
			// if no filters are specified, return the first entity that matches the name.
			entities = entities.Where(entity => entity.RespondsTo(name)).ToList();
		}
		
		if (entities.Any())
			return entities.FirstOrDefault();

		return default(MobileEntity);
	}

	public IEnumerable<ItemEntity> FindItemsByName(string name)
	{
		var segment = Segment;
		var location = Location;
		var mapTile = segment.FindTile(location);

		if (mapTile != null && mapTile.Items.Any())
		{
			foreach (var item in mapTile.Items.Where(item => item.RespondsTo(name)))
				yield return item;
		}
	}

	/// <summary>
	/// Determines whether this instance responds to the specified noun.
	/// </summary>
	public virtual bool RespondsTo(string noun)
	{
		if (String.IsNullOrEmpty(noun))
			return false;
		
		var name = Name;

		if (name is null)
		{
			Log.Warn($"Warning: Entity with invalid name in {Facet.Name} {Segment.Name} at {Location}.");
			return false;
		}
			
		var matchesName = name.Length >= noun.Length
		                  && name.Substring(0, noun.Length).Matches(noun, true);

		return matchesName;
	}
}

public class MobileDistanceComparer : IComparer<MobileEntity>
{
	private readonly MobileEntity _source;
	
	public MobileDistanceComparer(MobileEntity source)
	{
		_source = source;
	}
	
	public int Compare(MobileEntity xEntity, MobileEntity yEntity)
	{
		if (xEntity is null || yEntity is null) 
			return 0;
		
		var xDistance = _source.GetDistanceToMax(xEntity.Location);
		var yDistance = _source.GetDistanceToMax(yEntity.Location);

		if (xDistance > yDistance)
			return 1;

		if (xDistance < yDistance)
			return -1;

		return 0;
	}
}