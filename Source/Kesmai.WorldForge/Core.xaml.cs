using System;
using System.Collections.Generic;
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

namespace Kesmai.WorldForge
{
	public partial class Core : Application
	{
		#region Static
		
		public static ServiceContainer ServiceContainer = new ServiceContainer();
		public static Version Version { get; set; }

		public static bool AllowGenerator
		{
#if DEBUG
			get => true;
#else
			get => false;
#endif
		}
		
		public static bool AllowCompile
		{
#if DEBUG
			get => true;
#else
			get => false;
#endif
		}
		
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

			/*DispatcherUnhandledException += (sender, args) =>
			{
				args.Handled = true;
			};*/
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
			
			ServiceContainer.Register(typeof(ScriptTemplateProvider), null, new ScriptTemplateProvider());
			
			SyncfusionLicenseProvider.RegisterLicense("Mzk1NTI2QDMxMzgyZTM0MmUzMG85YlBIdldReGhYeUl3OFQxWUpUVDhyZ3gyRFpESm1NRUF1aUtpM01pcUk9");
		}

		public static void Authenticated()
		{
		}
		
		#endregion
	}
}
