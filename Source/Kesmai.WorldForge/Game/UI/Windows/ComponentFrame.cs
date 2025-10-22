using System;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Models;
using Color = Microsoft.Xna.Framework.Color;

namespace Kesmai.WorldForge.Windows;

public class ComponentFrame : StackPanel
{
	public static readonly int ClickEventId = CreateEvent(
		typeof(ButtonBase), nameof(OnClick), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	public static readonly int MoveUpEventId = CreateEvent(
		typeof(ComponentFrame), nameof(OnMoveUp), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	public static readonly int MoveDownEventId = CreateEvent(
		typeof(ComponentFrame), nameof(OnMoveDown), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	public static readonly int DeleteEventId = CreateEvent(
		typeof(ComponentFrame), nameof(OnDelete), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	public static readonly int ChangeEventId = CreateEvent(
		typeof(ComponentFrame), nameof(OnChange), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	private TextBlock _componentTypeTextBlock;
	private ComponentImage _image;
	private PropertyGrid _propertyGrid;

	private Button _orderUpButton;
	private Button _orderDownButton;
	
	private Button _deleteButton;

	public static readonly int ProviderPropertyId = CreateProperty(
		typeof(ComponentFrame), nameof(Provider), GamePropertyCategories.Default, null, default(IComponentProvider),
		UIPropertyOptions.AffectsRender);

	public IComponentProvider Provider
	{
		get => GetValue<IComponentProvider>(ProviderPropertyId);
		set => SetValue(ProviderPropertyId, value);
	}
	
	
	public bool CanMoveUp { get; set; }
	public bool CanMoveDown { get; set; }
	
	public bool CanDelete { get; set; }
	
	public event EventHandler<EventArgs> OnClick
	{
		add => Events.Get<EventArgs>(ClickEventId).Event += value;
		remove => Events.Get<EventArgs>(ClickEventId).Event -= value;
	}
	
	public event EventHandler<EventArgs> OnMoveUp
	{
		add => Events.Get<EventArgs>(MoveUpEventId).Event += value;
		remove => Events.Get<EventArgs>(MoveUpEventId).Event -= value;
	}
	
	public event EventHandler<EventArgs> OnMoveDown
	{
		add => Events.Get<EventArgs>(MoveDownEventId).Event += value;
		remove => Events.Get<EventArgs>(MoveDownEventId).Event -= value;
	}
	
	public event EventHandler<EventArgs> OnDelete
	{
		add => Events.Get<EventArgs>(DeleteEventId).Event += value;
		remove => Events.Get<EventArgs>(DeleteEventId).Event -= value;
	}
	
	public event EventHandler<EventArgs> OnChange
	{
		add => Events.Get<EventArgs>(ChangeEventId).Event += value;
		remove => Events.Get<EventArgs>(ChangeEventId).Event -= value;
	}
	
	public ComponentFrame()
	{
		Style = "Client-Content";

		Orientation = Orientation.Vertical;

		Properties.Get<IComponentProvider>(ProviderPropertyId).Changed += (sender, args) =>
		{
			if (IsLoaded)
				OnComponentUpdate(args.NewValue);
		};

		Opacity = 1.5f;
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		// display the component image and property grid.
		var frameGrid = new Grid()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
		};

		frameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Auto) });
		frameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });
		frameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
		frameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
		
		frameGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
		frameGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });

		// display the component type.
		if (_componentTypeTextBlock is null)
		{
			_componentTypeTextBlock = new TextBlock()
			{
				Font = "Tahoma", FontSize = 10,
				FontStyle = MSDFStyle.BoldOutline,

				Foreground = Color.Yellow, Stroke = Color.Black,
				
				HorizontalAlignment = HorizontalAlignment.Stretch,

				Background = Color.FromNonPremultiplied(255, 255, 255, 60),
				Padding = new  Vector4F(5, 5, 5, 5),
			};
		}
		frameGrid.AddChild(_componentTypeTextBlock, 1, 1, 4);
		
		if (_image is null)
		{
			_image = new ComponentImage()
			{
				Background = Color.FromNonPremultiplied(255, 255, 255, 60),
				Margin = new  Vector4F(0, 5, 0, 5),
			};
		}
		frameGrid.AddChild(_image, 1, 2);

		var orderPanel = new StackPanel()
		{
			Margin = new  Vector4F(0, 5, 0, 5),
			VerticalAlignment = VerticalAlignment.Top,
		};
		
		if (_orderUpButton is null)
		{
			_orderUpButton = new Button()
			{
				Content = new TextBlock()
				{
					Text = "▲", FontSize = 8, HorizontalAlignment = HorizontalAlignment.Center,
					Font = "Tahoma", Foreground = (CanMoveUp ? Color.Green : Color.Black),
				},
				Style = "GameIconButton",
				
				IsEnabled = CanMoveUp,
				
				ToolTip = "[SHIFT + UP]"
			};
		}
		_orderUpButton.Click += (o, args) => Events.Get<EventArgs>(MoveUpEventId).Raise();
		
		if (_orderDownButton is null)
		{
			_orderDownButton = new Button()
			{
				Content = new TextBlock()
				{
					Text = "▼", FontSize = 8, HorizontalAlignment = HorizontalAlignment.Center,
					Font = "Tahoma", Foreground = (CanMoveDown ? Color.Green : Color.Black),
				},
				Style = "GameIconButton",
				
				IsEnabled = CanMoveDown,
				
				ToolTip = "[SHIFT + DOWN]"
			};
		}
		_orderDownButton.Click += (o, args) => Events.Get<EventArgs>(MoveDownEventId).Raise();
		
		orderPanel.Children.Add(_orderUpButton);
		orderPanel.Children.Add(_orderDownButton);

		var actionsPanel = new StackPanel()
		{
			Margin = new Vector4F(0, 5, 0, 5),
			VerticalAlignment = VerticalAlignment.Bottom,
		};
		
		if (_deleteButton is null)
		{
			_deleteButton = new Button()
			{
				Content = new TextBlock()
				{
					Text = "X", FontSize = 8, HorizontalAlignment = HorizontalAlignment.Center,
					Font = "Tahoma", Foreground = (CanDelete ? Color.Red : Color.Black),
				},
				Style = "GameIconButton",
				
				IsEnabled = CanDelete,
				
				ToolTip = "[DELETE]"
			};
		}
		_deleteButton.Click += (o, args) => Events.Get<EventArgs>(DeleteEventId).Raise();
		
		actionsPanel.Children.Add(_deleteButton);
		
		frameGrid.AddChild(orderPanel, 4, 2);
		frameGrid.AddChild(actionsPanel, 4, 2);

		if (_propertyGrid is null)
		{
			_propertyGrid = new PropertyGrid()
			{
				Margin = new  Vector4F(5),
			};
			_propertyGrid.OnItemChanged += (o, args) =>
			{
				Events.Get<EventArgs>(ChangeEventId).Raise();

				if (_image != null)
					_image.Invalidate();
			};
		}

		frameGrid.AddChild(_propertyGrid, 2, 2);
		
		Children.Add(frameGrid);
		
		if (Provider != null)
			OnComponentUpdate(Provider);
	}

	private void OnComponentUpdate(IComponentProvider provider)
	{
		if (_componentTypeTextBlock != null)
			_componentTypeTextBlock.Text = provider.GetType().Name;
		
		if (_image != null)
			_image.Provider = provider;
		
		if (_propertyGrid != null)
			_propertyGrid.Item = provider;
	}

	protected override void OnHandleInput(InputContext context)
	{
		if (!IsLoaded || !IsVisible)
			return;
			
		base.OnHandleInput(context);

		var inputService = InputService;

		if (IsMouseOver && inputService.IsReleased(MouseButtons.Left))
			Events.Get<EventArgs>(ClickEventId).Raise();
	}
}