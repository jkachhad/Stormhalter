using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using Lidgren.Network;

namespace Kesmai.WorldForge.UI
{
	public partial class AuthenticationWindow : Window
	{
		private static string StoragePath = ".storage";

		private static string ComponentsName = "Components.cache";
		private static string ScriptingName = "Scripting.cache";
		
		private static Brush _errorBrush = new SolidColorBrush(Colors.Red);
		private static Brush _normalBrush = new SolidColorBrush(Colors.Black);
		
		private DirectoryInfo _storageDirectory;
		
		private FileInfo _componentsFile;
		private FileInfo _scriptsFile;
		
		private NetClient _client;
		private NetPeerConfiguration _configuration;
		private NetConnection _connection;

		private int _completedTasks = 0;
		private int _totalTasks = 4;

		private bool _authenticate = true;

		public AuthenticationWindow()
		{
			_storageDirectory = new DirectoryInfo(StoragePath);
			
			_componentsFile = new FileInfo($@"{_storageDirectory.FullName}\{ComponentsName}");
			_scriptsFile = new FileInfo($@"{_storageDirectory.FullName}\{ScriptingName}");
			
			InitializeComponent();

			if (_authenticate)
			{
				Loaded += (sender, args) =>
				{
#if (!DEBUG) 
					Task.Run(() => Authenticate("play.stormhalter.com", 2594));
#else
					Task.Run(() => Authenticate("127.0.0.1", 2594));
#endif
				};
			}
			else
			{
				OnComplete(false);
			}
		}
		
		public void OnComplete(bool useCache)
		{
			Dispatcher.Invoke(() =>
			{
				if (useCache)
				{
					if (_storageDirectory is { Exists: false })
						_storageDirectory.Create();
					
					if (_componentsFile.Exists)
						Core.ComponentsResource = XDocument.Load(_componentsFile.FullName);
					
					if (_scriptsFile.Exists)
						Core.ScriptingData = File.ReadAllBytes(_scriptsFile.FullName);
				}

				var applicationWindow = Application.Current.MainWindow = new ApplicationWindow();

				Close();
				
				applicationWindow.Show();
			});
		}
		
		public void OnFail()
		{
			Dispatcher.Invoke(Close);
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

		private async Task Authenticate(string host, int port)
		{
			if (_storageDirectory is { Exists: false })
				_storageDirectory.Create();

			SetStatus("Connecting...", _normalBrush);
			
			await Task.Delay(100);
			
			_configuration = new NetPeerConfiguration("WorldForge")
			{
				MaximumHandshakeAttempts = 3,
			};
			_configuration.SetMessageTypeEnabled(NetIncomingMessageType.UnconnectedData, true);

			_client = new NetClient(_configuration);
			_client.Start();

			await Task.Delay(10);
			
			var discovered = _client.DiscoverKnownPeer(host, port);

			if (!discovered)
			{
				SetStatus("Connection failed.", _errorBrush);
				return;
			}

			_connection = _client.Connect(host, port);

			var connected = true;
			var complete = false;

			while (!complete && connected)
			{
				var message = default(NetIncomingMessage);

				while ((message = _client.ReadMessage()) != null)
				{
					switch (message.MessageType)
					{
						case NetIncomingMessageType.UnconnectedData:
						case NetIncomingMessageType.Data:
						{
							var command = message.ReadByte();

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
										
										await File.WriteAllBytesAsync(_scriptsFile.FullName, data);
									}

									IncreaseProgress();
									break;
								}
								case 0x10: /* Complete */
								{
									IncreaseProgress();
									
									complete = true;
									SetStatus("Complete.", _normalBrush);
									
									await Task.Delay(1000);

									Dispatcher.Invoke(Core.Authenticated);
									break;
								}
							}
							
							break;
						}
						case NetIncomingMessageType.StatusChanged:
						{
							var status = (NetConnectionStatus)message.ReadByte();

							switch (status)
							{
								case NetConnectionStatus.Connected:
								{
									IncreaseProgress();
									
									SetStatus("Updating.", _normalBrush);
									break;
								}
								case NetConnectionStatus.Disconnected:
								case NetConnectionStatus.Disconnecting:
								{
									SetStatus("Disconnecting.", _normalBrush);
									
									await Task.Delay(1000);
									
									connected = false;
									
									break;
								}
							}

							break;
						}
					}

					_client.Recycle(message);
				}
			}

			var useCache = false;
			
			if (!complete)
			{
				var skipOfflinePrompt = new FileInfo($@"{_storageDirectory.FullName}\WorkOffline.flag").Exists;

				if (!skipOfflinePrompt) {
					var messageResult = MessageBox.Show("Do you wish to continue in offline mode?", "Unable to connect to server", MessageBoxButton.YesNo);
					if (messageResult != MessageBoxResult.Yes)
					{
						OnFail();
						return;
					}
				}

				useCache = true;
			}

			OnComplete(useCache);
		}
	}
}