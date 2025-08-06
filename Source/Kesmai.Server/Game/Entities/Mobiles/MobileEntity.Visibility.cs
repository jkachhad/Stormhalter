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
		// Alignment filters
		["hostile"] = (source, entities) => entities.RemoveAll(entity => !source.IsHostile(entity)),
		["friendly"] = (source, entities) => entities.RemoveAll(entity => source.IsHostile(entity)),
		
		// Type filters
		["pc"] = (source, entities) => entities.RemoveAll(entity => (entity is not PlayerEntity)),
		["npc"] = (source, entities) => entities.RemoveAll(entity => (entity is not CreatureEntity)),
		["conjured"] = (source, entities) => entities.RemoveAll(entity => entity is CreatureEntity { IsSubordinate: false }),
		
		// Health state filters
		["injured"] = (source, entities) => entities.RemoveAll(entity => (entity.Health != entity.MaxHealth)),
		["healthy"] = (source, entities) => entities.RemoveAll(entity => (entity.Health < entity.MaxHealth)),
		["deathly"] = (source, entities) => entities.RemoveAll(entity => (Combat.GetHealthState(entity) != 1)),
		
		// Activity filters
		["casting"] = (source, entities) => entities.RemoveAll(entity => (entity.Spell is null)), 

		// Weapon type filters
		["melee"] = (source, entities) => entities.RemoveAll(entity => (entity.GetWeapon() is not MeleeWeapon)),
		["ranged"] = (source, entities) => entities.RemoveAll(entity => (entity.GetWeapon() is not ProjectileWeapon)),
		
		// Distance filters
		["near"] = (source, entities) => entities.RemoveAll(entity => source.GetDistanceToMax(entity.Location) > 0),
		["distant"] = (source, entities) => entities.RemoveAll(entity => source.GetDistanceToMax(entity.Location) is 0),
		["far"] = (source, entities) => entities.RemoveAll(entity => source.GetDistanceToMax(entity.Location) < 3),
		
		// Effect filters
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
	/// Represents a filter expression that can be evaluated
	/// </summary>
	private abstract class FilterExpression
	{
		public abstract List<MobileEntity> Evaluate(MobileEntity source, List<MobileEntity> entities);
	}

	/// <summary>
	/// Represents a single filter condition
	/// </summary>
	private class FilterCondition : FilterExpression
	{
		public string FilterName { get; }
		public bool IsNegated { get; }

		public FilterCondition(string filterName, bool isNegated = false)
		{
			FilterName = filterName;
			IsNegated = isNegated;
		}

		public override List<MobileEntity> Evaluate(MobileEntity source, List<MobileEntity> entities)
		{
			var result = entities.ToList();
			ApplyFilter(this, source, result);
			return result;
		}
	}

	/// <summary>
	/// Represents a logical operation (AND/OR) between multiple expressions
	/// </summary>
	private class LogicalOperation : FilterExpression
	{
		public List<FilterExpression> Children { get; } = new();
		public bool IsOrOperation { get; }

		public LogicalOperation(bool isOrOperation)
		{
			IsOrOperation = isOrOperation;
		}

		public override List<MobileEntity> Evaluate(MobileEntity source, List<MobileEntity> entities)
		{
			if (IsOrOperation)
			{
				// OR: return first child that has results
				foreach (var child in Children)
				{
					var childResult = child.Evaluate(source, entities);
					if (childResult.Any())
						return childResult;
				}
				return new List<MobileEntity>();
			}
			else
			{
				// AND: apply all children sequentially
				var result = entities.ToList();
				foreach (var child in Children)
				{
					result = child.Evaluate(source, result);
					if (!result.Any()) break; // Short-circuit
				}
				return result;
			}
		}
	}

	/// <summary>
	/// Simplified parser for filter expressions
	/// </summary>
	private class FilterParser
	{
		private readonly string _input;
		private int _position;
		
		// Limits to prevent abuse
		private const int MAX_FILTER_LENGTH = 200;
		private const int MAX_OR_CONDITIONS = 5;
		private const int MAX_AND_CONDITIONS = 3;
		private const int MAX_NESTING_DEPTH = 3;

		public FilterParser(string input)
		{
			_input = input;
			_position = 0;
		}

		private char Peek() => _position < _input.Length ? _input[_position] : '\0';
		private char Advance() => _position < _input.Length ? _input[_position++] : '\0';
		private bool IsAtEnd() => _position >= _input.Length;

		private void SkipWhitespace()
		{
			while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
				Advance();
		}

		public FilterExpression Parse()
		{
			if (_input.Length > MAX_FILTER_LENGTH)
				throw new ArgumentException($"Filter expression too long. Maximum length is {MAX_FILTER_LENGTH} characters.");

			SkipWhitespace();
			var result = ParseOr(0);
			SkipWhitespace();
			
			if (!IsAtEnd())
				throw new ArgumentException($"Unexpected character at position {_position}: {Peek()}");
				
			return result;
		}

		private FilterExpression ParseOr(int depth)
		{
			if (depth > MAX_NESTING_DEPTH)
				throw new ArgumentException($"Filter expression too deeply nested. Maximum depth is {MAX_NESTING_DEPTH}.");

			var left = ParseAnd(depth);
			SkipWhitespace();

			if (Peek() == '|')
			{
				var orNode = new LogicalOperation(true) { Children = { left } };

				while (Peek() == '|')
				{
					if (orNode.Children.Count >= MAX_OR_CONDITIONS)
						throw new ArgumentException($"Too many OR conditions. Maximum is {MAX_OR_CONDITIONS}.");

					Advance(); // consume '|'
					SkipWhitespace();
					orNode.Children.Add(ParseAnd(depth));
					SkipWhitespace();
				}

				return orNode;
			}

			return left;
		}

		private FilterExpression ParseAnd(int depth)
		{
			var left = ParsePrimary(depth);
			SkipWhitespace();

			if (Peek() == ':')
			{
				var andNode = new LogicalOperation(false) { Children = { left } };

				while (Peek() == ':')
				{
					if (andNode.Children.Count >= MAX_AND_CONDITIONS)
						throw new ArgumentException($"Too many AND conditions. Maximum is {MAX_AND_CONDITIONS}.");

					Advance(); // consume ':'
					SkipWhitespace();
					andNode.Children.Add(ParsePrimary(depth));
					SkipWhitespace();
				}

				return andNode;
			}

			return left;
		}

		private FilterExpression ParsePrimary(int depth)
		{
			SkipWhitespace();

			if (Peek() == '(')
			{
				Advance(); // consume '('
				var result = ParseOr(depth + 1);
				SkipWhitespace();
				
				if (Peek() != ')')
					throw new ArgumentException($"Expected ')' at position {_position}");
					
				Advance(); // consume ')'
				return result;
			}

			if (Peek() == '!')
			{
				Advance(); // consume '!'
				SkipWhitespace();
				return new FilterCondition(ParseIdentifier(), true);
			}

			return new FilterCondition(ParseIdentifier(), false);
		}

		private string ParseIdentifier()
		{
			var start = _position;
			
			if (!char.IsLetter(Peek()))
				throw new ArgumentException($"Expected identifier at position {_position}");

			// Parse identifier
			while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
				Advance();

			// Handle function call syntax
			if (Peek() == '(')
			{
				Advance(); // consume '('
				while (!IsAtEnd() && Peek() != ')')
					Advance();
					
				if (Peek() == ')')
					Advance(); // consume ')'
			}

			return _input.Substring(start, _position - start);
		}
	}

	/// <summary>
	/// Applies a filter condition to the entities list
	/// </summary>
	private static void ApplyFilter(FilterCondition condition, MobileEntity source, List<MobileEntity> entities)
	{
		// Try basic filters first
		if (_basicFilters.TryGetValue(condition.FilterName.ToLower(), out var basicFilter))
		{
			ApplyFilterWithNegation(basicFilter, condition.IsNegated, source, entities);
			return;
		}

		// Try advanced filters
		foreach (var (filterRegex, function) in _advancedFilters)
		{
			if (filterRegex.TryGetMatch(condition.FilterName.ToLower(), out var match))
			{
				ApplyFilterWithNegation((s, e) => function(match, s, e), condition.IsNegated, source, entities);
				return;
			}
		}
	}

	/// <summary>
	/// Applies a filter with proper negation handling
	/// </summary>
	private static void ApplyFilterWithNegation(Action<MobileEntity, List<MobileEntity>> filter, bool isNegated, MobileEntity source, List<MobileEntity> entities)
	{
		if (isNegated)
		{
			// For negated filters, invert the logic
			var originalEntities = entities.ToList();
			filter(source, entities);
			var removedEntities = originalEntities.Except(entities).ToList();
			entities.Clear();
			entities.AddRange(removedEntities);
		}
		else
		{
			filter(source, entities);
		}
	}

	/// <summary>
	/// Finds an entity with the specified name reference.
	/// </summary>
	public MobileEntity FindMobileByName(string name)
	{
		var entities = GetBeheldInVisibility().SelectMany(g => g.Members)
			.OrderBy(m => m, new MobileDistanceComparer(this)).ToList();

		return FindMobileByName(name, entities);
	}

	/// <summary>
	/// Finds an entity with the specified name reference.
	/// </summary>
	public MobileEntity FindMobileByName(string name, List<MobileEntity> entities)
	{
		var originalEntities = entities.ToList();
		var filterString = "";
		var hasFilter = false;
		
		// Parse filter target format "@name[filter]"
		if (_filterTarget.TryGetMatch(name, out var filterTargetMatch))
		{
			filterString = filterTargetMatch.Groups[3].Value;
			name = filterTargetMatch.Groups[1].Value;
			hasFilter = !string.IsNullOrEmpty(filterString);

			// Apply filter if present
			if (hasFilter)
			{
				try
				{
					var parser = new FilterParser(filterString);
					var filterExpression = parser.Parse();
					var filteredEntities = filterExpression.Evaluate(this, entities);
					entities.Clear();
					entities.AddRange(filteredEntities);
				}
				catch (ArgumentException ex)
				{
					Log.Warn($"Filter parsing error: {ex.Message}");
				}
			}
		}
		
		// Filter by name if specified
		if (!String.IsNullOrEmpty(name))
			entities.RemoveAll(entity => !entity.RespondsTo(name));
		
		// Check if last target matches
		var lastTarget = FindMostRecentTarget();
		if (entities.Contains(lastTarget))
			return lastTarget;
		
		// Handle OR filter priority
		if (entities.Any() && hasFilter && filterString.Contains('|'))
		{
			try
			{
				var parser = new FilterParser(filterString);
				var filterExpression = parser.Parse();
				
				if (filterExpression is LogicalOperation orNode && orNode.IsOrOperation)
				{
					foreach (var child in orNode.Children)
					{
						var childResult = child.Evaluate(this, originalEntities);
						if (childResult.Any())
							return childResult.FirstOrDefault();
					}
				}
			}
			catch (ArgumentException ex)
			{
				Log.Warn($"OR filter priority parsing error: {ex.Message}");
			}
		}
		
		// If a filter was applied and no entities match, return null (no target)
		if (hasFilter && !entities.Any())
			return null;
		
		return entities.FirstOrDefault();
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