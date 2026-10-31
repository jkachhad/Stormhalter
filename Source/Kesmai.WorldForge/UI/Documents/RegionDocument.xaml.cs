using System;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using System.Windows.Controls;
using CommonServiceLocator;

namespace Kesmai.WorldForge.UI.Documents;

public partial class RegionDocument : UserControl
{
	private bool _isRegistered;

	// the palette shows while a drawing tool is picked: docked beside the map, or in its own window when floated
	private bool _paletteWanted;
	private Window _floatWindow;
	private Window _floatOwner;
	private System.ComponentModel.CancelEventHandler _ownerClosing;

	public RegionDocument()
	{
		InitializeComponent();

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
		DataContextChanged += (_, _) => Refresh();
	}

	private void OnRegionToolsMoved(object sender, EventArgs args) => _toolsTray.HorizontalAlignment = RegionToolsPosition.Alignment;

	private void OnLoaded(object sender, RoutedEventArgs args)
	{
		Refresh();

		// View > Region tools position
		_toolsTray.HorizontalAlignment = RegionToolsPosition.Alignment;
		RegionToolsPosition.Changed -= OnRegionToolsMoved;
		RegionToolsPosition.Changed += OnRegionToolsMoved;

		if (!_isRegistered)
		{
			WeakReferenceMessenger.Default.Register<RegionDocument, RegionFilterChanged>(this,
				static (recipient, _) => recipient.Refresh());

			WeakReferenceMessenger.Default.Register<RegionDocument, RegionVisibilityChanged>(this,
				static (recipient, message) => recipient.OnVisibilityChanged(message.PropertyName));

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

		RegionToolsPosition.Changed -= OnRegionToolsMoved;

		// another region's tab takes over the palette; this one floats it again when it comes back
		_paletteWanted = false;
		CloseFloatWindow();
		ShowPalette();

		_isRegistered = false;
	}

	private void Refresh()
	{
		if (DataContext is SegmentRegion region)
			region.UpdateTiles();

		if (_presenter is not null && _presenter.WorldScreen is not null)
			_presenter.WorldScreen.InvalidateRender();
	}

	private void OnVisibilityChanged(string propertyName)
	{
		// overlays (teleporters, spawns, comments) only need a redraw; wall and door settings change tile renders.
		if (RegionVisibility.AffectsTerrain(propertyName))
			Refresh();
		else if (_presenter is not null && _presenter.WorldScreen is not null)
			_presenter.WorldScreen.InvalidateRender();
	}

	private void OnToolChanged(Tool tool)
	{
		_paletteWanted = tool is DrawTool or PaintTool or FillTool;
		ShowPalette();
	}

	/// <summary>Shows the palette where it belongs, or hides it while no drawing tool is picked.</summary>
	private void ShowPalette()
	{
		var floating = ComponentPaletteSettings.Floating;
		var docked = _paletteWanted && !floating;

		if (_paletteWanted && floating)
		{
			OpenFloatWindow();
			_floatWindow?.Show();
		}
		else
		{
			_floatWindow?.Hide();
		}

		_componentsDock.Visibility = docked ? Visibility.Visible : Visibility.Collapsed;
		_componentsSplitter.Visibility = docked ? Visibility.Visible : Visibility.Collapsed;
		_componentsColumn.Width = docked ? new GridLength(ComponentPaletteSettings.DockWidth) : new GridLength(0);
		_componentsColumn.MinWidth = docked ? 150 : 0;
		_componentsPanel.ShowFloating(floating);
	}

	private void OnFloatToggled(object sender, EventArgs args)
	{
		ComponentPaletteSettings.Floating = !ComponentPaletteSettings.Floating;
		ComponentPaletteSettings.Save();

		if (!ComponentPaletteSettings.Floating)
			CloseFloatWindow();

		ShowPalette();
	}

	private void OnComponentsSplitterDragged(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs args)
	{
		if (_componentsColumn.ActualWidth < 150)
			return;

		ComponentPaletteSettings.DockWidth = _componentsColumn.ActualWidth;
		ComponentPaletteSettings.Save();
	}

	/// <summary>Moves the palette out of the dock into a window of its own, where it was last left.</summary>
	private void OpenFloatWindow()
	{
		if (_floatWindow is not null)
			return;

		_componentsDock.Child = null;

		var bounds = ComponentPaletteSettings.FloatBounds;
		var owner = Window.GetWindow(this);

		_floatWindow = new Window
		{
			Title = "Components",
			Content = _componentsPanel,
			Owner = owner,
			ShowInTaskbar = false,
			WindowStyle = WindowStyle.ToolWindow,
			Width = bounds.IsEmpty ? 420 : bounds.Width,
			Height = bounds.IsEmpty ? 700 : bounds.Height,
			FontFamily = FontFamily,
			FontSize = FontSize,
		};

		// a saved place that is off every screen (a monitor since unplugged) starts beside the editor instead
		if (!bounds.IsEmpty && bounds.IntersectsWith(new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
			    SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight)))
		{
			_floatWindow.WindowStartupLocation = WindowStartupLocation.Manual;
			_floatWindow.Left = bounds.Left;
			_floatWindow.Top = bounds.Top;
		}
		else
		{
			_floatWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
		}

		// WorldForge closing closes this window too; that isn't the user docking the palette, so it stays floated. If the
		// close is called off (unsaved changes), the palette comes back
		if (owner is not null)
		{
			_ownerClosing = (_, _) =>
			{
				CloseFloatWindow();
				Dispatcher.BeginInvoke(ShowPalette, System.Windows.Threading.DispatcherPriority.Background);
			};
			_floatOwner = owner;
			owner.Closing += _ownerClosing;
		}

		_floatWindow.LocationChanged += (_, _) => RememberFloatBounds();
		_floatWindow.SizeChanged += (_, _) => RememberFloatBounds();

		// closing the window docks the palette again
		_floatWindow.Closing += (_, _) =>
		{
			if (_floatWindow is null)
				return;

			RememberFloatBounds();
			ReleaseFloatWindow();

			if (_paletteWanted)
			{
				ComponentPaletteSettings.Floating = false;
				ComponentPaletteSettings.Save();
				ShowPalette();
			}
		};
	}

	private void RememberFloatBounds()
	{
		if (_floatWindow is not { IsVisible: true, WindowState: WindowState.Normal })
			return;

		ComponentPaletteSettings.FloatBounds = new Rect(_floatWindow.Left, _floatWindow.Top, _floatWindow.Width, _floatWindow.Height);
		ComponentPaletteSettings.Save();
	}

	/// <summary>Puts the palette back in the dock and forgets the window.</summary>
	private void ReleaseFloatWindow()
	{
		if (_floatWindow is null)
			return;

		_floatWindow.Content = null;
		_floatWindow = null;
		_componentsDock.Child = _componentsPanel;

		if (_floatOwner is not null)
			_floatOwner.Closing -= _ownerClosing;

		_floatOwner = null;
		_ownerClosing = null;
	}

	private void CloseFloatWindow()
	{
		if (_floatWindow is null)
			return;

		var window = _floatWindow;

		RememberFloatBounds();
		ReleaseFloatWindow();
		window.Close();
	}
}
