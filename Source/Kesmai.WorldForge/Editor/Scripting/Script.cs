using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Scripting;

/// <summary>
/// Represents a script with distinct name, signature, header, body and footer sections.
/// </summary>
public class Script
{
	/// <summary>Gets or sets the script name.</summary>
	public string Name { get; set; } = string.Empty;
	
	/// <summary>Gets or sets the script signature.</summary>
	public string Signature { get; set; } = string.Empty;

	/// <summary>Gets or sets the script header.</summary>
	public string Header { get; set; } = string.Empty;

	/// <summary>Gets or sets the script body.</summary>
	public string Body { get; set; } = string.Empty;

	/// <summary>Gets or sets the script footer.</summary>
	public string Footer { get; set; } = string.Empty;
	
	/// <summary>Gets or sets a value indicating whether this script is enabled.</summary>
	public bool IsEnabled { get; set; }

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
			
		Body = (string)element.Element("body") ?? string.Empty;
	}
	
	/// <summary>
	/// Gets the XML representation of this script.
	/// </summary>
	public XElement GetXElement()
	{
		var element = new XElement("script",
			new XAttribute("name", Name),
			new XAttribute("enabled", IsEnabled));
		
		element.Add(new XElement("body", new XCData(Body)));
			
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