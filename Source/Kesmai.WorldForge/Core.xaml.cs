using System;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using DigitalRune.ServiceLocation;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
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
		
	public static byte[] ScriptingData { get; set; }

	public static string CustomArtPath { get; set; }

	public static bool Offline { get; set; } = false;

	#endregion

	#region Fields

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

		if (Current.Resources["applicationPresenter"] is ApplicationPresenter presenter)
			ServiceContainer.Register(typeof(ApplicationPresenter), null, presenter);

		SyncfusionLicenseProvider.RegisterLicense("Mzk1NTI2QDMxMzgyZTM0MmUzMG85YlBIdldReGhYeUl3OFQxWUpUVDhyZ3gyRFpESm1NRUF1aUtpM01pcUk9");
			
		Network.Initialize();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		Network.Disconnect();
			
		base.OnExit(e);
	}

	public static void Authenticated()
	{
	}
		
	#endregion
}