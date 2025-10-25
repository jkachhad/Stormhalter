using System.Collections.Generic;
using System.Windows;
using DigitalRune.Game.Interop;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class BrushPresentationTarget : InteropPresentationTarget
{
	private BrushGraphicsScreen _brushGraphicsScreen;

	public static readonly DependencyProperty BrushProperty =
		DependencyProperty.Register(nameof(Brush), typeof(SegmentBrush), typeof(BrushPresentationTarget),
			new FrameworkPropertyMetadata(default(SegmentBrush), FrameworkPropertyMetadataOptions.AffectsRender));

	public SegmentBrush Brush
	{
		get => (SegmentBrush)GetValue(BrushProperty);
		set => SetValue(BrushProperty, value);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();

		_brushGraphicsScreen = new BrushGraphicsScreen(GraphicsService, this);
		_brushGraphicsScreen.Initialize();
	}

	protected override IEnumerable<InteropGraphicsScreen> GetGraphicsScreens()
	{
		yield return _brushGraphicsScreen;
	}
}
