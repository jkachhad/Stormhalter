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
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Kesmai.WorldForge.Windows;

public abstract class ComponentFrame : Grid
{
	public static readonly int OrderUpEventId = CreateEvent(
		typeof(ComponentFrame), nameof(OrderUp), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	public static readonly int OrderDownEventId = CreateEvent(
		typeof(ComponentFrame), nameof(OrderDown), GamePropertyCategories.Default, null, EventArgs.Empty);

	public static readonly int DeleteEventId = CreateEvent(
		typeof(ComponentFrame), nameof(Delete), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	public static readonly int ClickEventId = CreateEvent(
		typeof(ComponentFrame), nameof(Click), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	public static readonly int InvalidateEventId = CreateEvent(
		typeof(ComponentFrame), nameof(Invalidate), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	protected readonly IComponentProvider _componentProvider;
	protected ComponentImage _componentImage;
	
	public IComponentProvider Provider => _componentProvider;
	
	public bool AllowOrderUp { get; set; } = true;
	public bool AllowOrderDown { get; set; } = true;
	
	public bool AllowDelete { get; set; } = true;

	public bool ShowOrderUp { get; set; } = true;
	public bool ShowOrderDown { get; set; } = true;
	public bool ShowDelete { get; set; } = true;
	
	public bool IsSelected { get; set; } = false;
	
	public event EventHandler<EventArgs> OrderUp
	{
		add => Events.Get<EventArgs>(OrderUpEventId).Event += value;
		remove => Events.Get<EventArgs>(OrderUpEventId).Event -= value;
	}
	
	public event EventHandler<EventArgs> OrderDown
	{
		add => Events.Get<EventArgs>(OrderDownEventId).Event += value;
		remove => Events.Get<EventArgs>(OrderDownEventId).Event -= value;
	}
	
	public event EventHandler<EventArgs> Delete
	{
		add => Events.Get<EventArgs>(DeleteEventId).Event += value;
		remove => Events.Get<EventArgs>(DeleteEventId).Event -= value;
	}
	
	public event EventHandler<EventArgs> Click
	{
		add => Events.Get<EventArgs>(ClickEventId).Event += value;
		remove => Events.Get<EventArgs>(ClickEventId).Event -= value;
	}
	
	public event EventHandler<EventArgs> Invalidate
	{
		add => Events.Get<EventArgs>(InvalidateEventId).Event += value;
		remove => Events.Get<EventArgs>(InvalidateEventId).Event -= value;
	}
	
	public ComponentFrame(IComponentProvider provider)
	{
		Style = "Client-Content";
		HorizontalAlignment = HorizontalAlignment.Stretch;
		MinWidth = 800;
		
		_componentProvider = provider;
	}

	protected override void OnLoad()
	{
		base.OnLoad();
		
		_componentImage = new ComponentImage()
		{
			Background = Color.FromNonPremultiplied(255, 255, 255, 60),
			Margin = new Vector4F(0, 5, 0, 0),
			
			Provider = _componentProvider,
		};
		
		// Define grid structure.
		ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
		ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });
		ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
		
		RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
		RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });

		// Add header.		
		AddChild(GetHeaderControl(), 1, 1, 3);
		AddChild(GetRenderControl(), 1, 2);
		AddChild(GetContent(), 2, 2);
		AddChild(GetActions(), 3, 2);
	}
	
	protected abstract UIControl GetHeaderControl();
	protected abstract UIControl GetRenderControl();
	protected abstract UIControl GetContent();

	protected virtual UIControl GetActions()
	{
		var actionsPanel = new StackPanel()
		{
			Margin = new  Vector4F(0, 5, 0, 5),
			VerticalAlignment = VerticalAlignment.Top,
		};
		
		var orderUpButton = new Button()
		{
			Content = new TextBlock()
			{
				Text = "▲", FontSize = 8, HorizontalAlignment = HorizontalAlignment.Center,
				Font = "Tahoma", Foreground = (AllowOrderUp ? Color.Green : Color.Black),
			},
			Style = "GameIconButton",
			
			IsEnabled = AllowOrderUp,
			
			ToolTip = "[SHIFT + UP]"
		};
		orderUpButton.Click += (o, args) => Events.Get<EventArgs>(OrderUpEventId).Raise();
		
		var orderDownButton = new Button()
		{
			Content = new TextBlock()
			{
				Text = "▼", FontSize = 8, HorizontalAlignment = HorizontalAlignment.Center,
				Font = "Tahoma", Foreground = (AllowOrderDown ? Color.Green : Color.Black),
			},
			Style = "GameIconButton",
			
			IsEnabled = AllowOrderDown,
			
			ToolTip = "[SHIFT + DOWN]"
		};
		orderDownButton.Click += (o, args) => Events.Get<EventArgs>(OrderDownEventId).Raise();
		
		var deleteButton = new Button()
		{
			Content = new TextBlock()
			{
				Text = "X", FontSize = 8, HorizontalAlignment = HorizontalAlignment.Center,
				Font = "Tahoma", Foreground = (AllowDelete ? Color.Red : Color.Black),
			},
			Style = "GameIconButton",
			
			IsEnabled = AllowDelete,
			
			ToolTip = "[DELETE]"
		};
		deleteButton.Click += (o, args) => Events.Get<EventArgs>(DeleteEventId).Raise();

		if (ShowOrderUp) actionsPanel.Children.Add(orderUpButton);
		if (ShowOrderDown) actionsPanel.Children.Add(orderDownButton);
		if (ShowDelete) actionsPanel.Children.Add(deleteButton);
		
		return actionsPanel;
	}
	
	protected override void OnHandleInput(InputContext context)
	{
		if (!IsLoaded || !IsVisible)
			return;
			
		base.OnHandleInput(context);

		var inputService = InputService;

		if (!inputService.IsMouseOrTouchHandled && IsMouseOver)
		{
			if (inputService.IsDown(MouseButtons.Left))
			{
				Events.Get<EventArgs>(ClickEventId).Raise();
			
				inputService.IsMouseOrTouchHandled = true;
			}
		}
	}

	protected override void OnRender(UIRenderContext context)
	{
		var screen = Screen ?? context.Screen;
		var renderer = screen.Renderer;
		var spriteBatch = renderer.SpriteBatch;

		if (IsSelected)
			spriteBatch.FillRectangle(ActualBounds.ToRectangle(true), Color.OrangeRed);
		
		base.OnRender(context);
	}
}

public class TerrainComponentFrame : ComponentFrame
{
	public static readonly int ConvertEventId = CreateEvent(
		typeof(TerrainComponentFrame), nameof(Convert), GamePropertyCategories.Default, null, EventArgs.Empty);
	
	public event EventHandler<EventArgs> Convert
	{
		add => Events.Get<EventArgs>(ConvertEventId).Event += value;
		remove => Events.Get<EventArgs>(ConvertEventId).Event -= value;
	}
	
	public TerrainComponentFrame(IComponentProvider provider) : base(provider)
	{
	}

	protected override UIControl GetHeaderControl()
	{
		var headerPanel = new StackPanel()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};
		headerPanel.Children.Add(new TextBlock()
		{
			Text = _componentProvider.GetType().Name,
			
			Font = "Tahoma", FontSize = 10,
			FontStyle = MSDFStyle.BoldOutline,

			Foreground = Color.Yellow, Stroke = Color.Black,
			
			Background = Color.FromNonPremultiplied(255, 255, 255, 60),
			Padding = new  Vector4F(5, 5, 5, 5),
			
			HorizontalAlignment = HorizontalAlignment.Stretch,
		});

		return headerPanel;
	}

	protected override UIControl GetRenderControl()
	{
		return _componentImage;
	}

	protected override UIControl GetContent()
	{
		var propertyGrid = new PropertyGrid()
		{
			Margin = new  Vector4F(5),
			Item = _componentProvider,
		};
		propertyGrid.OnItemChanged += (s, e) => Events.Get<EventArgs>(InvalidateEventId).Raise();

		return propertyGrid;
	}
	
	protected override UIControl GetActions()
	{
		var actionsPanel = base.GetActions();

		if (actionsPanel is not StackPanel stackPanel)
			return actionsPanel;
		
		var convertComponent = new Button()
		{
			Content = new TextBlock()
			{
				Text = "C", FontSize = 8, HorizontalAlignment = HorizontalAlignment.Center,
				Font = "Tahoma", Foreground = (AllowOrderUp ? Color.Green : Color.Black),
			},
			Style = "GameIconButton",
		};
		convertComponent.Click += (o, args) => Events.Get<EventArgs>(ConvertEventId).Raise();
		
		stackPanel.Children.Add(convertComponent);
		
		return actionsPanel;
	}
}

public class SegmentComponentFrame : ComponentFrame
{
	public SegmentComponentFrame(IComponentProvider provider) : base(provider)
	{
	}

	protected override UIControl GetHeaderControl()
	{
		var headerPanel = new StackPanel()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};
		
		var internalType = String.Empty;
		var components = _componentProvider.GetComponents().ToList();
		
		if (components.Count() > 1)
			internalType = " (Multiple)";
		else if (components.Count() is 1)
			internalType = $" ({components.First().GetType().Name})";
		
		headerPanel.Children.Add(new TextBlock()
		{
			Text = $"SegmentComponent [{internalType}]",
			
			Font = "Tahoma", FontSize = 10,
			FontStyle = MSDFStyle.BoldOutline,

			Foreground = Color.Yellow, Stroke = Color.Black,
			
			Background = Color.FromNonPremultiplied(255, 255, 255, 60),
			Padding = new  Vector4F(5, 5, 5, 5),
			
			HorizontalAlignment = HorizontalAlignment.Stretch,
		});

		return headerPanel;
	}

	protected override UIControl GetRenderControl()
	{
		return _componentImage;
	}

	protected override UIControl GetContent()
	{
		var presentButton = new Button()
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
		presentButton.Click += (o, args) =>
		{
			var applicationPresenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

			if (applicationPresenter != null && _componentProvider is SegmentComponent segmentComponent)
				segmentComponent.Present(applicationPresenter);
		};

		return presentButton;
	}
}

public class SegmentBrushComponentFrame : ComponentFrame
{
	public SegmentBrushComponentFrame(IComponentProvider provider) : base(provider)
	{
	}

	protected override UIControl GetHeaderControl()
	{
		var headerPanel = new StackPanel()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};
		
		var name = "Unnamed";

		if (_componentProvider != null)
			name = _componentProvider.Name;
		
		headerPanel.Children.Add(new TextBlock()
		{
			Text = $"SegmentBrush [{name}]",
			
			Font = "Tahoma", FontSize = 10,
			FontStyle = MSDFStyle.BoldOutline,

			Foreground = Color.Yellow, Stroke = Color.Black,
			
			Background = Color.FromNonPremultiplied(255, 255, 255, 60),
			Padding = new  Vector4F(5, 5, 5, 5),
			
			HorizontalAlignment = HorizontalAlignment.Stretch,
		});

		return headerPanel;
	}

	protected override UIControl GetRenderControl()
	{
		return _componentImage;
	}

	protected override UIControl GetContent()
	{
		var presentButton = new Button()
		{
			Content = new TextBlock()
			{
				Text = "Edit Brush",
				FontSize = 10,
				HorizontalAlignment = HorizontalAlignment.Center,
				Font = "Tahoma",
				Foreground = Color.Yellow,
			},
			Style = "Client-Button",

			Margin = new Vector4F(5),

			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};
		presentButton.Click += (o, args) =>
		{
			var applicationPresenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

			if (applicationPresenter != null && _componentProvider is SegmentBrush segmentBrush)
				segmentBrush.Present(applicationPresenter);
		};

		return presentButton;
	}
}

public class SegmentTemplateComponentFrame : ComponentFrame
{
	public SegmentTemplateComponentFrame(IComponentProvider provider) : base(provider)
	{
	}

	protected override UIControl GetHeaderControl()
	{
		var headerPanel = new StackPanel()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};
		
		var name = "Unnamed";

		if (_componentProvider != null)
			name = _componentProvider.Name;
		
		headerPanel.Children.Add(new TextBlock()
		{
			Text = $"SegmentTemplate [{name}]",
			
			Font = "Tahoma", FontSize = 10,
			FontStyle = MSDFStyle.BoldOutline,

			Foreground = Color.Yellow, Stroke = Color.Black,
			
			Background = Color.FromNonPremultiplied(255, 255, 255, 60),
			Padding = new  Vector4F(5, 5, 5, 5),
			
			HorizontalAlignment = HorizontalAlignment.Stretch,
		});

		return headerPanel;
	}

	protected override UIControl GetRenderControl()
	{
		return _componentImage;
	}

	protected override UIControl GetContent()
	{
		var presentButton = new Button()
		{
			Content = new TextBlock()
			{
				Text = "Edit Template",
				FontSize = 10,
				HorizontalAlignment = HorizontalAlignment.Center,
				Font = "Tahoma",
				Foreground = Color.Yellow,
			},
			Style = "Client-Button",

			Margin = new Vector4F(5),

			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};
		presentButton.Click += (o, args) =>
		{
			var applicationPresenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

			if (applicationPresenter != null && _componentProvider is SegmentTemplate segmentTemplate)
				segmentTemplate.Present(applicationPresenter);
		};

		return presentButton;
	}
}
