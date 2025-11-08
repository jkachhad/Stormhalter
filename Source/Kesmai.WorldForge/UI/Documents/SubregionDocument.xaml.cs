using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SubregionDocument : UserControl
{
	private SubregionViewModel? _viewModel;
	
	public SubregionDocument()
	{
		InitializeComponent();

		DataContextChanged += OnDataContextChanged;
    }

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (_viewModel != null)
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = e.NewValue as SubregionViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		UpdatePresenter();
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SubregionViewModel.Subregion) || e.PropertyName == nameof(SubregionViewModel.Region))
		{
			UpdatePresenter();
		}
	}

	private void UpdatePresenter()
	{
		if (_presenter == null || _viewModel == null || _viewModel.Subregion == null || _viewModel.Region == null)
			return;

		_presenter.Region = _viewModel.Region;
		_presenter.Subregion = _viewModel.Subregion;
		_presenter.SetSubregion(_viewModel.Subregion);

		var initialBounds = _viewModel.Subregion.Rectangles.FirstOrDefault();

		if (initialBounds != null)
		{
			_presenter.Bounds = initialBounds;
			_presenter.SetBounds(initialBounds);
		}
	}

	private void RectanglesSelectionChanged(object sender, SelectionChangedEventArgs args)
	{
		var selectedBounds = args.AddedItems.OfType<SegmentBounds>().FirstOrDefault();

		if (selectedBounds is null && sender is DataGrid grid)
			selectedBounds = grid.SelectedItem as SegmentBounds;

		if (selectedBounds is null || !selectedBounds.IsValid)
			return;

		_presenter.SetBounds(selectedBounds);
	}
}

public class SubregionViewModel : ObservableRecipient
{
	private SegmentSubregion? _subregion;
	private SegmentRegion? _region;

	public string Name => "(Subregions)";

	public SegmentSubregion? Subregion
	{
		get => _subregion;
		set => SetProperty(ref _subregion, value);
	}

	public SegmentRegion? Region
	{
		get => _region;
		set => SetProperty(ref _region, value);
	}
}
