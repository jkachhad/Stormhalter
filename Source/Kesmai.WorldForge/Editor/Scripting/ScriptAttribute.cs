using System;

namespace Kesmai.WorldForge.Scripting;

/// <summary>
/// Attribute for marking scripts available on a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ScriptAttribute : Attribute
{
	/// <summary>Gets the script name.</summary>
	public string Name { get; }

	/// <summary>Gets the script signature.</summary>
	public string Signature { get; }

	/// <summary>Gets the script header.</summary>
	public string Header { get; }

	/// <summary>Gets the script footer.</summary>
	public string Footer { get; }

	/// <summary>Gets the optional script body.</summary>
	public string Body { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ScriptAttribute"/> class.
	/// </summary>
	/// <param name="name">The script name.</param>
	/// <param name="signature">The script signature.</param>
	/// <param name="header">The script header.</param>
	/// <param name="footer">The script footer.</param>
	/// <param name="body">Optional script body.</param>
	public ScriptAttribute(string name, string signature, string header, string footer, string body = "")
	{
		Name = name;
		Signature = signature;
		Header = header;
		Footer = footer;
		Body = body;
	}
}