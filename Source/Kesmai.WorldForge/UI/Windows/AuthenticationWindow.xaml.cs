using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using Lidgren.Network;

namespace Kesmai.WorldForge.UI
{
	public partial class AuthenticationWindow : Window
	{
		private static Brush _errorBrush = new SolidColorBrush(Colors.Red);
		private static Brush _normalBrush = new SolidColorBrush(Colors.Black);
		
		private NetClient _client;
		private NetPeerConfiguration _configuration;
		private NetConnection _connection;

		private int _completedTasks = 0;
		private int _totalTasks = 4;

		private bool _authenticate = true;
		
		public AuthenticationWindow()
		{
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
				OnComplete();
			}
		}
		
		public void OnComplete()
		{
			Dispatcher.Invoke(() =>
			{
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
											
											Core.ComponentsResource = XDocument.Load(stream);
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
										Core.ScriptingData = data;

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
									
									await Task.Delay(5000);
									
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

			if (complete)
				OnComplete();
			else
				OnFail();
		}
	}
}