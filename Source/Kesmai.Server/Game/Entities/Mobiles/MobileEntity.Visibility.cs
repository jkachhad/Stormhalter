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
				{
					entities.Clear();
					entities.Add(entity);
				}
			}
		}, 
	};

	/// <summary>
	/// Represents a node in the filter expression AST
	/// </summary>
	private abstract class FilterNode
	{
		public abstract List<MobileEntity> Evaluate(MobileEntity source, List<MobileEntity> entities);
	}

	/// <summary>
	/// Represents a leaf node with a single filter condition
	/// </summary>
	private class FilterLeaf : FilterNode
	{
		public string FilterName { get; set; }
		public bool IsNegated { get; set; }

		public FilterLeaf(string filterName, bool isNegated = false)
		{
			FilterName = filterName;
			IsNegated = isNegated;
		}

		public override List<MobileEntity> Evaluate(MobileEntity source, List<MobileEntity> entities)
		{
			var result = entities.ToList();
			ApplyFilterCondition(this, source, result);
			return result;
		}
	}

	/// <summary>
	/// Represents an AND operation between multiple nodes
	/// </summary>
	private class AndNode : FilterNode
	{
		public List<FilterNode> Children { get; set; } = new();

		public override List<MobileEntity> Evaluate(MobileEntity source, List<MobileEntity> entities)
		{
			var result = entities.ToList();
			foreach (var child in Children)
			{
				result = child.Evaluate(source, result);
				if (!result.Any()) break; // Short-circuit if any condition fails
			}
			return result;
		}
	}

	/// <summary>
	/// Represents an OR operation between multiple nodes (with priority)
	/// </summary>
	private class OrNode : FilterNode
	{
		public List<FilterNode> Children { get; set; } = new();

		public override List<MobileEntity> Evaluate(MobileEntity source, List<MobileEntity> entities)
		{
			// Priority-based OR: return first child that has results
			foreach (var child in Children)
			{
				var childResult = child.Evaluate(source, entities);
				if (childResult.Any())
				{
					return childResult;
				}
			}
			return new List<MobileEntity>(); // No results from any child
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
		public FilterNode Parse()
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
		private FilterNode ParseOr()
		{
			var left = ParseAnd();

			if (Peek() == '|')
			{
				var orNode = new OrNode();
				orNode.Children.Add(left);

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
		private FilterNode ParseAnd()
		{
			var left = ParsePrimary();

			if (Peek() == ':')
			{
				var andNode = new AndNode();
				andNode.Children.Add(left);

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
		private FilterNode ParsePrimary()
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
				return new FilterLeaf(filterName, true);
			}

			var name = ParseIdentifier();
			return new FilterLeaf(name, false);
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
	/// Applies a single filter condition to the entities list
	/// </summary>
	private static void ApplyFilterCondition(FilterLeaf condition, MobileEntity source, List<MobileEntity> entities)
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
			// Remove all whitespace from the filter string before parsing.
			// None of filters have whitespace in them, should be safe.
			var filterString = filterTargetMatch.Groups[3].Value.Replace(" ", String.Empty);

			// Parse the filter string into an AST
			try
			{
				var parser = new FilterParser(filterString);
				var ast = parser.Parse();

				// Evaluate the AST to get filtered entities
				var filteredEntities = ast.Evaluate(this, entities);
				entities.Clear();
				entities.AddRange(filteredEntities);
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