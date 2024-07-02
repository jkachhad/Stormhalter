using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Collections;
using Kesmai.WorldForge.Scripting;
using Kesmai.WorldForge.UI.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Configuration;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CodeAnalysis;

namespace Kesmai.WorldForge;

[ScriptTemplate("OnSpawn", typeof(EntitySpawnScriptTemplate))]
[ScriptTemplate("OnDeath", typeof(EntityDeathScriptTemplate))]
[ScriptTemplate("OnIncomingPlayer", typeof(EntityIncomingPlayerScriptTemplate))]
public class Entity : ObservableObject, ICloneable
{
	private string _name;
	private string _notes;
	private string _group;
		
	private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();
	
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
			{
				OnPropertyChanged("Name");
			}
		}
		
	}
		
	public string Notes
	{
		get => _notes;
		set => SetProperty(ref _notes, value);
	}
	
	public string Group
	{
		get => _group;
		set
		{
			if (SetProperty(ref _group, value))
			{
				OnPropertyChanged("Group");
			}
		}
	}
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}	
	public ObservableCollection<Script> Scripts
	{
		get => _scripts;
		set => SetProperty(ref _scripts, value);
	}

	public int? XP
	{
		get
		{
			var onSpawnScript = _scripts.FirstOrDefault(
				s => string.Equals(s.Name, "OnSpawn", StringComparison.OrdinalIgnoreCase));

			if (onSpawnScript is null)
				return null;

			/* Create a syntax tree for analysis. */
			var syntaxTree = CSharpSyntaxTree.ParseText(onSpawnScript.Blocks[1]);
			var syntaxRoot = syntaxTree.GetCompilationUnitRoot();

			/* Find a node that is an assignment, where the left identifier is "Experience" */
			var experienceAssignment = syntaxRoot
				.DescendantNodes().LastOrDefault(
					n => n is AssignmentExpressionSyntax assignmentSyntax
					     && assignmentSyntax.Left is IdentifierNameSyntax nameSyntax
					     && String.Equals(nameSyntax.Identifier.Text, "Experience",
						     StringComparison.OrdinalIgnoreCase));

			if (experienceAssignment is AssignmentExpressionSyntax assignment
			    && assignment.Right is LiteralExpressionSyntax valueSyntax)
				return int.Parse(valueSyntax.Token.Text);

			return null; // a null value here indicates I can't trust this number
		}
	}
	public int? HP
	{
		get
		{
			var onSpawnScript = _scripts.FirstOrDefault(
				s => string.Equals(s.Name, "OnSpawn", StringComparison.OrdinalIgnoreCase));

			if (onSpawnScript is null)
				return null;

			/* Create a syntax tree for analysis. */
			var syntaxTree = CSharpSyntaxTree.ParseText(onSpawnScript.Blocks[1]);
			var syntaxRoot = syntaxTree.GetCompilationUnitRoot();

			/* Find a node that is an assignment, where the left identifier is "MaxHealth" */
			var experienceAssignment = syntaxRoot
				.DescendantNodes().LastOrDefault(
					n => n is AssignmentExpressionSyntax assignmentSyntax
					     && assignmentSyntax.Left is IdentifierNameSyntax nameSyntax
					     && String.Equals(nameSyntax.Identifier.Text, "MaxHealth",
						     StringComparison.OrdinalIgnoreCase));

			if (experienceAssignment is AssignmentExpressionSyntax assignment
			    && assignment.Right is LiteralExpressionSyntax valueSyntax)
				return int.Parse(valueSyntax.Token.Text);

			return null; // a null value here indicates I can't trust this number
		}
	}

	public double? MeleeXPPerThreat
	{
		get {
			if(XP == null || XP < 1 || Threat.Item1 is null) return null;

			return Math.Round( (double) XP / (double) Threat.Item1 / (double)HP, 2);
		}
	}

	public double? RangedXPPerThreat
	{
		get {
			if(XP == null || XP < 1 || Threat.Item2 is null) return null;

			return Math.Round( (double) XP / (double) Threat.Item2 / (double)HP, 2 );
		}
	}

	public double? MagicXPPerThreat
	{
		get {
			if(XP == null || XP < 1 || Threat.Item3 is null) return null;

			return Math.Round( (double) XP / (double) Threat.Item3 / (double)HP, 2);
		}
	}

	[Description("Approximate offensive power (melee,ranged,magic)")]
	public Tuple<int?,int?,int?> Threat
	{
		get
		{

			var onSpawnScript = _scripts.FirstOrDefault(
				s => string.Equals(s.Name, "OnSpawn", StringComparison.OrdinalIgnoreCase));
			if (onSpawnScript is null)
				return new Tuple<int?, int?, int?>(null, null, null);

			/* Create a syntax tree for analysis. */
			var syntaxTree = CSharpSyntaxTree.ParseText("void OnSpawn(){"+onSpawnScript.Blocks[1]+"}");
			var syntaxRoot = syntaxTree.GetCompilationUnitRoot();

			//For melee skill, find all ObjectCreation nodes with a type of CreatureAttack or CreatureBasicAttack
			var attacks = syntaxRoot.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
				//.Where(node => node.DescendantNodes().OfType<IdentifierNameSyntax>().First().Identifier.Text is String objectName 
				.Where(node => node.Type is IdentifierNameSyntax type 
				               && type.Identifier.Text is String objectName 
				               && (objectName == "CreatureBasicAttack" || objectName == "CreatureAttack"));
			//For each attack, determine the value of the first argument, which is skill level, then find the maximum
			int? meleeSkill = attacks.Select(attack => attack.ArgumentList.Arguments.First().Expression.GetFirstToken().Value as int?).Max();
			//TODO: When we have a collection of attacks, Would be great here to not just get max but an Average based on the chance of each attack. a 10% chance stronger attack shouldnt be considdered the attack value.
			//TODO: When we have custom damage from attacks, would also be nice to factor in average damage to the threat. A level 20 attack doing 1 damage is no real threat.
			//      Maybe we could have a formular for how much damage a given skill level is "expected" to do and then scale the threat based on deviance.
			//		The expected value could be based on how much an average melee unit does damage for example. 

			//For spell skill, find all GenericNameSyntax nodes of type CreatureSpell.
			//TODO: should take into account the chance of a spell possibly - and instant cast!
			var spells = syntaxRoot.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
				.Where(node => node.Type is GenericNameSyntax type && type.Identifier.Text == "CreatureSpell");
			double? magicSkill = spells.Select(node => {
				double? skill = 0;
				var identifiers = node.DescendantNodes().OfType<IdentifierNameSyntax>();
						
				var spellType = identifiers.First().Identifier.Text;
				// Spells are more dangerous than melee hits because of the lack of dodging and resistances
				// They get a multiplier on the skill level to determine relative threat.
				// Some spells are more or less dangerous from a DPS output, so modify the multiplier for them
				// These values are guesses and may need to be tweaked.
				double? multiplier;
				switch (spellType)
				{
					case "DeathSpell": multiplier = 2.3; break; //Hard to say because Death Prot Halves the damage. At one point you have to assume people have this. Lets do a sliding scale
					case "IceSpearSpell": multiplier = 2.5; break;
					case "LightningBoltSpell": multiplier = 2.0; break;
					case "ConcussionSpell": multiplier = 1.25; break;
					case "MagicMissileSpell": multiplier = 1.7; break;
					case "CurseSpell": multiplier = 1.5; break;
					case "DragonBreathFireSpell": multiplier = 1.3; break;
					case "FirewallSpell": multiplier = 1; break;
					case "FireBoltSpell": multiplier = 1.1; break;
					case "IceStormSpell": //these are not currently dangerous 
					case "FireballSpell":
					case "DarknessSpell":
					case "BlindSpell":
					case "StunSpell":
					case "FearSpell": //Ignored because simple to mitigate 
					case "CreateWebSpell": multiplier = null; break;
					default: multiplier = 1.3; break;
				}

				// The syntax used for spell definitions often (always?) uses named parameters.
				// I can't just assume the first parameter is the right one, so see if there is a
				// named parameter for skillLevel and use that if present.
				var namedSkillArgument = identifiers.FirstOrDefault(i => i.Identifier.Text == "skillLevel");
				if (namedSkillArgument != null)
				{
					var skillargument = namedSkillArgument.Parent.Parent as ArgumentSyntax;
					var skilltoken = skillargument.Expression.GetFirstToken();

					if (double.TryParse(skilltoken.ValueText, out double d) && !Double.IsNaN(d) && !Double.IsInfinity(d))
						skill = double.Parse(skilltoken.ValueText);
					else
						skill = 1;
				} else
				{
					skill = node.ArgumentList.Arguments.First().Expression.GetFirstToken().Value as double?;
				}

				return skill * multiplier;
			}).Max();

			//new Ranged weapons are created, such as monster-wielded returning weapons.
			var wieldedItems = syntaxRoot.DescendantNodes().OfType<InvocationExpressionSyntax>()
				.Where(i => i.ChildNodes().First() is MemberAccessExpressionSyntax memberExpression
				            && memberExpression.ChildNodes().Last() is IdentifierNameSyntax member
				            && member.Identifier.Text == "Wield");
			var hasRangedWeapon = wieldedItems.Any(node => node.ChildNodes().Last() is ArgumentListSyntax arguments
			                                               && arguments.Arguments.First().Expression is ObjectCreationExpressionSyntax item
			                                               && item.Type is IdentifierNameSyntax itemName
			                                               && itemName.Identifier.Text.EndsWith("bow",StringComparison.InvariantCultureIgnoreCase));
			int? rangedSkill = null;
			if (hasRangedWeapon)
			{
				rangedSkill = meleeSkill;
				meleeSkill = null;
			}

			//factor in some threat from flags
			String flags = Flags;
			if (flags.Contains("Pois"))
			{
				
				// when using variables in the onspawn script it is possible to have a null exception since meleeSkill gets passed null if it doesnt parse a number.
				if (meleeSkill is not null)
				meleeSkill += Math.Max(1, (int)(meleeSkill * 0.3)); //scale up the threat of melee if they cause poison At least 1 level
			}
			if (flags.Contains("Prone"))
			{
				meleeSkill += 1; //slightly more threat due to prone
			}
			if (flags.Contains("NV"))
			{
				//slightly more threat due to night vision
				if (meleeSkill is not null) meleeSkill += 1; 
				if (rangedSkill is not null) rangedSkill += 1;
				if (magicSkill is not null) magicSkill += 1;
			}

			return new Tuple<int?, int?, int?>(meleeSkill, rangedSkill, (int?)magicSkill);
		}
	}

	[Description("Indicators for various modifiers on a mob like NightVision, swimming, etc")]
	public String Flags
	{
		get
		{
			var flags = "";
			var onSpawnScript = _scripts.FirstOrDefault(
				s => string.Equals(s.Name, "OnSpawn", StringComparison.OrdinalIgnoreCase));

			if (onSpawnScript is null)
				return flags;

			/* Create a syntax tree for analysis. */
			var parseOptions = new CSharpParseOptions(
				kind: SourceCodeKind.Script,
				languageVersion: LanguageVersion.CSharp8
			);
			var syntaxTree = CSharpSyntaxTree.ParseText("#load \"WorldForge\"\nusing System;\nCreatureEntity OnSpawn(){" + onSpawnScript.Blocks[1] + "}", parseOptions);
			var syntaxRoot = syntaxTree.GetCompilationUnitRoot();

			var assignments = syntaxRoot.DescendantNodes().OfType<AssignmentExpressionSyntax>();

			if (syntaxRoot.DescendantNodes().Any(n => n is IdentifierNameSyntax identifier && identifier.Identifier.Text == "NightVisionStatus"))
				flags += "NV ";
			if (syntaxRoot.DescendantNodes().Any(n => n is IdentifierNameSyntax identifier && identifier.Identifier.Text == "BreatheWaterStatus"))
				flags += "BW ";


			if (assignments.Where(n => n.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault() is IdentifierNameSyntax i && i.Identifier.Text == "CanSwim")
			    .Any(n => n.ChildNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault() is LiteralExpressionSyntax l && l.Kind() == SyntaxKind.TrueLiteralExpression))
				flags += "Swim ";
			if (assignments.Where(n => n.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault() is IdentifierNameSyntax i && i.Identifier.Text == "CanFly")
			    .Any(n => n.ChildNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault() is LiteralExpressionSyntax l && l.Kind() == SyntaxKind.TrueLiteralExpression))
				flags += "Fly ";
			if (assignments.Where(n => n.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault() is IdentifierNameSyntax i && i.Identifier.Text == "CanLoot")
			    .Any(n => n.ChildNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault() is LiteralExpressionSyntax l && l.Kind() == SyntaxKind.TrueLiteralExpression))
				flags += "Loot ";

			if (syntaxRoot.DescendantNodes().Any(n => n is IdentifierNameSyntax identifier && identifier.Identifier.Text == "AttackPoisonComponent"))
				flags += "Pois ";
			if (syntaxRoot.DescendantNodes().Any(n => n is IdentifierNameSyntax identifier && identifier.Identifier.Text == "AttackProneComponent"))
				flags += "Prone ";

			return flags;
		}
	}
	public Entity()
	{
		ValidateScripts();
	}
		
	public Entity(XElement element)
	{
		_name = (string)element.Attribute("name");

		if (element.TryGetElement("notes", out var notesElement))
			_notes = (string)notesElement;
		
		if (element.TryGetElement("group", out var groupElement))
			_group = (string)groupElement;
			
		foreach (var scriptElement in element.Elements("script"))
			_scripts.Add(new Script(scriptElement));

		ValidateScripts();
	}

	private void ValidateScripts()
	{
		if (_scripts.All(s => s.Name != "OnSpawn"))
		{
			_scripts.Add(new Script("OnSpawn", true,
				String.Empty,
				"\n\treturn new MobileEntity();\n",
				String.Empty
			));
		}
			
		if (_scripts.All(s => s.Name != "OnDeath"))
		{
			_scripts.Add(new Script("OnDeath", false,
				String.Empty, 
				"\n", 
				String.Empty
			));
		}
			
		if (_scripts.All(s => s.Name != "OnIncomingPlayer"))
		{
			_scripts.Add(new Script("OnIncomingPlayer", false,
				String.Empty, 
				"\n", 
				String.Empty
			));
		}

		var provider = ServiceLocator.Current.GetInstance<ScriptTemplateProvider>();
		var attributes = GetType().GetCustomAttributes(typeof(ScriptTemplateAttribute), false)
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
		
	public XElement GetXElement()
	{
		var element = new XElement("entity", 
			new XAttribute("name", _name));
			
		foreach (var script in _scripts)
			element.Add(script.GetXElement());

		if (!String.IsNullOrEmpty(_notes))
			element.Add(new XElement("notes", _notes));
		
		if (!String.IsNullOrEmpty(_group))
			element.Add(new XElement("group", _group));
			
		return element;
	}

	public override string ToString() => _name;
		
	public object Clone()
	{
		var clone = new Entity()
		{
			Name = $"Copy of {_name}",
			Notes = _notes,
			Group = _group
		};
			
		clone.Scripts.Clear();
		clone.Scripts.AddRange(_scripts.Select(s => s.Clone()));

		return clone;
	}
}