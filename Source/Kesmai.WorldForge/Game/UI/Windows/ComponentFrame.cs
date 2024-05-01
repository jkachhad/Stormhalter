using System;
using System.Windows.Media;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Models;
using Color = Microsoft.Xna.Framework.Color;

namespace Kesmai.WorldForge.Windows;

public class ComponentFrame : Canvas
{
	private ComponentImage _image;

	public static readonly int ComponentPropertyId = CreateProperty(
		typeof(ComponentFrame), "Component", GamePropertyCategories.Default, null, default(TerrainComponent),
		UIPropertyOptions.AffectsRender);

	public TerrainComponent Component
	{
		get => GetValue<TerrainComponent>(ComponentPropertyId);
		set => SetValue(ComponentPropertyId, value);
	}

	public static readonly int ClickEventId = CreateEvent(
		typeof(ButtonBase), "Click", GamePropertyCategories.Default, null, EventArgs.Empty);
		
	public event EventHandler<EventArgs> Click
	{
		add => Events.Get<EventArgs>(ClickEventId).Event += value;
		remove => Events.Get<EventArgs>(ClickEventId).Event -= value;
	}

	public ComponentFrame()
	{
		Style = "DarkCanvas";

		HorizontalAlignment = HorizontalAlignment.Stretch;
	}

	protected override void OnLoad()
	{
		base.OnLoad();
			
		var stackFrame = new StackPanel()
		{
			Margin = new Vector4F(5),
			HorizontalAlignment = HorizontalAlignment.Center,
		};
			
		_image = new ComponentImage()
		{
			Background = Color.FromNonPremultiplied(0, 0, 0, 50),
				
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		_image.Component = Component;

		var componentName = new TextBlock()
		{
			Font = "Tahoma", FontSize = 10,
				
			Foreground = Color.Yellow, Stroke = Color.Black,
			FontStyle = MSDFStyle.Outline,
				
			HorizontalAlignment = HorizontalAlignment.Center,
				
			Text = Component.GetType().Name,
		};
			
		stackFrame.Children.Add(_image);
		stackFrame.Children.Add(componentName);
			
		Children.Add(stackFrame);
			
		Properties.Get<TerrainComponent>(ComponentPropertyId).Changed += (o, args) =>
		{
			_image.Component = args.NewValue;
		};
	}

	protected override void OnHandleInput(InputContext context)
	{
		if (!IsLoaded || !IsVisible)
			return;
			
		base.OnHandleInput(context);

		var inputService = InputService;

		if (inputService.IsMouseOrTouchHandled)
			return;

		if ((IsMouseDirectlyOver || _image.IsMouseOver) && inputService.IsReleased(MouseButtons.Left))
			Events.Get<EventArgs>(ClickEventId).Raise();
	}
}