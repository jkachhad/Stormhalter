using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace Kesmai.WorldForge.Scripting;

public class ScriptBackgroundRenderer : IBackgroundRenderer
{
	private static SolidColorBrush _brush = new (Color.FromArgb(0x20, 0x40, 0x40, 0x40));
		
	private ScriptEditor _editor;
		
	public KnownLayer Layer { get; } = KnownLayer.Background;
		
	public ScriptBackgroundRenderer(ScriptEditor editor)
	{
		_editor = editor;
	}

	public void Draw(TextView textView, DrawingContext drawingContext)
	{
		if (_editor.Document == null)
			return;

		textView.EnsureVisualLines();

		foreach (var segment in _editor.ReadOnlySegments)
		{
			foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
			{
				drawingContext.DrawRectangle(_brush, null, 
					new Rect(rect.Location, new Size(rect.Width, rect.Height)));
			}
		}
	}
}