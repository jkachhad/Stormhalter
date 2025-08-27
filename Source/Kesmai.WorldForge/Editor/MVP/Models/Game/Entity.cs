using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Roslyn;

namespace Kesmai.WorldForge;

public class Entity : ObservableObject, ICloneable, ISegmentObject
{
        private string _name;
        private string _notes;
        private string _group;
        
        public ObservableCollection<EntityScript> Scripts { get; } = new();

        public OnDeathScript OnDeath { get; } = new();

        public OnIncomingPlayerScript OnIncomingPlayer { get; } = new();
	
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
                Scripts.Add(OnDeath);
                Scripts.Add(OnIncomingPlayer);
        }

        public Entity(XElement element) : this()
        {
                _name = (string)element.Attribute("name");

                if (element.TryGetElement("notes", out var notesElement))
                        _notes = (string)notesElement;

                if (element.TryGetElement("group", out var groupElement))
                        _group = (string)groupElement;

                if (element.TryGetElement("ondeath", out var onDeathElement))
                        OnDeath.Body = (string)onDeathElement;

                if (element.TryGetElement("onincomingplayer", out var onIncomingElement))
                        OnIncomingPlayer.Body = (string)onIncomingElement;
        }
	
		
	public XElement GetXElement()
	{
		var element = new XElement("entity", 
			new XAttribute("name", _name));

                if (!String.IsNullOrEmpty(_notes))
                        element.Add(new XElement("notes", _notes));

                if (!String.IsNullOrEmpty(_group))
                        element.Add(new XElement("group", _group));

                if (!String.IsNullOrWhiteSpace(OnDeath.Body))
                        element.Add(new XElement("ondeath", OnDeath.Body));

                if (!String.IsNullOrWhiteSpace(OnIncomingPlayer.Body))
                        element.Add(new XElement("onincomingplayer", OnIncomingPlayer.Body));

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

                clone.OnDeath.Body = OnDeath.Body;
                clone.OnIncomingPlayer.Body = OnIncomingPlayer.Body;

                return clone;
        }
}