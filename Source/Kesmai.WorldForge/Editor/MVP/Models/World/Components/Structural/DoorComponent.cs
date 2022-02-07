using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models
{
	public class DoorComponent : TerrainComponent
	{
		#region Static

		#endregion

		#region Fields

		private int _openId;
		private int _closedId;
		private int _secretId;
		private int _destroyedId;

		private bool _isOpen;
		private bool _isSecret;
		private bool _isDestroyed;
		
		private bool _indestructible;

		#endregion

		#region Properties and Events
		
		
		/// <summary>
		/// Gets or sets a value indicating whether this door is open.
		/// </summary>
		[Browsable(true)]
		public bool IsOpen
		{
			get { return _isOpen; }
			set { _isOpen = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this door is secret.
		/// </summary>
		[Browsable(true)]
		public bool IsSecret
		{
			get { return _isSecret; }
			set { _isSecret = value; }
		}
		
		[Browsable(true)]
		public bool IsDestroyed
		{
			get { return _isDestroyed; }
			set { _isDestroyed = value; }
		}

		[Browsable(true)]
		public int OpenId
		{
			get => _openId;
			set => _openId = value;
		}

		[Browsable(true)]
		public int ClosedId
		{
			get => _closedId;
			set => _closedId = value;
		}

		[Browsable(true)]
		public int SecretId
		{
			get => _secretId;
			set => _secretId = value;
		}

		[Browsable(true)]
		public int DestroyedId
		{
			get => _destroyedId;
			set => _destroyedId = value;
		}
		
		/// <summary>
        /// Gets a value indicating whether this component is indestructible.
        /// </summary>
        [Browsable(true)]
        public bool IsIndestructible
        {
        	get { return _indestructible; }
        	set { _indestructible = value; }
        }

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="DoorComponent"/> class.
		/// </summary>
		public DoorComponent(int closedId, int openId, int secretId, int destroyedId, bool isSecret = false, bool isOpen = false)
		{
			_openId = openId;
			_closedId = closedId;
			_secretId = secretId;
			_destroyedId = destroyedId;

			_isOpen = isOpen;
			_isSecret = isSecret;
		}
		
		public DoorComponent(XElement element) : base(element)
		{
			_openId = (int)element.Element("openId");
			_closedId = (int)element.Element("closedId");
			_secretId = (int)element.Element("secretId");
			_destroyedId = (int)element.Element("destroyedId");

			var openElement = element.Element("isOpen");

			if (openElement != null)
				_isOpen = (bool)openElement;
			
			var secretElement = element.Element("isSecret");

			if (secretElement != null)
				_isSecret = (bool)secretElement;
			
			var destroyedElement = element.Element("isDestroyed");

			if (destroyedElement != null)
				_isDestroyed = (bool)destroyedElement;
				
		    var indestructibleElement = element.Element("indestructible");
        
        	if (indestructibleElement != null)
        		_indestructible = (bool)indestructibleElement;		
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override IEnumerable<ComponentRender> GetTerrain()
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var visibility = presenter.Visibility;
			var showOpened = IsOpen || visibility.OpenDoors;
			var showHidden = visibility.HideSecretDoors;
			var showDestroyed = visibility.BreakWalls;
			
			var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

			var id = (showOpened ? _openId : _closedId);

			if (_isSecret && showHidden)
				id = _secretId;
			
			if (_isDestroyed || (showDestroyed && ! IsIndestructible))
				id = _destroyedId;
			
			if (terrainManager.TryGetValue(id, out Terrain terrain))
				yield return new ComponentRender(terrain, Color);
		}
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			element.Add(new XElement("openId", _openId));
			element.Add(new XElement("closedId", _closedId));
			element.Add(new XElement("secretId", _secretId));
			element.Add(new XElement("destroyedId", _destroyedId));

			if (_isSecret)
				element.Add(new XElement("isSecret", _isSecret));

			if (_isOpen)
				element.Add(new XElement("isOpen", _isOpen));
			
			if (_isOpen)
				element.Add(new XElement("isDestroyed", _isDestroyed));

            if (_indestructible)
				element.Add(new XElement("indestructible", _indestructible));
				
			return element;
		}

		public override TerrainComponent Clone()
		{
			return new DoorComponent(GetXElement());
		}

		#endregion
	}
}
