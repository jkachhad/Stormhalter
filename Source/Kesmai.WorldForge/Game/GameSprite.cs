using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge
{
	[Serializable]
	public class MissingTextureException : Exception
    {
		public MissingTextureException() : base() { }
		public MissingTextureException(string message) : base(message) { }
    }

    public class GameSprite
	{
		public int ID { get; protected set; }
		public Texture2D Texture { get; protected set; }
		public WriteableBitmap Bitmap { get; protected set; }
		
		public Vector2F Offset { get; protected set; }

		public GameSprite(Texture2D texture)
		{
			Texture = texture;
		}

		public GameSprite(XElement element)
		{
			Initialize(element);
		}

		public bool HitTest(int x, int y)
		{
			var dx = (int)(x + Offset.X);
			var dy = (int)(y + Offset.Y);
			
			if (Texture != null && !Texture.IsTransparent(dx, dy))
				return true;

			return false;
		}

		public virtual void Initialize(XElement element)
		{
			var idAttribute = element.Attribute("id");

			if (idAttribute != null)
				ID = (int)idAttribute;

			var textureElement = element.Element("texture");
			var sourceElement = element.Element("source");
			var framesElement = element.Element("frames");
			var offsetElement = element.Element("offset");
			
			if (offsetElement != null)
				Offset = Vector2F.Parse(offsetElement.Value);

			var services = (ServiceContainer)ServiceLocator.Current;
			var contentManager = services.GetInstance<ContentManager>();
			var graphicsDevice = services.GetInstance<IGraphicsDeviceService>();

			var bounds = Vector4F.Parse((string)sourceElement);

			Texture2D sourceTexture = null;

			//Prefer loose art files, if they exist.
			var customTexturePath = $@"{Core.CustomArtPath}\{(string)textureElement}.png";
			if (File.Exists(customTexturePath))
			{
				using (var sourceStream = File.Open(customTexturePath, FileMode.Open, FileAccess.Read, FileShare.None))
					sourceTexture = Texture2D.FromStream(graphicsDevice.GraphicsDevice, sourceStream);
			}

			if (sourceTexture == null) {
				try
				{
					sourceTexture = contentManager.Load<Texture2D>((string)textureElement);
				} 
				catch
				{
					throw (new MissingTextureException((string)textureElement));
				}
			}

			var sourceBounds = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Z, (int)bounds.W);
			var sourceWidth = sourceBounds.Width;
			var sourceHeight = sourceBounds.Height;
			
			if (framesElement != null)
			{
				var firstFrame = framesElement.Elements("frame").FirstOrDefault();
				var frameBounds = Vector4F.Parse((string)firstFrame);

				var frameWidth = (int)frameBounds.Z;
				var frameHeight = (int)frameBounds.W;

				var frameData = new Color[frameWidth * frameHeight];
				
				sourceTexture.GetData(0, 
					new Rectangle((int)frameBounds.X, (int)frameBounds.Y, frameWidth, frameHeight), 
					frameData, 0, frameWidth * frameHeight);
				
				Texture = new Texture2D(graphicsDevice.GraphicsDevice, frameWidth, frameHeight);
				Texture.SetData<Color>(frameData);
			}
			else
			{
				var sourceData = new Color[sourceWidth * sourceHeight];

				sourceTexture.GetData(0, sourceBounds, sourceData, 0, sourceData.Length);
				
				Texture = new Texture2D(graphicsDevice.GraphicsDevice, sourceWidth, sourceHeight);
				Texture.SetData<Color>(sourceData);
			}

			Bitmap = Texture.ToBitmapSource();
		}
	}
}
