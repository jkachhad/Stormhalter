using System;
using System.Windows;
using CommonServiceLocator;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Interop;
using DigitalRune.ServiceLocation;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge
{
	public class RegionPresentationTarget : PresentationTarget
	{
		public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
		{
			return new WorldGraphicsScreen(this, _uiManager, graphicsService);
		}
	}

	public class LocationsPresentationTarget : PresentationTarget
	{
		private LocationsGraphicsScreen _screen;
		
		public override bool AllowInput => true;
		
		public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
		{
			return (_screen = new LocationsGraphicsScreen(this, _uiManager, graphicsService));
		}

		public void SetLocation(SegmentLocation location)
		{
			if (_screen != null)
				_screen.SetLocation(location.X, location.Y);
		}

	}

	public class SpawnsPresentationTarget : PresentationTarget
	{
		private SpawnsGraphicsScreen _screen;

		public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
		{
			return (_screen = new SpawnsGraphicsScreen(this, _uiManager, graphicsService));
		}

		public void SetLocation(Spawner spawner)
		{
			if (_screen != null)
				_screen.SetSpawner(spawner);
		}
	}

	public class SubregionsPresentationTarget : PresentationTarget
	{
		private SubregionsGraphicsScreen _screen;
		
		public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
		{
			return (_screen = new SubregionsGraphicsScreen(this, _uiManager, graphicsService));
		}

		public void SetSubregion(SegmentSubregion subregion)
		{
			if (_screen != null)
				_screen.SetSubregion(subregion);
		}
	}

	public abstract class PresentationTarget : D3DImagePresentationTarget
	{
		private bool _isInitialized;
		private bool _isRendering;
		
		protected WorldGraphicsScreen _worldScreen;

		protected WpfInputManager _inputManager;
		protected WpfUIManager _uiManager;

		private Selection _selection;
		
		public bool HasScreens => _isInitialized;
		
		public WorldGraphicsScreen WorldScreen => _worldScreen;
		
		public static readonly DependencyProperty RegionProperty =
			DependencyProperty.Register(nameof(Region), typeof(SegmentRegion), typeof(PresentationTarget),
				new FrameworkPropertyMetadata(
					default(SegmentRegion), FrameworkPropertyMetadataOptions.AffectsRender));
		
		public SegmentRegion Region
		{
			get => (SegmentRegion)GetValue(RegionProperty);
			set
			{
				SetValue(RegionProperty, value);

				if (value != null)
					InvalidateRender();
			}
		}

		public virtual bool AllowInput => true;
		
		protected PresentationTarget()
		{
			MaxWidth = 4096;
			MaxHeight = 16384;
			
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;

			_isInitialized = false;
		}

		public void Initialize()
		{
			var services = (ServiceContainer)ServiceLocator.Current;
			var graphicsService = services.GetInstance<IGraphicsService>();
			
			_inputManager = new WpfInputManager(new WpfKeyboard(this), new WpfMouse(this));
			_uiManager = new WpfUIManager(_inputManager);
			
			_worldScreen = CreateGraphicsScreen(graphicsService);
			_worldScreen.OnSizeChanged((int)ActualWidth, (int)ActualHeight);
			_worldScreen.Initialize();

			graphicsService.Screens.Add(_worldScreen);

			_isInitialized = true;
		}

		public abstract WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService);

		public void Update(TimeSpan deltaTime)
		{
			if (!_isInitialized || !_isRendering)
				return;

			if (AllowInput)
				_inputManager.Update(deltaTime);
			
			_uiManager.Update(deltaTime);
		}

		protected override void OnBeginRender(RenderContext context)
		{
			context.SetPresentationTarget(this);
			_worldScreen.IsVisible = true;
		}
		
		protected override void OnEndRender(RenderContext context)
		{
			_worldScreen.IsVisible = false;
			context.SetPresentationTarget(null);
		}

		private void OnLoaded(object sender, RoutedEventArgs eventArgs)
		{
			if (GraphicsService != null)
				return;
			
			var services = (ServiceContainer)ServiceLocator.Current;
			var graphicsService = services.GetInstance<IGraphicsService>();
			
			graphicsService.PresentationTargets.Add(this);

			_isRendering = true;
		}

		private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
		{
			if (GraphicsService == null)
				return;
			
			var services = (ServiceContainer)ServiceLocator.Current;
			var graphicsService = services.GetInstance<IGraphicsService>();

			graphicsService.PresentationTargets.Remove(this);

			_isRendering = false;
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			var newSize = sizeInfo.NewSize;
			
			var width = (int)newSize.Width;
			var height = (int)newSize.Height;

			if (width > 0 && height > 0)
			{
				if (_worldScreen != null)
					_worldScreen.OnSizeChanged(width, height);
			}
		}

		public void InvalidateRender()
		{
			if (_worldScreen != null)
				_worldScreen.InvalidateRender();
		}
	}
}
