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
	private static readonly Regex _filterTarget = new Regex(@"^\s*@\s*(\w*)(\[(.*?)\])\s*?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private static readonly Dictionary<string, Func<MobileEntity, MobileEntity, bool>> _basicFilters = new()
	{
		["hostile"] = (source, entity) => source.IsHostile(entity),
		["friendly"] = (source, entity) => !source.IsHostile(entity),
		
		["pc"] = (source, entity) => entity is PlayerEntity,
		["npc"] = (source, entity) => entity is CreatureEntity,
		["conjured"] = (source, entity) => entity is CreatureEntity { IsSubordinate: true },
		
		["injured"] = (source, entity) => entity.Health != entity.MaxHealth,
		["healthy"] = (source, entity) => entity.Health == entity.MaxHealth,
		["deathly"] = (source, entity) => Combat.GetHealthState(entity) != 1,
		
		["casting"] = (source, entity) => entity.Spell is not null, 

		["melee"] = (source, entity) => entity.GetWeapon() is MeleeWeapon,
		["ranged"] = (source, entity) => entity.GetWeapon() is ProjectileWeapon,
		
		["near"] = (source, entity) => source.GetDistanceToMax(entity.Location) == 0,
		["distant"] = (source, entity) => source.GetDistanceToMax(entity.Location) != 0,
		["far"] = (source, entity) => source.GetDistanceToMax(entity.Location) >= 3,
		
		["poisoned"] = (source, entity) => entity.IsPoisoned,
		["feared"] = (source, entity) => entity.IsFeared,
		["stunned"] = (source, entity) => entity.IsStunned || entity.IsDazed,
		["blind"] = (source, entity) => entity.IsBlind,
	};
	
	private static readonly Dictionary<Regex, Func<Match, MobileEntity, MobileEntity, bool>> _advancedFilters = new()
	{
		// distance(value) - includes entities at the specified distance.
		[new Regex(@"^distance\((\w*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = (match, source, entity) =>
		{
			if (Int32.TryParse(match.Groups[1].Value, out int value))
				return source.GetDistanceToMax(entity.Location) == value;
			return false;
		},
		
		// serial(value) - finds the entity by serial.
		[new Regex(@"^serial\((\w*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = (match, source, entity) =>
		{
			if (Int32.TryParse(match.Groups[1].Value, out int value))
				return entity.Serial.Value == value;
			return false;
		},
		
		// index(value) - finds the entity by index or specifier.
		[new Regex(@"^index\((\w*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = (match, source, entity) =>
		{
			// Note: index filter needs special handling in the AST since it requires the full list
			// This predicate will always return false for individual entities
			return false;
		}, 
	};

	/// <summary>
	/// High-performance filter state that tracks entity inclusion without copying lists
	/// </summary>
	private class FilterState
	{
		public List<MobileEntity> Entities { get; set; }
		public bool[] Included { get; set; }
		public int Count { get; private set; }
		
		public FilterState(List<MobileEntity> entities)
		{
			Entities = entities;
			Included = new bool[entities.Count];
			Count = entities.Count;
			// Initially include all entities
			Array.Fill(Included, true);
		}
		
		public void Exclude(int index)
		{
			if (Included[index])
			{
				Included[index] = false;
				Count--;
			}
		}
		
		public void ExcludeAll(Func<MobileEntity, bool> predicate)
		{
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Included[i] && predicate(Entities[i]))
				{
					Included[i] = false;
					Count--;
				}
			}
		}
		
		public List<MobileEntity> GetResult()
		{
			if (Count == Entities.Count)
				return Entities; // No filtering occurred, return original
				
			var result = new List<MobileEntity>(Count);
			for (int i = 0; i < Entities.Count; i++)
			{
				if (Included[i])
					result.Add(Entities[i]);
			}
			return result;
		}
		
		public bool HasAny() => Count > 0;
	}

	/// <summary>
	/// Represents a filter expression that can be evaluated
	/// </summary>
	private abstract class FilterExpression
	{
		public abstract void Evaluate(MobileEntity source, FilterState state);
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

		public override void Evaluate(MobileEntity source, FilterState state)
		{
			ApplyFilterCondition(this, source, state);
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

		public override void Evaluate(MobileEntity source, FilterState state)
		{
			if (IsOrOperation)
			{
				// OR: try each child and use the first one that has results
				foreach (var child in Children)
				{
					var branchState = new FilterState(state.Entities);
					child.Evaluate(source, branchState);
					
					if (branchState.HasAny())
					{
						// Found a working branch, use its result
						var bestResult = branchState.GetResult();
						
						// Update the original state with the best result
						Array.Fill(state.Included, false);
						state.Count = 0;
						
						foreach (var entity in bestResult)
						{
							var index = state.Entities.IndexOf(entity);
							if (index >= 0)
							{
								state.Included[index] = true;
								state.Count++;
							}
						}
						return;
					}
				}
				
				// No results from any child
				Array.Fill(state.Included, false);
				state.Count = 0;
			}
			else
			{
				// AND: apply all children sequentially
				foreach (var child in Children)
				{
					child.Evaluate(source, state);
					if (!state.HasAny()) break; // Short-circuit if any condition fails
				}
			}
		}
	}

	/// <summary>
	/// Parser for filter expressions that builds an AST
	/// </summary>
	private ref struct FilterParser
	{
		private readonly ReadOnlySpan<char> _input;
		private int _position;

		public FilterParser(string input)
		{
			_input = input.AsSpan();
			_position = 0;
		}

		private char Peek() => _position < _input.Length ? _input[_position] : '\0';
		private char Advance() => _position < _input.Length ? _input[_position++] : '\0';
		private bool IsAtEnd() => _position >= _input.Length;

		/// <summary>
		/// Parse the entire expression
		/// </summary>
		public FilterExpression Parse()
		{
			var result = ParseOr();
			if (!IsAtEnd())
			{
				throw new ArgumentException($"Unexpected character at position {_position}: {Peek()}");
			}
			return result;
		}

		/// <summary>
		/// Parse OR expressions (lowest precedence)
		/// </summary>
		private FilterExpression ParseOr()
		{
			var left = ParseAnd();

			if (Peek() == '|')
			{
				var orNode = new LogicalOperation(true) { Children = { left } };

				while (Peek() == '|')
				{
					Advance(); // consume '|'
					orNode.Children.Add(ParseAnd());
				}

				return orNode;
			}

			return left;
		}

		/// <summary>
		/// Parse AND expressions (medium precedence)
		/// </summary>
		private FilterExpression ParseAnd()
		{
			var left = ParsePrimary();

			if (Peek() == ':')
			{
				var andNode = new LogicalOperation(false) { Children = { left } };

				while (Peek() == ':')
				{
					Advance(); // consume ':'
					andNode.Children.Add(ParsePrimary());
				}

				return andNode;
			}

			return left;
		}

		/// <summary>
		/// Parse primary expressions (highest precedence)
		/// </summary>
		private FilterExpression ParsePrimary()
		{
			if (Peek() == '(')
			{
				Advance(); // consume '('
				var result = ParseOr();

				if (Peek() != ')')
				{
					throw new ArgumentException($"Expected ')' at position {_position}");
				}
				Advance(); // consume ')'
				return result;
			}

			if (Peek() == '!')
			{
				Advance(); // consume '!'
				var filterName = ParseIdentifier();
				return new FilterCondition(filterName, true);
			}

			var name = ParseIdentifier();
			return new FilterCondition(name, false);
		}

		/// <summary>
		/// Parse a filter identifier (basic filter name or advanced filter with parameters)
		/// </summary>
		private string ParseIdentifier()
		{
			var start = _position;

			// Handle advanced filters like distance(3), serial(12345), etc.
			if (char.IsLetter(Peek()))
			{
				while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
				{
					Advance();
				}

				// Check for function call syntax
				if (Peek() == '(')
				{
					Advance(); // consume '('

					// Parse parameters (simplified - just consume until closing paren)
					while (!IsAtEnd() && Peek() != ')')
					{
						Advance();
					}

					if (Peek() == ')')
					{
						Advance(); // consume ')'
					}
				}
			}
			else
			{
				throw new ArgumentException($"Expected identifier at position {_position}");
			}

			return _input.Slice(start, _position - start).ToString();
		}
	}

	/// <summary>
	/// Applies a single filter condition to the filter state
	/// </summary>
	private static void ApplyFilterCondition(FilterCondition condition, MobileEntity source, FilterState state)
	{
		// Check basic filters first
		if (_basicFilters.TryGetValue(condition.FilterName.ToLower(), out var basicFilter))
		{
			if (condition.IsNegated)
			{
				// For negated filters, exclude entities that match the filter (keep non-matching ones)
				state.ExcludeAll(entity => basicFilter(source, entity));
			}
			else
			{
				// Apply filter directly to state - exclude entities that don't match
				state.ExcludeAll(entity => !basicFilter(source, entity));
			}
			return;
		}

		// Check advanced filters
		foreach (var (filterRegex, function) in _advancedFilters)
		{
			if (!filterRegex.TryGetMatch(condition.FilterName.ToLower(), out var advancedFilters)) 
				continue;
			
			// Special case for index filter which needs the full list context
			if (condition.FilterName.ToLower().StartsWith("index("))
			{
				ApplyIndexFilter(advancedFilters, source, state);
				return;
			}
			
			if (condition.IsNegated)
			{
				// For negated advanced filters, exclude entities that match the filter (keep non-matching ones)
				state.ExcludeAll(entity => function(advancedFilters, source, entity));
			}
			else
			{
				// Apply filter directly to state - exclude entities that don't match
				state.ExcludeAll(entity => !function(advancedFilters, source, entity));
			}
			return;
		}
	}

	/// <summary>
	/// Special handling for index filter which requires full list context
	/// </summary>
	private static void ApplyIndexFilter(Match match, MobileEntity source, FilterState state)
	{
		var indexValue = match.Groups[1].Value;
		var index = 1; // index is 1-based for the user to prevent confusion.
		
		if (!Int32.TryParse(indexValue, out index) && state.Entities.Any())
		{
			if (indexValue.Matches("last"))
				index = state.Entities.Count;
		}
		
		if (index > 0 && index <= state.Entities.Count)
		{
			var entity = state.Entities[index - 1];
			// Clear all and only include the selected entity
			Array.Fill(state.Included, false);
			state.Included[index - 1] = true;
			state.Count = 1;
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
			// Remove all whitespace from the filter string before parsing.
			// None of filters have whitespace in them, should be safe.
			var filterString = filterTargetMatch.Groups[3].Value.Replace(" ", String.Empty);

			// Parse the filter string into an AST
			try
			{
				var parser = new FilterParser(filterString);
				var ast = parser.Parse();

				// Evaluate the AST to get filtered entities
				var state = new FilterState(entities);
				ast.Evaluate(this, state);
				entities.Clear();
				entities.AddRange(state.GetResult());
			}
			catch (ArgumentException ex)
			{
				// Log parsing errors but continue with unfiltered entities
				Log.Warn($"Filter parsing error: {ex.Message}");
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