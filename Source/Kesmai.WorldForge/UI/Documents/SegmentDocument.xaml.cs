using System.Collections.Generic;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.UI.Documents
{
	public class SegmentScriptTemplate : ScriptTemplate
	{
		public override IEnumerable<string> GetSegments()
		{
			yield return "public class Internal\n{";
			yield return "}";
		}
	}
	
	public partial class SegmentDocument : UserControl
	{
		public SegmentDocument()
		{
			InitializeComponent();
		}
	}

	public class SegmentViewModel : ObservableObject
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
	}
}