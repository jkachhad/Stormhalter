using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.UI.Documents;

public class SegmentInternalScriptTemplate : ScriptTemplate
{
	public override IEnumerable<string> GetSegments()
	{
		yield return "public class Internal\n{";
		yield return "}";
	}
}

public class SegmentDefinitionScriptTemplate : ScriptTemplate
{
	public override IEnumerable<string> GetSegments()
	{
		yield return "#load \"WorldForge\"\n";
	}
}
	
public partial class SegmentDocument : UserControl
{
	public SegmentDocument()
	{
		InitializeComponent();
    }

    private void OnExpandAllClicked(object sender, RoutedEventArgs e)
    {
        _internalEditor.ExpandAllFolds();
    }

    private void OnCollapseAllClicked(object sender, RoutedEventArgs e)
    {
        _internalEditor.CollapseAllFolds();
    }
}

public class SegmentViewModel : ObservableObject, IDisposable
{
	public string Name => "(Segment)";
		
	private Segment _segment;
		
	public Segment Segment
	{
		get => _segment;
		set => SetProperty(ref _segment, value);
	}
		
	public SegmentViewModel(Segment segment)
	{
		_segment = segment;
    }
    public void Dispose()
    {
        _segment = null;
    }
}
