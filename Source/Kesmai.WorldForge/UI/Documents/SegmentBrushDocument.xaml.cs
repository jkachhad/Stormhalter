using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Windows;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SegmentBrushDocument : UserControl
{
	private SegmentBrush _segmentBrush;
	
	public SegmentBrushDocument()
	{
		InitializeComponent();
		
		var messenger = WeakReferenceMessenger.Default;

		messenger.Register<ActiveContentChanged>(this, (_, message) =>
		{
			if (message.Value is not SegmentBrush segmentBrush)
				return;

			_segmentBrush = segmentBrush;
		});
	}

	private void OnAddComponentClick(object sender, RoutedEventArgs e)
	{
		if (_segmentBrush is null)
			return;

		var picker = new ComponentsWindow
		{
			Owner = Window.GetWindow(this)
		};

		var result = picker.ShowDialog();

		if (!result.HasValue || result != true || picker.SelectedComponent is null)
			return;

		var entry = new SegmentBrushEntry(_segmentBrush)
		{
			Component = picker.SelectedComponent
		};

		_segmentBrush.Entries.Add(entry);

		_entriesList.SelectedItem = entry;
		_entriesList.ScrollIntoView(entry);
	}

	private void OnDeleteComponentClick(object sender, RoutedEventArgs e)
	{
		if (_segmentBrush is null)
			return;

		if (_entriesList.SelectedItem is not SegmentBrushEntry entry)
			return;

		_segmentBrush.Entries.Remove(entry);
	}
}

public class SegmentBrushViewModel : ObservableRecipient
{
	public string Name => "(Brushes)";
}
