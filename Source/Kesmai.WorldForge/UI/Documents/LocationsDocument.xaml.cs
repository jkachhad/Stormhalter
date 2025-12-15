using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.UI.Documents;

public partial class LocationsDocument : UserControl
{
	private LocationsViewModel? _viewModel;
	
	public LocationsDocument()
	{
		InitializeComponent();
		
		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
	{
		if (_viewModel != null)
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = args.NewValue as LocationsViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		UpdatePresenter();
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == nameof(LocationsViewModel.Location) || args.PropertyName == nameof(LocationsViewModel.Region))
			UpdatePresenter();
	}

	private void UpdatePresenter()
	{
		if (_presenter == null || _viewModel == null || _viewModel.Location == null || _viewModel.Region == null)
			return;

		_presenter.Region = _viewModel.Region;
		_presenter.Location = _viewModel.Location;
		_presenter.SetCameraLocation(_viewModel.Location);

		_presenter.Focus();
	}
}

public class LocationsViewModel : ObservableRecipient
{
	public string Name => "(Locations)";
	
	private SegmentLocation? _location;
	private SegmentRegion? _region;

	public SegmentLocation? Location
	{
		get => _location;
		set => SetProperty(ref _location, value);
	}

	public SegmentRegion? Region
	{
		get => _region;
		set => SetProperty(ref _region, value);
	}
}
