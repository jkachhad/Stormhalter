using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Scripting;

/// <summary>
/// Represents a script with distinct name, signature, header, body and footer sections.
/// </summary>
public class Script : INotifyPropertyChanged
{
	private string _name = string.Empty;
	private string _signature = string.Empty;
	private string _header = string.Empty;
	private string _body = string.Empty;
	private string _footer = string.Empty;
	private bool _isEnabled;

	public event PropertyChangedEventHandler? PropertyChanged;

	private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (Equals(field, value)) return;
		field = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>Gets or sets the script name.</summary>
	public string Name { get => _name; set => SetField(ref _name, value); }
	
	/// <summary>Gets or sets the script signature.</summary>
	public string Signature { get => _signature; set => SetField(ref _signature, value); }

	/// <summary>Gets or sets the script header.</summary>
	public string Header { get => _header; set => SetField(ref _header, value); }

	/// <summary>Gets or sets the script body.</summary>
	public string Body { get => _body; set { SetField(ref _body, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmpty))); } }

	/// <summary>Gets or sets the script footer.</summary>
	public string Footer { get => _footer; set => SetField(ref _footer, value); }
	
	/// <summary>Gets or sets a value indicating whether this script is enabled.</summary>
	public bool IsEnabled { get => _isEnabled; set => SetField(ref _isEnabled, value); }
	
	public bool IsEmpty => String.IsNullOrWhiteSpace(Body);

	/// <summary>
	/// Initializes a new instance of the <see cref="Script"/> class.
	/// </summary>
	public Script()
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Script"/> class.
	/// </summary>
	public Script(XElement element)
	{
		Name = (string)element.Attribute("name");
		IsEnabled = (bool)element.Attribute("enabled");
			
		Body = (string)element;
	}
	
	/// <summary>
	/// Gets the XML representation of this script.
	/// </summary>
	public XElement GetSerializingElement()
	{
		var element = new XElement("script",
			new XAttribute("name", Name),
			new XAttribute("enabled", IsEnabled),
			new XCData(Body));
			
		return element;
	}
	
	/// <summary>
	/// Clones this script.
	/// </summary>
	public Script Clone()
	{
		return new Script()
		{
			Name = Name,
			Signature = Signature,
			Header = Header,
			Body = Body,
			Footer = Footer,
			IsEnabled = IsEnabled
		};
	}
}
