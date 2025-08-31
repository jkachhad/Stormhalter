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
using Kesmai.WorldForge.Editor;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Configuration;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.CodeAnalysis;

namespace Kesmai.WorldForge;

[Script("OnSpawn", "CreatureEntity OnSpawn()", "{", "}", "\treturn new MobileEntity();")]
[Script("OnDeath", "void OnDeath(MobileEntity source, MobileEntity killer)", "{", "}")]
[Script("OnIncomingPlayer", "void OnIncomingPlayer(MobileEntity source, PlayerEntity player)", "{", "}")]
public class Entity : ObservableObject, ICloneable, ISegmentObject
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
	public new event PropertyChangedEventHandler PropertyChanged;

	protected new virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
		
		if (element.TryGetElement("group", out var groupElement))
			_group = (string)groupElement;
			
		foreach (var scriptElement in element.Elements("script"))
			_scripts.Add(new Script(scriptElement));

		ValidateScripts();
	}

	private void ValidateScripts()
	{
		var attributes = GetType().GetCustomAttributes(typeof(ScriptAttribute), inherit: false)
			.Cast<ScriptAttribute>();

		foreach (var attribute in attributes.Where(a => _scripts.All(s => s.Name != a.Name)))
		{
			_scripts.Add(new Script
			{
				Name = attribute.Name,
				Signature = attribute.Signature,
				Header = attribute.Header,
				Body = attribute.Body,
				Footer = attribute.Footer
			});
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