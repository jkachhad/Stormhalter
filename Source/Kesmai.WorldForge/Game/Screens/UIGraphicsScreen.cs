using System;
using CommonServiceLocator;
using DigitalRune.Game.Interop;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Kesmai.WorldForge;

public abstract class UIGraphicsScreen : InteropGraphicsScreen
{
	protected UIScreen _uiScreen;
	protected UIRenderer _uiRenderer;
	protected Theme _theme;
	
	protected Menu _contextMenu;
	
	public UIScreen UI => _uiScreen;

	protected UIRenderer UIRenderer => _uiRenderer ?? throw new InvalidOperationException("Initialize must be called before accessing the UI renderer.");

	protected UIGraphicsScreen(IGraphicsService graphicsService, InteropPresentationTarget presentationTarget) : base(graphicsService, presentationTarget)
	{
	}
	
	public void Initialize()
	{
		var services = ServiceLocator.Current;
		var contentManager = services.GetInstance<ContentManager>();

		_theme = contentManager.Load<Theme>(@"UI\Theme");
		_uiRenderer = new UIRenderer(GraphicsService.GraphicsDevice, _theme);
		
		_uiScreen = new UIScreen($"{PresentationTarget.GetHashCode()} GUI Screen", _uiRenderer);
		_uiScreen.Background = Color.Transparent;
		_uiScreen.ZIndex = int.MaxValue;

		PresentationTarget.UIManager.Screens.Add(_uiScreen);

		_contextMenu = new Menu();

		OnInitialize();
	}
	
	protected virtual void OnInitialize()
	{
	}

	protected void RenderUI(RenderContext context)
	{
		if (_uiScreen != null) 
			_uiScreen.Draw(context.DeltaTime);
	}
}
