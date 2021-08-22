using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Collections;
using Kesmai.WorldForge.Scripting;
using Kesmai.WorldForge.UI.Documents;
using Microsoft.Toolkit.Mvvm.ComponentModel;

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