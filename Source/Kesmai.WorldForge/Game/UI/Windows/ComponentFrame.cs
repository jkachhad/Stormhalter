using System;
using System.Linq;
using CommonServiceLocator;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
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
	
	protected TextBlock _componentTypeTextBlock;
	protected ComponentImage _image;

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

		var content = GetContent();
		
		if (content != null)
			frameGrid.AddChild(content, 2, 2);
		
		Children.Add(frameGrid);
		
		if (Provider != null)
			OnComponentUpdate(Provider);
	}

	protected virtual void OnComponentUpdate(IComponentProvider provider)
	{
		if (_componentTypeTextBlock != null)
			_componentTypeTextBlock.Text = GetHeader();
		
		if (_image != null)
			_image.Provider = provider;
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

	protected virtual UIControl GetContent() => null;
	
	protected virtual string GetHeader() => "Component";
}

public class TerrainComponentFrame : ComponentFrame
{
	private PropertyGrid _propertyGrid;
	
	public TerrainComponentFrame()
	{
	}

	protected override UIControl GetContent()
	{
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
		
		return _propertyGrid;
	}

	protected override void OnComponentUpdate(IComponentProvider provider)
	{
		base.OnComponentUpdate(provider);
		
		if (_propertyGrid != null)
			_propertyGrid.Item = provider;
	}

	protected override string GetHeader() => Provider.GetType().Name;
}

public class SegmentComponentFrame : ComponentFrame
{
	private Button _presentButton;
	
	public SegmentComponentFrame()
	{
	}

	protected override UIControl GetContent()
	{
		if (_presentButton is null)
		{
			_presentButton = new Button()
			{
				Content = new TextBlock()
				{
					Text = "Edit Component", FontSize = 10, HorizontalAlignment = HorizontalAlignment.Center,
					Font = "Tahoma", Foreground = Color.Yellow,
				},
				Style = "Client-Button",
				
				Margin = new  Vector4F(5),
				
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
			};
			_presentButton.Click += (o, args) =>
			{
				var applicationPresenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

				if (applicationPresenter != null && Provider is SegmentComponent segmentComponent)
					segmentComponent.Present(applicationPresenter);
			};
		}
		
		return _presentButton;
	}
	
	protected override string GetHeader()
	{
		var internalType = String.Empty;
		var components = Provider.GetComponents().ToList();
		
		if (components.Count() > 1)
			internalType = " (Multiple)";
		else if (components.Count() is 1)
			internalType = $" ({components.First().GetType().Name})";
		
		return $"SegmentComponent [{internalType}]";
	}
}