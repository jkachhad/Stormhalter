using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Kesmai.WorldForge.UI;

public sealed class DropHighlightAdorner : Adorner
{
	private static readonly Brush Fill = CreateFillBrush();
	private static readonly Pen Outline = CreateOutlinePen();

	private readonly TreeViewItem _treeViewItem;

	public DropHighlightAdorner(TreeViewItem treeViewItem) : base(treeViewItem)
	{
		_treeViewItem = treeViewItem;
		IsHitTestVisible = false;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		var rect = GetHeaderBounds();

		if (rect.Width <= 0 || rect.Height <= 0)
			return;

		drawingContext.DrawRoundedRectangle(Fill, Outline, rect, 3, 3);
	}

	private Rect GetHeaderBounds()
	{
		_treeViewItem.ApplyTemplate();

		if (_treeViewItem.Template?.FindName("PART_Header", _treeViewItem) is FrameworkElement header)
		{
			var transform = header.TransformToAncestor(_treeViewItem);
			var headerBounds = new Rect(new Point(0, 0), header.RenderSize);
			return transform.TransformBounds(headerBounds);
		}

		return new Rect(new Point(0, 0), _treeViewItem.RenderSize);
	}

	private static Brush CreateFillBrush()
	{
		var brush = new SolidColorBrush(Color.FromArgb(48, 0, 120, 215));
		brush.Freeze();
		return brush;
	}

	private static Pen CreateOutlinePen()
	{
		var brush = new SolidColorBrush(Color.FromArgb(160, 0, 120, 215));
		brush.Freeze();
		var pen = new Pen(brush, 1);
		pen.Freeze();
		return pen;
	}
}