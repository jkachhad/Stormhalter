using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using System.Windows.Controls;
using CommonServiceLocator;

namespace Kesmai.WorldForge.UI.Documents;

public partial class RegionDocument : UserControl
{
	private bool _isRegistered;

	public RegionDocument()
	{
		InitializeComponent();

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
		DataContextChanged += (_, _) => Refresh();
	}

	private void OnLoaded(object sender, RoutedEventArgs args)
	{
		Refresh();

		if (!_isRegistered)
		{
			WeakReferenceMessenger.Default.Register<RegionDocument, RegionFilterChanged>(this,
				static (recipient, _) => recipient.Refresh());

			WeakReferenceMessenger.Default.Register<RegionDocument, RegionVisibilityChanged>(this,
				static (recipient, _) => recipient.Refresh());

			WeakReferenceMessenger.Default.Register<RegionDocument, RegionToolChanged>(this,
				(recipient, message) => recipient.OnToolChanged(message.Value));

			_isRegistered = true;
		}
		
		var regionToolbar = ServiceLocator.Current.GetInstance<RegionToolbar>();

		if (regionToolbar.SelectedTool is not null)
			OnToolChanged(regionToolbar.SelectedTool);

		_presenter.Focus();
	}

	private void OnUnloaded(object sender, RoutedEventArgs args)
	{
		if (_isRegistered)
			WeakReferenceMessenger.Default.UnregisterAll(this);
		
		_componentsPanel.Visibility = Visibility.Collapsed;
		
		_isRegistered = false;
	}
	
	private void Refresh()
	{
		if (DataContext is SegmentRegion region)
			region.UpdateTiles();

		if (_presenter is not null && _presenter.WorldScreen is not null)
			_presenter.WorldScreen.InvalidateRender();
	}
	
	private void OnToolChanged(Tool tool)
	{
		if (tool is DrawTool or PaintTool)
			_componentsPanel.Visibility = Visibility.Visible;
		else
			_componentsPanel.Visibility = Visibility.Collapsed;
	}

	private void OnCategorySelected(object sender, RoutedPropertyChangedEventArgs<object> args)
	{
		var componentPalette = ServiceLocator.Current.GetInstance<ComponentPalette>();
		
		if (args.NewValue is ComponentsCategory category)
			componentPalette.SelectedCategory = category;
	}
}
