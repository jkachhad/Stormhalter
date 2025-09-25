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

public class RegionPresentationTarget : WorldPresentationTarget
{
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return new WorldGraphicsScreen(graphicsService, this);
	}
}

public class LocationsPresentationTarget : WorldPresentationTarget
{
	private LocationsGraphicsScreen _screen;
		
	public override bool AllowInput => true;
		
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return (_screen = new LocationsGraphicsScreen(graphicsService, this));
	}

	public void SetLocation(SegmentLocation location)
	{
		if (_screen != null)
			_screen.SetLocation(location.X, location.Y);
	}

}

public class SpawnsPresentationTarget : WorldPresentationTarget
{
	private SpawnsGraphicsScreen _screen;

	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return (_screen = new SpawnsGraphicsScreen(graphicsService, this));
	}

	public void SetLocation(Spawner spawner)
	{
		if (_screen != null)
		{
			_screen.SetSpawner(spawner);
		}
    }
}

public class SubregionsPresentationTarget : WorldPresentationTarget
{
	private SubregionsGraphicsScreen _screen;
		
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return (_screen = new SubregionsGraphicsScreen(graphicsService, this));
	}

	public void SetSubregion(SegmentSubregion subregion)
	{
		if (_screen != null)
			_screen.SetSubregion(subregion);
	}
}

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