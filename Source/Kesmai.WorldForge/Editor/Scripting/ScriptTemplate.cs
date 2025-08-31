using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.Document;

namespace Kesmai.WorldForge.Scripting;

public abstract class ScriptTemplate
{
	public abstract IEnumerable<string> GetSegments();
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ScriptTemplateAttribute : Attribute
{
	public string Name { get; private set; }
	public Type TemplateType { get; private set; }
		
	public ScriptTemplateAttribute(string name, Type templateType)
	{
		if (String.IsNullOrEmpty(name))
			throw new ArgumentNullException(nameof(name));
			
		if (!templateType.IsAssignableTo(typeof(ScriptTemplate)))
			throw new ArgumentException("The specified type must be child of ScriptTemplate.");

		Name = name;
		TemplateType = templateType;
	}
}