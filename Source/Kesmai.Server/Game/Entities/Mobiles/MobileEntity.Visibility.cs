﻿using System;
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
	/// Represents a complete filter expression that can contain multiple AND conditions
	/// </summary>
	private class FilterExpression
	{
		public List<FilterCondition> AndConditions { get; set; } = new();
		public bool IsOrGroup { get; set; }
	}

	/// <summary>
	/// Parses filter string into expressions, handling | (or) and : (and) operators
	/// </summary>
	private static List<FilterExpression> ParseFilterExpressions(string filterString)
	{
		var expressions = new List<FilterExpression>();
		
		// First split by OR operator to get OR groups
		var orGroups = filterString.Split('|', StringSplitOptions.RemoveEmptyEntries);
		
		foreach (var orGroup in orGroups)
		{
			var trimmedGroup = orGroup.Trim();
			var expression = new FilterExpression
			{
				IsOrGroup = orGroups.Length > 1
			};
			
			// Within each OR group, split by AND operator (colon)
			var andConditions = trimmedGroup.Split(':', StringSplitOptions.RemoveEmptyEntries);
			
			foreach (var condition in andConditions)
			{
				var trimmedCondition = condition.Trim();
				var isNegated = trimmedCondition.StartsWith('!');
				var filterName = isNegated ? trimmedCondition.Substring(1).Trim() : trimmedCondition;
				
				expression.AndConditions.Add(new FilterCondition
				{
					FilterName = filterName,
					IsNegated = isNegated,
					IsOrGroup = false // AND conditions are never OR groups
				});
			}
			
			expressions.Add(expression);
		}
		
		return expressions;
	}

	/// <summary>
	/// Applies a single filter expression (AND group) to the entities list
	/// </summary>
	private static void ApplyFilterExpression(FilterExpression expression, MobileEntity source, List<MobileEntity> entities)
	{
		// Apply all AND conditions sequentially
		foreach (var condition in expression.AndConditions)
		{
			ApplyFilterCondition(condition, source, entities);
		}
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
			var filterString = filterTargetMatch.Groups[3].Value;

			// Parse the filter string into expressions
			var expressions = ParseFilterExpressions(filterString);
			
			if (expressions.Count == 1)
			{
				// Single expression - apply directly
				ApplyFilterExpression(expressions[0], this, entities);
			}
			else if (expressions.Count > 1)
			{
				// Multiple expressions with priority-based OR operator
				// Process expressions in order and return the first one that has results
				var originalEntities = entities.ToList();
				var finalResults = new List<MobileEntity>();
				
				foreach (var expression in expressions)
				{
					var tempEntities = originalEntities.ToList();
					ApplyFilterExpression(expression, this, tempEntities);
					
					// If this expression found results, use them and stop processing
					if (tempEntities.Any())
					{
						finalResults = tempEntities;
						break;
					}
				}
				
				// Replace entities with the results from the first matching expression
				entities.Clear();
				entities.AddRange(finalResults);
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