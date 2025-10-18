using System.Collections.Generic;
using System.Windows;
using DigitalRune.Game.Interop;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge;

public class ComponentPresentationTarget : InteropPresentationTarget
{
	private ComponentGraphicsScreen _componentGraphicsScreen;
	
	public static readonly DependencyProperty ComponentProperty =
		DependencyProperty.Register(nameof(Component), typeof(TerrainComponent), typeof(ComponentPresentationTarget),
			new FrameworkPropertyMetadata(default(TerrainComponent), FrameworkPropertyMetadataOptions.AffectsRender));
		
	public TerrainComponent Component
	{
		get => (TerrainComponent)GetValue(ComponentProperty);
		set => SetValue(ComponentProperty, value);
	}
	
	public ComponentPresentationTarget()
	{
	}
	
	protected override void OnInitialize()
	{
		base.OnInitialize();
		
		_componentGraphicsScreen = new ComponentGraphicsScreen(GraphicsService, this);
		_componentGraphicsScreen.Initialize();
	}
	
	protected override IEnumerable<InteropGraphicsScreen> GetGraphicsScreens()
	{
		yield return _componentGraphicsScreen;
	}
}
