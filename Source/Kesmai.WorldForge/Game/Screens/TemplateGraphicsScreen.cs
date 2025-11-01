using System;
using System.Collections.ObjectModel;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class TemplateGraphicsScreen : UIGraphicsScreen
{
	private TemplatePresentationTarget _presentationTarget;
	
	private bool _invalidated;
	
	private Grid _grid;
	private Button _resetButton;
	private StackPanel _componentFrames;        
	
	private readonly ObservableCollection<IComponentProvider> _editingProviders;

	public TemplateGraphicsScreen(IGraphicsService graphicsService, TemplatePresentationTarget presentationTarget) : base(graphicsService, presentationTarget)
	{
		_presentationTarget = presentationTarget ?? throw new ArgumentNullException(nameof(presentationTarget));
		_editingProviders = new ObservableCollection<IComponentProvider>();
	}

	public void Invalidate()
	{
		_invalidated = true;
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		
		_grid = new Grid()
		{
			IsVisible = false
		};
		
		_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });
		_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });

		_grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Star) });
		_grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
		
		_uiScreen.Children.Add(_grid);
		
		if (_resetButton is null)
		{
			_resetButton = new Button()
			{
				Content = new TextBlock()
				{
					Text = "Reset", FontSize = 10, HorizontalAlignment = HorizontalAlignment.Center,
					Font = "Tahoma", Foreground = Color.White,
				},
				Style = "Client-Button-Red",
				
				HorizontalAlignment = HorizontalAlignment.Stretch,
				
				ToolTip = "[CONTROL + Z]"
			};
		}
		_resetButton.Click += (o, args) => { Reset(); };
		
		_grid.AddChild(_resetButton, 2, 2);
		
		_componentFrames = new StackPanel()
		{
			VerticalAlignment = VerticalAlignment.Stretch,
		};
		
		_grid.AddChild(_componentFrames, 2, 1);
	}

	public void Reset()
	{
		var template = _presentationTarget.Template;
		
		if (template is null)
			return;

		template.Providers.Clear();
		
		foreach(var provider in _editingProviders)
			provider.AddComponent(template.Providers);
		
		Invalidate();
	}

	protected override void OnUpdate(TimeSpan deltaTime)
	{
		if (_invalidated)
		{
			var template = _presentationTarget.Template;
			var providers = template.Providers;
			
			_grid.IsVisible = providers.Any();

			_editingProviders.Clear();
			
			foreach(var provider in providers)
				provider.AddComponent(_editingProviders);
				
			_componentFrames.Children.Clear();
			
			for (int index = 0; index < providers.Count; index++)
			{
				var provider = providers[index];
				var providerFrame = provider.GetComponentFrame();
				
				providerFrame.AllowOrderUp = index > 0;
				providerFrame.AllowOrderDown = index < (providers.Count - 1);
				
				providerFrame.ShowDelete = false;
				
				providerFrame.OrderUp += OnFrameOrderUp;
				providerFrame.OrderDown += OnFrameOrderDown;
				
				_componentFrames.Children.Add(providerFrame);
			}
			
			_invalidated = false;
		}
		
		base.OnUpdate(deltaTime);
	}

	private void OnFrameOrderDown(object sender, EventArgs args)
	{
		if (sender is not ComponentFrame frame)
			return;
		
		var template = _presentationTarget.Template;
		var providers = template.Providers;
		
		var index = providers.IndexOf(frame.Provider);
		
		if (index < providers.Count - 1)
			providers.Move(index, index + 1);
		
		Invalidate();
	}

	private void OnFrameOrderUp(object sender, EventArgs args)
	{
		if (sender is not ComponentFrame frame)
			return;
		
		var template = _presentationTarget.Template;
		var providers = template.Providers;
		
		var index = providers.IndexOf(frame.Provider);
		
		if (index > 0)
			providers.Move(index, index - 1);
		
		Invalidate();
	}

	protected override void OnRender(RenderContext context)
	{
		base.OnRender(context);

		var graphicsService = context.GraphicsService;
		var graphicsDevice = graphicsService.GraphicsDevice;
		var spriteBatch = graphicsService.GetSpriteBatch();

		graphicsDevice.Clear(Color.Black);

		var template = _presentationTarget.Template;

		if (template is null)
			return;

		spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

		var width = (int)Math.Max(1, PresentationTarget.ActualWidth);
		var height = (int)Math.Max(1, PresentationTarget.ActualHeight);

		var renderSize = 160;
		var bounds = new Rectangle( (width - renderSize) / 2, (height - renderSize) / 2, renderSize, renderSize);

		foreach (var render in template.GetRenders())
		{
			foreach (var layer in render.Terrain)
			{
				var sprite = layer.Sprite;

				if (sprite is null)
					continue;

				var spriteBounds = bounds;

				if (sprite.Offset != Vector2F.Zero)
				{
					spriteBounds.Offset(
						(int)Math.Floor(sprite.Offset.X),
						(int)Math.Floor(sprite.Offset.Y));
				}

				spriteBatch.Draw( sprite.Texture, spriteBounds, null, render.Color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}
		}

		spriteBatch.End();
		
		RenderUI(context);
	}
}
