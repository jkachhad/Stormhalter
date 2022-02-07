using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Collections;
using Kesmai.WorldForge.Scripting;
using Kesmai.WorldForge.UI.Documents;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.CodeAnalysis;

namespace Kesmai.WorldForge
{
	[ScriptTemplate("OnSpawn", typeof(EntitySpawnScriptTemplate))]
	[ScriptTemplate("OnDeath", typeof(EntityDeathScriptTemplate))]
	[ScriptTemplate("OnIncomingPlayer", typeof(EntityIncomingPlayerScriptTemplate))]
	public class Entity : ObservableObject, ICloneable
	{
		private string _name;
		private string _notes;
		
		private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();
		
		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}
		
		public string Notes
		{
			get => _notes;
			set => SetProperty(ref _notes, value);
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

		[Description("Approximate offensive power (melee,ranged & magic)")]
		public Tuple<int?,int?> Threat
        {
			get
			{

				var onSpawnScript = _scripts.FirstOrDefault(
					s => string.Equals(s.Name, "OnSpawn", StringComparison.OrdinalIgnoreCase));
				if (onSpawnScript is null)
					return new Tuple<int?, int?>(null, null);

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
				

				//For spell skill, find all GenericNameSyntax nodes of type CreatureSpell.
				var spells = syntaxRoot.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
					.Where(node => node.Type is GenericNameSyntax type && type.Identifier.Text == "CreatureSpell");
				double? rangedSkill = spells.Select(node => {
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
						case "DeathSpell": multiplier = 2.5; break;
						case "IceSpearSpell": multiplier = 2.5; break;
						case "LightningBoltSpell": multiplier = 2.5; break;
						case "ConcussionSpell": multiplier = 2.5; break;
						case "BlindSpell":
						case "StunSpell":
						case "CreateWebSpell": multiplier = null; break;
						default: multiplier = 1.8;break;
					}


					// The syntax used for spell definitions often (always?) uses named parameters.
					// I can't just assume the first parameter is the right one, so see if there is a
					// named parameter for skillLevel and use that if present.
					var namedSkillArgument = identifiers.FirstOrDefault(i => i.Identifier.Text == "skillLevel");
					if (namedSkillArgument != null)
					{
						var skillargument = namedSkillArgument.Parent.Parent as ArgumentSyntax;
						var skilltoken = skillargument.Expression.GetFirstToken();
						skill = double.Parse(skilltoken.ValueText);
					} else
					{
						skill = node.ArgumentList.Arguments.First().Expression.GetFirstToken().Value as double?;
					}
					return skill * multiplier;
				}).Max();

				//If a creature has a bow, then consider its melee skill as ranged
				//First find all the wielded items and see if any end with "bow". This may need to be changed if 
				//new Ranged weapons are created, such as monster-wielded returning weapons.
				var wieldedItems = syntaxRoot.DescendantNodes().OfType<InvocationExpressionSyntax>()
					.Where(i => i.ChildNodes().First() is MemberAccessExpressionSyntax memberExpression
						&& memberExpression.ChildNodes().Last() is IdentifierNameSyntax member
						&& member.Identifier.Text == "Wield");
				var hasRangedWeapon = wieldedItems.Any(node => node.ChildNodes().Last() is ArgumentListSyntax arguments
					&& arguments.Arguments.First().Expression is ObjectCreationExpressionSyntax item
					&& item.Type is IdentifierNameSyntax itemName
					&& itemName.Identifier.Text.EndsWith("bow",StringComparison.InvariantCultureIgnoreCase));
				if (hasRangedWeapon)
				{
					if (meleeSkill > rangedSkill || rangedSkill is null)
						rangedSkill = meleeSkill;
					meleeSkill = null;
				}

				return new Tuple<int?, int?>(meleeSkill, (int?)rangedSkill);
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
			
			return element;
		}

		public override string ToString() => _name;
		
		public object Clone()
		{
			var clone = new Entity()
			{
				Name = $"Copy of {_name}",
				Notes = _notes,
			};
			
			clone.Scripts.Clear();
			clone.Scripts.AddRange(_scripts.Select(s => s.Clone()));

			return clone;
		}
	}
}