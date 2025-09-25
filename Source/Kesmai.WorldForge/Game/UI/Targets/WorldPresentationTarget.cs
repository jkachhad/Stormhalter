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
	private bool _isRendering;
		
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

	public virtual bool AllowInput => true;
		
	protected WorldPresentationTarget()
	{
		EnableAlpha = true;
	}
	
	public abstract WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService);

	protected override void OnInitialize()
	{
		base.OnInitialize();
		
		_worldScreen = CreateGraphicsScreen(GraphicsService);
		_worldScreen.Initialize();
	}
	
	protected override IEnumerable<InteropGraphicsScreen> GetGraphicsScreens()
	{
		yield return _worldScreen;
	}
}