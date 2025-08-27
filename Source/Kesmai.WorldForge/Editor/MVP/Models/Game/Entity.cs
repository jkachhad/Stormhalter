using System;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class Entity : ObservableObject, ICloneable, ISegmentObject
{
	private string _name;
	private string _notes;
	private string _group;
	
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
	
	public Entity()
	{
	}
		
	public Entity(XElement element)
	{
		_name = (string)element.Attribute("name");

		if (element.TryGetElement("notes", out var notesElement))
			_notes = (string)notesElement;
		
		if (element.TryGetElement("group", out var groupElement))
			_group = (string)groupElement;
	}
	
		
	public XElement GetXElement()
	{
		var element = new XElement("entity", 
			new XAttribute("name", _name));

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
		
		return clone;
	}
}