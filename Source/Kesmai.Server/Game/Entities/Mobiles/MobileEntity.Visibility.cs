using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game;

public abstract partial class MobileEntity
{
	private static readonly Regex _filterTarget = new Regex(@"^@(\w*)(\[(.*?)\])?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private static readonly Dictionary<string, Action<MobileEntity, List<MobileEntity>>> _basicFilters = new()
	{
		["hostile"] = (source, entities) => entities.RemoveAll(entity => !source.IsHostile(entity)),
		["friendly"] = (source, entities) => entities.RemoveAll(entity => source.IsHostile(entity)),
		
		["pc"] = (source, entities) => entities.RemoveAll(entity => (entity is not PlayerEntity)),
		["npc"] = (source, entities) => entities.RemoveAll(entity => (entity is not CreatureEntity)),
		["conjured"] = (source, entities) => entities.RemoveAll(entity => entity is CreatureEntity { IsSubordinate: false }),
		
		["injured"] = (source, entities) => entities.RemoveAll(entity => (entity.Health != entity.MaxHealth)),
		["healthy"] = (source, entities) => entities.RemoveAll(entity => (entity.Health < entity.MaxHealth)),
		["deathly"] = (source, entities) => entities.RemoveAll(entity => (Combat.GetHealthState(entity) != 1)),
		
		["casting"] = (source, entities) => entities.RemoveAll(entity => (entity.Spell is null)), 

		["melee"] = (source, entities) => entities.RemoveAll(entity => (entity.GetWeapon() is not MeleeWeapon)),
		["ranged"] = (source, entities) => entities.RemoveAll(entity => (entity.GetWeapon() is not ProjectileWeapon)),
		
		["near"] = (source, entities) => entities.RemoveAll(entity => source.GetDistanceToMax(entity.Location) > 0),
		["distant"] = (source, entities) => entities.RemoveAll(entity => source.GetDistanceToMax(entity.Location) is 0),
		["far"] = (source, entities) => entities.RemoveAll(entity => source.GetDistanceToMax(entity.Location) < 3),
		
		["poisoned"] = (source, entities) => entities.RemoveAll(entity => !entity.IsPoisoned),
		["feared"] = (source, entities) => entities.RemoveAll(entity => !entity.IsFeared),
		["stunned"] = (source, entities) => entities.RemoveAll(entity => !entity.IsStunned && !entity.IsDazed),
		["blind"] = (source, entities) => entities.RemoveAll(entity => !entity.IsBlind),
	};
	
	private static readonly Dictionary<Regex, Action<Match, MobileEntity, List<MobileEntity>>> _advancedFilters = new()
	{
		// distance(value) - includes entities at the specified distance.
		[new Regex(@"^distance\((\w*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = (match, source, entities) =>
		{
			if (Int32.TryParse(match.Groups[1].Value, out int value))
				entities.RemoveAll(e => source.GetDistanceToMax(e.Location) != value);
		},
		
		// serial(value) - finds the entity by serial.
		[new Regex(@"^serial\((\w*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = (match, source, entities) =>
		{
			if (Int32.TryParse(match.Groups[1].Value, out int value))
				entities.RemoveAll(e => e.Serial.Value != value);
		},
		
		// index(value) - finds the entity by index or specifier.
		[new Regex(@"^index\((\w*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = (match, source, entities) =>
		{
			var indexValue = match.Groups[1].Value;

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
					entities = [entity];
			}
		}, 
	};

	/// <summary>
	/// Represents a filter condition with optional negation and OR grouping
	/// </summary>
	private class FilterCondition
	{
		public string FilterName { get; set; }
		public bool IsNegated { get; set; }
		public bool IsOrGroup { get; set; }
	}

	/// <summary>
	/// Parses filter string into individual conditions, handling | (or) and ! (not) operators
	/// </summary>
	private static List<FilterCondition> ParseFilterConditions(string filterString)
	{
		var conditions = new List<FilterCondition>();
		var parts = filterString.Split('|', StringSplitOptions.RemoveEmptyEntries);
		
		foreach (var part in parts)
		{
			var trimmedPart = part.Trim();
			var isNegated = trimmedPart.StartsWith('!');
			var filterName = isNegated ? trimmedPart.Substring(1).Trim() : trimmedPart;
			
			conditions.Add(new FilterCondition
			{
				FilterName = filterName,
				IsNegated = isNegated,
				IsOrGroup = parts.Length > 1
			});
		}
		
		return conditions;
	}

	/// <summary>
	/// Applies a single filter condition to the entities list
	/// </summary>
	private static void ApplyFilterCondition(FilterCondition condition, MobileEntity source, List<MobileEntity> entities)
	{
		// Check basic filters first
		if (_basicFilters.TryGetValue(condition.FilterName.ToLower(), out var basicFilter))
		{
			if (condition.IsNegated)
			{
				// For negated filters, we need to invert the logic
				// Store original entities and apply inverse filter
				var originalEntities = entities.ToList();
				basicFilter(source, entities);
				var removedEntities = originalEntities.Except(entities).ToList();
				entities.Clear();
				entities.AddRange(removedEntities);
			}
			else
			{
				basicFilter(source, entities);
			}
			return;
		}

		// Check advanced filters
		foreach (var (filterRegex, function) in _advancedFilters)
		{
			if (!filterRegex.TryGetMatch(condition.FilterName.ToLower(), out var advancedFilters)) 
				continue;
			
			if (condition.IsNegated)
			{
				// For negated advanced filters, we need to invert the logic
				var originalEntities = entities.ToList();
				function(advancedFilters, source, entities);
				var removedEntities = originalEntities.Except(entities).ToList();
				entities.Clear();
				entities.AddRange(removedEntities);
			}
			else
			{
				function(advancedFilters, source, entities);
			}
			return;
		}
	}

	/// <summary>
	/// Finds an entity with the specified name reference.
	/// </summary>
	public MobileEntity FindMobileByName(string name)
	{
		// get all the visible entities.
		var entities = GetBeheldInVisibility().SelectMany(g => g.Members)
			.OrderBy(m => m, new MobileDistanceComparer(this)).ToList();

		return FindMobileByName(name, entities);
	}

	/// <summary>
	/// Finds an entity with the specified name reference.
	/// </summary>
	public MobileEntity FindMobileByName(string name, List<MobileEntity> entities)
	{
		// the client sends reference in the form of "@name[filter]"
		if (_filterTarget.TryGetMatch(name, out var filterTargetMatch))
		{
			var targetFilters = filterTargetMatch.Groups[3].Value.Split(":");

			// apply filters.
			foreach (var filterString in targetFilters)
			{
				var conditions = ParseFilterConditions(filterString);
				
				if (conditions.Count == 1)
				{
					// Single condition - apply directly
					ApplyFilterCondition(conditions[0], this, entities);
				}
				else if (conditions.Count > 1)
				{
					// Multiple conditions with OR operator
					var orResults = new List<MobileEntity>();
					
					foreach (var condition in conditions)
					{
						var tempEntities = entities.ToList();
						ApplyFilterCondition(condition, this, tempEntities);
						orResults.AddRange(tempEntities);
					}
					
					// Keep only entities that match any of the OR conditions
					entities.Clear();
					entities.AddRange(orResults.Distinct());
				}
			}
			
			name = filterTargetMatch.Groups[1].Value;
		}
		
		// if a name is specified, filter the entities by name.
		if (!String.IsNullOrEmpty(name))
			entities.RemoveAll(entity => !entity.RespondsTo(name));
		
		// check if the last target matches the filters.
		var lastTarget = FindMostRecentTarget();

		if (entities.Contains(lastTarget))
			return lastTarget;
		
		// return the first entity that matches the filters.
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