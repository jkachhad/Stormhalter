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

namespace Kesmai.WorldForge
{
	[ScriptTemplate("OnSpawn", typeof(EntitySpawnScriptTemplate))]
	[ScriptTemplate("OnDeath", typeof(EntityDeathScriptTemplate))]
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
				//These regexes are fraught. They are likely to break based on developer syntax preferences, but seem to work across most mobs I've tested.
				var skillPattern = new System.Text.RegularExpressions.Regex("Creature(?:Basic)?Attack\\(\\s*(\\d+)\\s*[,)]", System.Text.RegularExpressions.RegexOptions.Multiline);
				var matches = skillPattern.Matches(this.Scripts[0].Blocks[1]);
				int? meleeSkill = 0;
				foreach (System.Text.RegularExpressions.Match match in matches)
				{
					if (int.TryParse(match.Groups[1].Value, out var thisAttack))
					{
						meleeSkill = Math.Max((int)meleeSkill, thisAttack);
					}
					else
						meleeSkill = null;
				}
				skillPattern = new System.Text.RegularExpressions.Regex("CreatureSpell<(\\w*)>\\(\\s*(?:skillLevel:)?\\s*(\\d+\\.?\\d*)?\\s*[,)]", System.Text.RegularExpressions.RegexOptions.Multiline);
				matches = skillPattern.Matches(this.Scripts[0].Blocks[1]);
				int? rangedSkill = 0;
				foreach (System.Text.RegularExpressions.Match match in matches.Where(m => !new[] {"BlindSpell","StunSpell"}.Contains(m.Groups[1].Value)))
				{
					if (double.TryParse(match.Groups[2].Value, out var thisAttack))
					{
						rangedSkill = Math.Max((int)rangedSkill, (int)(thisAttack*2)); //magic skills are more of a threat than melee.
					}
					else
						rangedSkill = null;
				}
				//This regex will need attention if there are ranged weapons that aren't longbow, shortbow, crossbow, etc. RHammer trolls come to mind, but may be the only exception.
				skillPattern = new System.Text.RegularExpressions.Regex("Wield\\([^)]*bow", System.Text.RegularExpressions.RegexOptions.Multiline);
				matches = skillPattern.Matches(this.Scripts[0].Blocks[1]);
				if (matches.Count > 0) // if we're equiping a bow, then the melee skill is actually ranged.
				{
					rangedSkill = Math.Max((int)meleeSkill, (int)rangedSkill);
					meleeSkill = null;
				}

				return new Tuple<int?, int?>(meleeSkill, rangedSkill);
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