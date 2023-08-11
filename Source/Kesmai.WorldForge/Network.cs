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

	public static Action<NetIncomingMessage> OnIncoming;
	public static Action OnConnect;
	public static Action OnDisconnect;

	private static Thread _networkThread;
	private static bool _closing = false;

	public static NetClient Client => _client;

	public static bool Disconnected => _connection.Status != NetConnectionStatus.Connected;
	
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
							if (OnIncoming != null)
								OnIncoming(message);
							
							break;
						}
						case NetIncomingMessageType.StatusChanged:
						{
							var status = (NetConnectionStatus)message.ReadByte();

							switch (status)
							{
								case NetConnectionStatus.Connected:
								{
									if (OnConnect != null)
										OnConnect();

									break;
								}
								case NetConnectionStatus.Disconnected:
								case NetConnectionStatus.Disconnecting:
								{
									if (OnDisconnect != null)
										OnDisconnect();

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

	public static void Send(byte command, byte[] buffer)
	{
		lock (_client)
		{
			var message = _client.CreateMessage();

			message.Write((byte)command);
			message.Write((int)buffer.Length);
			message.Write((byte[])buffer);

			_client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
			_client.FlushSendQueue();
		}
	}
}