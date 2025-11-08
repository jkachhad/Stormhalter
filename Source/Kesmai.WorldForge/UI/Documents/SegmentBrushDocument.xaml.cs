using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Windows;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SegmentBrushDocument : UserControl
{
	private SegmentBrushViewModel? _viewModel;
	
	public SegmentBrushDocument()
	{
		InitializeComponent();
		
		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
	{
		_viewModel = args.NewValue as SegmentBrushViewModel;
	}

	private void OnAddComponentClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Brush is null)
			return;

		var picker = new ComponentsWindow
		{
			Owner = Window.GetWindow(this),
			ComponentFilter = provider => provider is not SegmentBrush brush || !ContainsBrush(brush, _viewModel.Brush)
		};

		var result = picker.ShowDialog();

		if (result is not true || picker.SelectedComponent is null)
			return;

		var entry = new SegmentBrushEntry(_viewModel.Brush)
		{
			Component = picker.SelectedComponent
		};

		_viewModel.Brush.Entries.Add(entry);

		_entriesList.SelectedItem = entry;
		_entriesList.ScrollIntoView(entry);
	}

	private void OnDeleteComponentClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Brush is null)
			return;

		if (_entriesList.SelectedItem is not SegmentBrushEntry entry)
			return;

		_viewModel.Brush.Entries.Remove(entry);
	}

	private static bool ContainsBrush(SegmentBrush source, SegmentBrush target)
	{
		// short-circuit for self-reference
		if (ReferenceEquals(source, target))
			return true;
		
		// perform a depth-first search to find the target brush
		return containsBrushRecursive(source, target, new HashSet<SegmentBrush>());

		static bool containsBrushRecursive(SegmentBrush current, SegmentBrush search, HashSet<SegmentBrush> visited)
		{
			if (!visited.Add(current))
				return false;

			foreach (var entry in current.Entries)
			{
				if (entry.Component is not SegmentBrush brush)
					continue;

				if (ReferenceEquals(brush, search))
					return true;

				if (containsBrushRecursive(brush, search, visited))
					return true;
			}

			return false;
		}
	}
}

public class SegmentBrushViewModel : ObservableRecipient
{
	private SegmentBrush? _brush;

	public string Name => "(Brushes)";

	public SegmentBrush? Brush
	{
		get => _brush;
		set => SetProperty(ref _brush, value);
	}
}
