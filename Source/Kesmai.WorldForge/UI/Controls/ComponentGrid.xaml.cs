using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Kesmai.WorldForge.UI;

public partial class ComponentGrid : ListBox
{
	/// <summary>
	/// 0 lays the tiles out as a grid that scrolls down; 1 to 4 lays them out in that many rows that scroll left to
	/// right, and sizes the grid to fit them.
	/// </summary>
	public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(nameof(Rows), typeof(int),
		typeof(ComponentGrid), new FrameworkPropertyMetadata(0, (d, _) => ((ComponentGrid)d).UpdateLayoutMode()));

	/// <summary>How large the tiles are drawn: 1 is their full 100 pixels.</summary>
	public static readonly DependencyProperty TileScaleProperty = DependencyProperty.Register(nameof(TileScale), typeof(double),
		typeof(ComponentGrid), new FrameworkPropertyMetadata(1.0, (d, _) => ((ComponentGrid)d).UpdateLayoutMode()));

	/// <summary>The wrap panel's orientation: Horizontal fills rows and scrolls down, Vertical fills columns and scrolls across.</summary>
	public static readonly DependencyProperty PanelOrientationProperty = DependencyProperty.Register(nameof(PanelOrientation),
		typeof(Orientation), typeof(ComponentGrid), new FrameworkPropertyMetadata(Orientation.Horizontal));

	public int Rows
	{
		get => (int)GetValue(RowsProperty);
		set => SetValue(RowsProperty, value);
	}

	public double TileScale
	{
		get => (double)GetValue(TileScaleProperty);
		set => SetValue(TileScaleProperty, value);
	}

	public Orientation PanelOrientation
	{
		get => (Orientation)GetValue(PanelOrientationProperty);
		set => SetValue(PanelOrientationProperty, value);
	}

	/// <summary>The tiles' scale as a transform for the item template (a binding inside a transform can't find this grid).</summary>
	public static readonly DependencyProperty TileTransformProperty = DependencyProperty.Register(nameof(TileTransform),
		typeof(Transform), typeof(ComponentGrid), new FrameworkPropertyMetadata(Transform.Identity));

	public Transform TileTransform
	{
		get => (Transform)GetValue(TileTransformProperty);
		set => SetValue(TileTransformProperty, value);
	}

	// a tile is 100 pixels with a 2 pixel border and 4 below it, a label about 22 high, and the list item's padding
	private const double TileHeight = 130;
	private const double ItemPadding = 6;
	private const double ScrollBarHeight = 20;

	public ComponentGrid()
	{
		InitializeComponent();

		PreviewMouseWheel += OnPreviewMouseWheel;
		UpdateLayoutMode();
	}

	private void UpdateLayoutMode()
	{
		var across = Rows > 0;

		PanelOrientation = across ? Orientation.Vertical : Orientation.Horizontal;
		TileTransform = TileScale == 1.0 ? Transform.Identity : new ScaleTransform(TileScale, TileScale);

		ScrollViewer.SetHorizontalScrollBarVisibility(this, across ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled);
		ScrollViewer.SetVerticalScrollBarVisibility(this, across ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto);

		// rows across are as tall as the rows; the grid fills the room it has and scrolls down
		if (across)
		{
			Height = Rows * (TileHeight * TileScale + ItemPadding) + ScrollBarHeight + ItemPadding;
			VerticalAlignment = VerticalAlignment.Top;
		}
		else
		{
			ClearValue(HeightProperty);
			VerticalAlignment = VerticalAlignment.Stretch;
		}
	}

	/// <summary>In the rows that scroll left to right, the mouse wheel scrolls across.</summary>
	private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
	{
		if (Rows <= 0 || FindScrollViewer(this) is not { } scrollViewer)
			return;

		for (var i = 0; i < 3; i++)
		{
			if (args.Delta > 0)
				scrollViewer.LineLeft();
			else
				scrollViewer.LineRight();
		}

		args.Handled = true;
	}

	private static ScrollViewer FindScrollViewer(DependencyObject parent)
	{
		for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			var child = VisualTreeHelper.GetChild(parent, i);

			if (child is ScrollViewer scrollViewer)
				return scrollViewer;

			if (FindScrollViewer(child) is { } found)
				return found;
		}

		return null;
	}
}
