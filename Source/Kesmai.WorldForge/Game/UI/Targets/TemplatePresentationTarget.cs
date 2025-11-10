using System.Collections.Generic;
using System.Windows;
using DigitalRune.Game.Interop;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class TemplatePresentationTarget : InteropPresentationTarget
{
	private TemplateGraphicsScreen _graphicsScreen;

	public static readonly DependencyProperty TemplateProperty =
		DependencyProperty.Register(nameof(Template), typeof(SegmentTemplate), typeof(TemplatePresentationTarget),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	public SegmentTemplate? Template
	{
		get => (SegmentTemplate)GetValue(TemplateProperty);
		set => SetValue(TemplateProperty, value);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();

		_graphicsScreen = new TemplateGraphicsScreen(GraphicsService, this);
		_graphicsScreen.Initialize();
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);

		if (e.Property != TemplateProperty)
			return;

		Invalidate();
	}

	public void Invalidate()
	{
		if (_graphicsScreen != null)
			_graphicsScreen.Invalidate();
	}

	protected override IEnumerable<InteropGraphicsScreen> GetGraphicsScreens()
	{
		yield return _graphicsScreen;
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);

		var screen = _graphicsScreen.UI;
		
		if (screen != null)
		{
			screen.Width = (float)sizeInfo.NewSize.Width;
			screen.Height = (float)sizeInfo.NewSize.Height;
		}
	}
}
