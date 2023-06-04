using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kesmai.WorldForge.Scripting;

public class ScriptTemplateProvider
{
	private Dictionary<Type, ScriptTemplate> _templates;
		
	public ScriptTemplateProvider()
	{
		_templates = new Dictionary<Type, ScriptTemplate>();

		var templateType = typeof(ScriptTemplate);
			
		var assembly = Assembly.GetExecutingAssembly();
		var types = assembly.GetTypes().Where(t => t.IsSubclassOf(templateType));

		foreach (var type in types)
		{
			if (Activator.CreateInstance(type) is ScriptTemplate template)
				_templates.Add(type, template);
		}
	}

	public bool TryGetTemplate(Type type, out ScriptTemplate template)
	{
		return _templates.TryGetValue(type, out template);
	}
}