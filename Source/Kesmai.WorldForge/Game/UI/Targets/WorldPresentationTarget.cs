using CommonServiceLocator;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Interop;
using DigitalRune.ServiceLocation;
using Kesmai.WorldForge.Editor;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Windows;
using DigitalRune.Game.Interop;

namespace Kesmai.WorldForge;

public abstract class WorldPresentationTarget : InteropPresentationTarget
{
	protected WorldGraphicsScreen _worldScreen;
	
	public WorldGraphicsScreen WorldScreen => _worldScreen;
		
	public static readonly DependencyProperty RegionProperty =
		DependencyProperty.Register(nameof(Region), typeof(SegmentRegion), typeof(WorldPresentationTarget),
			new FrameworkPropertyMetadata(
				default(SegmentRegion), FrameworkPropertyMetadataOptions.AffectsRender));
		
	public SegmentRegion Region
	{
		get => (SegmentRegion)GetValue(RegionProperty);
		set => SetValue(RegionProperty, value);
	}
		
	protected WorldPresentationTarget()
	{
		EnableAlpha = true;
	}
	
	public abstract WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService);

	protected override void OnInitialize()
	{
		// Capture mouse within the control so we can pan the view by dragging.
		InputManager.Mouse.CaptureMouseWithin = true;
		
		base.OnInitialize();
		
		_worldScreen = CreateGraphicsScreen(GraphicsService);
		_worldScreen.Initialize();
	}
	
	protected override IEnumerable<InteropGraphicsScreen> GetGraphicsScreens()
	{
		yield return _worldScreen;
	}
	
	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);

		var screen = _worldScreen.UI;
		
		if (screen != null)
		{
			screen.Width = (float)sizeInfo.NewSize.Width;
			screen.Height = (float)sizeInfo.NewSize.Height;
		}
	}
}