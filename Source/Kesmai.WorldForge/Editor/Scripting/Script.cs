using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.Scripting;

public class Script : ObservableObject
{
	private string _name;
	private bool _enabled;
		
	private List<string> _blocks;

	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}
		
	public bool IsEnabled
	{
		get => _enabled;
		set => SetProperty(ref _enabled, value);
	}
		
	public ScriptTemplate Template { get; set; }
		
	public List<string> Blocks => _blocks;

	public Script(string name, bool enabled, params string[] blocks)
	{
		_name = name;
		_enabled = enabled;
			
		_blocks = new List<string>(blocks);
	}

	public Script(XElement element)
	{
		_name = (string)element.Attribute("name");
		_enabled = (bool)element.Attribute("enabled");
			
		_blocks = new List<string>();

		foreach (var block in element.Elements("block"))
			_blocks.Add(block.Value);
	}

	public XElement GetXElement()
	{
		var element = new XElement("script",
			new XAttribute("name", _name),
			new XAttribute("enabled", _enabled));

		foreach (var segment in _blocks)
			element.Add(new XElement("block", new XCData(segment)));
			
		return element;
	}
		
	public void Parse(ScriptEditor editor)
	{
		_blocks.Clear();

		foreach (var segment in editor.GetNonReadOnlySegments())
			_blocks.Add(editor.Document.GetText(segment));
	}

	public Script Clone()
	{
		return new Script(_name, _enabled, _blocks.ToArray())
		{
			Template = Template,
		};
	}

        public override string ToString()
        {
                var segments = Template.GetSegments().ToList();
                var blocks = Blocks;

                var builder = new StringBuilder();

                if (!String.Equals(Name, "Internal", StringComparison.OrdinalIgnoreCase))
                {
                        try
                        {
                                var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
                                var segmentName = presenter.Segment?.Name;

                                if (!String.IsNullOrEmpty(segmentName))
                                {
                                        builder.Append($"using static Kesmai.Server.Segment.{segmentName}.Internal;\n");
                                }
                        }
                        catch
                        {
                        }
                }

                if (blocks.Any() && blocks.Count >= (segments.Count + 1))
                {
                        for (var i = 0; i < segments.Count; i++)
                        {
                                builder.Append(blocks[i]);
                                builder.Append(segments[i]);

                                if (i < (segments.Count - 1))
                                        continue;

                                builder.Append(blocks[i + 1]);
                        }
                }

                return builder.ToString();
        }
}