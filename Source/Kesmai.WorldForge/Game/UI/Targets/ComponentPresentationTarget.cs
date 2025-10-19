using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DigitalRune.Game.Interop;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge;

public class ComponentPresentationTarget : InteropPresentationTarget
{
	private ComponentGraphicsScreen _componentGraphicsScreen;
	private List<TerrainRender> _renders;
	
	public static readonly DependencyProperty ComponentProperty =
		DependencyProperty.Register(nameof(Component), typeof(TerrainComponent), typeof(ComponentPresentationTarget),
			new FrameworkPropertyMetadata(default(TerrainComponent), FrameworkPropertyMetadataOptions.AffectsRender));
		
	public TerrainComponent Component
	{
		get => (TerrainComponent)GetValue(ComponentProperty);
		set
		{
			var oldValue = Component;
			var newValue = value;
			
			SetValue(ComponentProperty, value);
			
			if (oldValue != newValue)
			{
				_renders = value.GetTerrain()
					.SelectMany(render => render.Terrain.Select(layer => new TerrainRender(layer, render.Color)))
					.OrderBy(render => render.Layer.Order).ToList();
			}
		}
	}

	public List<TerrainRender> Renders => _renders;

	public ComponentPresentationTarget()
	{
		_renders = new List<TerrainRender>();
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
