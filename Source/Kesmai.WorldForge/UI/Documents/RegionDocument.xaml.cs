using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using System.Windows.Controls;

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

	private void OnLoaded(object? sender, RoutedEventArgs args)
	{
		Refresh();
		
		RegisterMessages();

		_presenter.Focus();
	}

	private void OnUnloaded(object? sender, RoutedEventArgs args)
	{
		UnregisterMessages();
	}

	private void RegisterMessages()
	{
		if (_isRegistered)
			return;

		WeakReferenceMessenger.Default.Register<RegionDocument, RegionFilterChanged>(this,
			static (recipient, _) => recipient.Refresh());

		WeakReferenceMessenger.Default.Register<RegionDocument, RegionVisibilityChanged>(this,
			static (recipient, _) => recipient.Refresh());

		WeakReferenceMessenger.Default.Register<RegionDocument, ToolSelected>(this,
			(recipient, message) => recipient.OnToolSelected(message));
		
		_isRegistered = true;
	}

	private void UnregisterMessages()
	{
		if (!_isRegistered)
			return;

		WeakReferenceMessenger.Default.UnregisterAll(this);
		_componentsPanel.Visibility = Visibility.Collapsed;
		_isRegistered = false;
	}

	private void OnToolSelected(ToolSelected message)
	{
		if (message.Value is DrawTool or PaintTool)
			_componentsPanel.Visibility = Visibility.Visible;
		else
			_componentsPanel.Visibility = Visibility.Collapsed;
	}

	private void Refresh()
	{
		if (DataContext is SegmentRegion region)
			region.UpdateTiles();

		if (_presenter is not null && _presenter.WorldScreen is not null)
			_presenter.WorldScreen.InvalidateRender();
	}
}
