using System;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge;

public class GraphicsDeviceManager : IGraphicsDeviceService
{
	public GraphicsDevice GraphicsDevice { get; private set; }

#pragma warning disable 67
	public event EventHandler<EventArgs> DeviceCreated;
	public event EventHandler<EventArgs> DeviceDisposing;
	public event EventHandler<EventArgs> DeviceReset;
	public event EventHandler<EventArgs> DeviceResetting;
#pragma warning restore 67


	public GraphicsDeviceManager(GraphicsDevice graphicsDevice)
	{
		GraphicsDevice = graphicsDevice;
	}
}