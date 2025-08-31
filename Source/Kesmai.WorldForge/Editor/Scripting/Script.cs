using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Scripting;

/// <summary>
/// Represents a script with distinct signature, header, body and footer sections.
/// </summary>
public class Script
{
	/// <summary>Gets or sets the script signature.</summary>
	public string Signature { get; set; } = string.Empty;

	/// <summary>Gets or sets the script header.</summary>
	public string Header { get; set; } = string.Empty;

	/// <summary>Gets or sets the script body.</summary>
	public string Body { get; set; } = string.Empty;

	/// <summary>Gets or sets the script footer.</summary>
	public string Footer { get; set; } = string.Empty;
}