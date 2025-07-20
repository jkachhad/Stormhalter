using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Game.Timing;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Content;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Content;
using DigitalRune.ServiceLocation;
using DigitalRune.Storages;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class Game
{
	private readonly GraphicsManager _graphicsManager;

	public Game(ApplicationWindow window)
	{
		// ----- Service Container
		// The MyGame uses a ServiceContainer, which is a simple service locator 
		// and Inversion of Control (IoC) container. (The ServiceContainer can be 
		// replaced by any other container that implements System.IServiceProvider.)
		var serviceContainer = (ServiceContainer)ServiceLocator.Current;

		// ----- Storage
		// Create a "virtual file system" for reading game assets.
		var vfsStorage = new VfsStorage();
		
		vfsStorage.MountInfos.Add(new VfsMountInfo(new TitleStorage(String.Empty), null));
		
		try
		{
			vfsStorage.MountInfos.Add(new VfsMountInfo(new GZipStorage(vfsStorage, "Data.bin"), null));
			vfsStorage.MountInfos.Add(new VfsMountInfo(new GZipStorage(vfsStorage, "Kesmai.bin"), null));
			vfsStorage.MountInfos.Add(new VfsMountInfo(new GZipStorage(vfsStorage, "Stormhalter.bin"), null));
			vfsStorage.MountInfos.Add(new VfsMountInfo(new GZipStorage(vfsStorage, "UI.bin"), null));
		}
		catch
		{
			MessageBox.Show("Missing either Data.bin, Kesmai.bin, Stormhalter.bin, or UI.bin.");
			throw;
		}
		
		vfsStorage.Readers.Add(typeof(XDocument), new XDocumentReader());

		// ----- Content
		ContentManager contentManager = new StorageContentManager(ServiceLocator.Current, vfsStorage);

		serviceContainer.Register(typeof(IStorage), null, vfsStorage);
		serviceContainer.Register(typeof(ContentManager), null, contentManager);
			
#if (DEBUG)
		/* Hack to allow content reading from external library. Release builds have the types IL merged. */
		ContentTypeReaderManager.AddTypeCreator("DigitalRune.Game.UI.Content.ThemeReader", () => new ThemeReader());
		ContentTypeReaderManager.AddTypeCreator("DigitalRune.Mathematics.Content.Vector4FReader", () => new Vector4FReader());
		ContentTypeReaderManager.AddTypeCreator("DigitalRune.Game.UI.BitmapFontReader", () => new BitmapFontReader());
#endif
			
		// ----- Graphics
		// Create Direct3D 11 device.
		var presentationParameters = new PresentationParameters
		{
			// Do not associate graphics device with any window.
			DeviceWindowHandle = IntPtr.Zero,
		};
		var graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters);

		// An IGraphicsDeviceService is required by the MonoGame/XNA content manager.
		var graphicsDeviceManager = new GraphicsDeviceManager(graphicsDevice);
			
		serviceContainer.Register(typeof(IGraphicsDeviceService), null, graphicsDeviceManager);

		// Create and register the graphics manager.
		_graphicsManager = new GraphicsManager(graphicsDevice, contentManager);

		serviceContainer.Register(typeof(IGraphicsService), null, _graphicsManager);
		serviceContainer.Register(typeof(GraphicsDevice), null, graphicsDevice);
			
		serviceContainer.Register(typeof(TerrainManager), null, new TerrainManager());
			
		// ----- Timing
		// We can use the CompositionTarget.Rendering event to trigger our game loop.
		// The CompositionTarget.Rendering event is raised once per frame by WPF.

		// To measure the time that has passed, we use a HighPrecisionClock.
		var clock = new HighPrecisionClock();
		clock.Start();

		CompositionTarget.Rendering += (s, e) => clock.Update();

		// The FixedStepTimer reads the clock and triggers the game loop at 60 Hz.
		//_timer = new FixedStepTimer(_clock)
		//{
		//  StepSize = new TimeSpan(166667), // ~60 Hz
		//  AccumulateTimeSteps = false,
		//};
		// The VariableStepTimer reads the clock and triggers the game loop as often
		// as possible.
		IGameTimer timer = new VariableStepTimer(clock);
		timer.TimeChanged += (s, e) => GameLoop(e.DeltaTime);
		timer.Start();
	}


	private void GameLoop(TimeSpan deltaTime)
	{
		// Update graphics service and graphics screens.
		_graphicsManager.Update(deltaTime);

		// Render the current graphics screens into the presentation targets.
		foreach (var target in _graphicsManager.PresentationTargets.OfType<PresentationTarget>())
		{
			target.Update(deltaTime);
				
			if (!target.HasScreens)
				target.Initialize();
				
			_graphicsManager.Render(target);
		}
	}
}