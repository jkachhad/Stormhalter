using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using DigitalRune.ServiceLocation;
using CommonServiceLocator;
using DigitalRune.Collections;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Syncfusion.Licensing;

namespace Kesmai.WorldForge;

public partial class Core : Application
{
	#region Static
		
	public static ServiceContainer ServiceContainer = new ServiceContainer();
	public static Version Version { get; set; }

	public static Visibility DebugOnly
	{
#if DEBUG
		get { return Visibility.Visible; }
#else
            get { return Visibility.Collapsed; }
#endif
	}
		
	public static Visibility ReleaseOnly
	{
#if DEBUG
		get { return Visibility.Collapsed; }
#else
            get { return Visibility.Visible; }
#endif
	}
		
	public static XDocument ComponentsResource { get; set; }

	public static string CustomArtPath { get; set; }

	public static bool Offline { get; set; } = false;

	#endregion

	#region Fields

	private static string StoragePath = ".storage";

	private static string ComponentsName = "Components.cache";
	private static string ScriptingName = "Scripting.cache";
	private static string CustomArtConfigName = "CustomArt.cfg";

	private DirectoryInfo _storageDirectory;
		
	private FileInfo _componentsFile;
	private FileInfo _customArtConfig;
	
	#endregion

	#region Properties and Events

	#endregion

	#region Constructors and Cleanup

	/// <summary>
	/// Initializes a new instance of the <see cref="Core"/> class.
	/// </summary>
	public Core()
	{
		ServiceLocator.SetLocatorProvider(() => ServiceContainer);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Invoked when the application has started.
	/// </summary>
	protected override void OnStartup(StartupEventArgs e)
	{
		Version = Assembly.GetExecutingAssembly().GetName().Version;

		ServiceContainer.Register(typeof(SegmentProject), null, new SegmentProject());
		
		if (Current.Resources["applicationPresenter"] is ApplicationPresenter presenter)
			ServiceContainer.Register(typeof(ApplicationPresenter), null, presenter);

		SyncfusionLicenseProvider.RegisterLicense("Mzk1NTI2QDMxMzgyZTM0MmUzMG85YlBIdldReGhYeUl3OFQxWUpUVDhyZ3gyRFpESm1NRUF1aUtpM01pcUk9");
			
		_storageDirectory = new DirectoryInfo(StoragePath);
			
		_componentsFile = new FileInfo($@"{_storageDirectory.FullName}\{ComponentsName}");
		_customArtConfig = new FileInfo($@"{_storageDirectory.FullName}\{CustomArtConfigName}");

		InitializeComponent();
			
		if (_storageDirectory is { Exists: false })
			_storageDirectory.Create();
		
		Offline = true;
		
		if (_componentsFile.Exists)
			ComponentsResource = XDocument.Load(_componentsFile.FullName);

		if (!_customArtConfig.Exists)
			File.WriteAllText(_customArtConfig.FullName, $"{_storageDirectory.FullName}\nModify the above line to point to your GitHub local repo's 'Content' directory.\nIf the folder does not exists, WorldForge will default to the .storage folder.\nCreate a Data\\Terrain-External.xml, a WorldForge\\Compontents.xml (get these from the GitHub repo under Content), and then a folder to contain your texture sheets.");

		var CustomArtConfigPath = File.ReadLines(_customArtConfig.FullName).FirstOrDefault();
		
		if (Directory.Exists(CustomArtConfigPath))
			CustomArtPath = CustomArtConfigPath;
		else
			CustomArtPath = _storageDirectory.FullName;
	}
	
	#endregion
}