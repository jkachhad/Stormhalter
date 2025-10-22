using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DigitalRune.Game.Interop;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge;

public class ComponentPresentationTarget : InteropPresentationTarget
{
	private ComponentGraphicsScreen _componentGraphicsScreen;
	
	public static readonly DependencyProperty ProviderProperty =
		DependencyProperty.Register(nameof(Provider), typeof(IComponentProvider), typeof(ComponentPresentationTarget),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		
	public IComponentProvider Provider
	{
		get => (IComponentProvider)GetValue(ProviderProperty);
		set => SetValue(ProviderProperty, value);
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
