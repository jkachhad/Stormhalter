using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Lidgren.Network;

namespace Kesmai.WorldForge;

public static class Network
{
	private static NetClient _client;
	private static NetPeerConfiguration _configuration;
	private static NetConnection _connection;

	public static Action<NetIncomingMessage> Incoming;
	public static Action Connected;
	public static Action Disconnected;

	private static Thread _networkThread;
	private static bool _closing = false;
	
	public static void Initialize()
	{
		Application.Current.Exit += (sender, args) =>
		{
			_closing = true;
		};
		
		_configuration = new NetPeerConfiguration("WorldForge")
		{
			MaximumHandshakeAttempts = 3,
		};
		_configuration.SetMessageTypeEnabled(NetIncomingMessageType.UnconnectedData, true);

		_client = new NetClient(_configuration);
		_client.Start();
		
		_networkThread = new Thread(Process);
		_networkThread.Start();
	}

	private static void Process()
	{
		while (!_closing)
		{
			var message = default(NetIncomingMessage);

			lock (_client)
			{
				while ((message = _client.ReadMessage()) != null)
				{
					switch (message.MessageType)
					{
						case NetIncomingMessageType.UnconnectedData:
						case NetIncomingMessageType.Data:
						{
							if (Incoming != null)
								Incoming(message);
							
							break;
						}
						case NetIncomingMessageType.StatusChanged:
						{
							var status = (NetConnectionStatus)message.ReadByte();

							switch (status)
							{
								case NetConnectionStatus.Connected:
								{
									if (Connected != null)
										Connected();

									break;
								}
								case NetConnectionStatus.Disconnected:
								case NetConnectionStatus.Disconnecting:
								{
									if (Disconnected != null)
										Disconnected();

									break;
								}
							}

							break;
						}
					}

					_client.Recycle(message);
				}
			}
			
			Thread.Sleep(1);
		}
	}

	public static void Connect(string host, int port)
	{
		lock (_client)
		{
			var discovered = _client.DiscoverKnownPeer(host, port);

			if (!discovered)
				return;
			
			_connection = _client.Connect(host, port);
		}
	}

	public static void Disconnect()
	{
		lock (_client)
		{
			_client.Disconnect(String.Empty);
		}
	}
}