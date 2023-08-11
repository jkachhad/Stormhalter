using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.Document;

namespace Kesmai.WorldForge.Scripting;

public abstract class ScriptTemplate
{
	public void Apply(ScriptEditor editor, Script script)
	{
		editor.Clear();
		editor.ReadOnlySegments.Clear();
			
		OnApply(editor, script);
			
		var document = editor.Document;
		var blocks = script.Blocks;
		var readonlySegments = editor.ReadOnlySegments.ToList();
				
		if (blocks.Any() && blocks.Count >= (readonlySegments.Count + 1))
		{
			for (var i = 0; i < readonlySegments.Count; i++)
			{
				var segment = readonlySegments[i];
					
				document.Insert(segment.StartOffset, blocks[i], AnchorMovementType.AfterInsertion);
						
				if (i < (readonlySegments.Count - 1))
					continue;

				document.Insert(segment.EndOffset, blocks[i + 1], AnchorMovementType.BeforeInsertion);
			}
		}
			
		document.UndoStack.ClearAll();
	}

	protected virtual void OnApply(ScriptEditor editor, Script script)
	{
		foreach (var segment in GetSegments())
			editor.Insert(segment, true);
	}

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