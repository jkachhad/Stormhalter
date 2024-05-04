using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using Lidgren.Network;

namespace Kesmai.WorldForge.UI;

public partial class AuthenticationWindow : Window
{
	private static string StoragePath = ".storage";

	private static string ComponentsName = "Components.cache";
	private static string ScriptingName = "Scripting.cache";
	private static string CustomArtConfigName = "CustomArt.cfg";

	private static Brush _errorBrush = new SolidColorBrush(Colors.Red);
	private static Brush _normalBrush = new SolidColorBrush(Colors.Black);
		
	private DirectoryInfo _storageDirectory;
		
	private FileInfo _componentsFile;
	private FileInfo _scriptsFile;
	private FileInfo _customArtConfig;

	private int _completedTasks = 0;
	private int _totalTasks = 4;

	private bool _authenticate = true;

	public AuthenticationWindow()
	{
		_storageDirectory = new DirectoryInfo(StoragePath);
			
		_componentsFile = new FileInfo($@"{_storageDirectory.FullName}\{ComponentsName}");
		_scriptsFile = new FileInfo($@"{_storageDirectory.FullName}\{ScriptingName}");
		_customArtConfig = new FileInfo($@"{_storageDirectory.FullName}\{CustomArtConfigName}");

		InitializeComponent();
			
		if (_authenticate)
		{
			Loaded += (sender, args) =>
			{
				Network.OnConnect += Connected;
				Network.OnDisconnect += Disconnected;
				Network.OnIncoming += Incoming;
					
#if (DEBUG) 
					Task.Run(() => Authenticate("play.stormhalter.com", 2594));
#else
				Task.Run(() => Authenticate("127.0.0.1", 2594));
#endif
			};

			Unloaded += (sender, args) =>
			{
				Network.OnConnect -= Connected;
				Network.OnDisconnect -= Disconnected;
				Network.OnIncoming -= Incoming;
			};
		}
		else
		{
			OnComplete(false);
		}
	}

	private void Incoming(NetIncomingMessage message)
	{
		var command = message.ReadInt16();
					
		switch (command)
		{
			case 0x01: /* Components Data. */
			{
				var length = message.ReadInt32();
				var data = message.ReadBytes(length);
				if (data.Length > 0)
				{
					using (var stream = new MemoryStream())
					{
						stream.Write(data, 0, length);
						stream.Seek(0, SeekOrigin.Begin);

						var components = Core.ComponentsResource = XDocument.Load(stream);
	
						if (_componentsFile.Exists)
							_componentsFile.Delete();
									
						components.Save(_componentsFile.FullName);
					}
				}
	
				IncreaseProgress();
				break;
			}
			case 0x02: /* Assembly Data. */
			{
				var length = message.ReadInt32();
				var data = message.ReadBytes(length);
				if (data.Length > 0)
				{
					Core.ScriptingData = data;
								
					if (_scriptsFile.Exists)
						_scriptsFile.Delete();
								
					File.WriteAllBytesAsync(_scriptsFile.FullName, data);
				}
				IncreaseProgress();
				break;
			}
			case 0x10: /* Complete */
			{
				IncreaseProgress();
				SetStatus("Complete.", _normalBrush);
					
				OnComplete(false);
				break;
			}
		}
	}

	private void Disconnected()
	{
		SetStatus("Disconnecting.", _normalBrush);
			
		var skipOfflinePrompt = new FileInfo($@"{_storageDirectory.FullName}\WorkOffline.flag").Exists;

		if (!skipOfflinePrompt) 
		{
			var messageResult = MessageBox.Show("Do you wish to continue in offline mode?", "Unable to connect to server", MessageBoxButton.YesNo);
				
			if (messageResult != MessageBoxResult.Yes)
			{
				Dispatcher.Invoke(Close);
				return;
			}
		}

		OnComplete(true);
	}

	private void Connected()
	{
		IncreaseProgress();
		SetStatus("Updating.", _normalBrush);
	}

	public void OnComplete(bool useCache)
	{
		Dispatcher.Invoke(() =>
		{
			if (useCache)
			{
				Core.Offline = true;
					
				if (_storageDirectory is { Exists: false })
					_storageDirectory.Create();
					
				if (_componentsFile.Exists)
					Core.ComponentsResource = XDocument.Load(_componentsFile.FullName);
					
				if (_scriptsFile.Exists)
					Core.ScriptingData = File.ReadAllBytes(_scriptsFile.FullName);
			}

			if (!_customArtConfig.Exists)
			{
				File.WriteAllText(_customArtConfig.FullName, $"{_storageDirectory.FullName}\nModify the above line to point to your GitHub local repo's 'Content' directory.\nIf the folder does not exists, WorldForge will default to the .storage folder.\nCreate a Data\\Terrain-External.xml, a WorldForge\\Compontents.xml (get these from the GitHub repo under Content), and then a folder to contain your texture sheets.");
			}
			var CustomArtConfigPath = File.ReadLines(_customArtConfig.FullName).FirstOrDefault();
			if (Directory.Exists(CustomArtConfigPath)) { Core.CustomArtPath = CustomArtConfigPath; } else { Core.CustomArtPath = _storageDirectory.FullName; }

			var applicationWindow = Application.Current.MainWindow = new ApplicationWindow();

			Close();
				
			applicationWindow.Show();
		});
	}

	public void SetStatus(string status, Brush brush)
	{
		Dispatcher.Invoke(() =>
		{
			_status.Text = status;
			_status.Foreground = brush;
		});
	}
		
	public void IncreaseProgress()
	{
		Dispatcher.Invoke(() =>
		{
			_progress.Value = (int)(++_completedTasks * 100 / _totalTasks);
		});
	}

	private void Authenticate(string host, int port)
	{
		if (_storageDirectory is { Exists: false })
			_storageDirectory.Create();

		SetStatus("Connecting...", _normalBrush);
			
		Network.Connect(host, port);
	}
}