using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models
{
	public class RopeComponent : ActiveTeleporter
	{
		#region Static

		#endregion

		#region Fields

		private bool _isSecret;
		private int _slipChance;
		
		#endregion

		#region Properties and Events

		/// <summary>
		/// Gets or sets a value indicating whether this instance is secret.
		/// </summary>
		[Browsable(true)]
		public bool IsSecret
		{
			get { return _isSecret; }
			set { _isSecret = value; }
		}

		/// <summary>
		/// Gets or sets the chance to slip while interacting with this rope.
		/// </summary>
		[Browsable(true)]
		public int SlipChance
		{
			get => _slipChance;
			set => _slipChance = value;
		}
		
		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="RopeComponent"/> class.
		/// </summary>
		public RopeComponent(int ropeId, int x, int y, int region, bool isSecret = false) : base(ropeId, x, y, region)
		{
			_isSecret = isSecret;
		}
		
		public RopeComponent(XElement element) : base(element)
		{
			var isSecretElement = element.Element("isSecret");
			var slipChanceElement = element.Element("slipChance");

			if (isSecretElement != null)
				_isSecret = (bool)isSecretElement;

			if (slipChanceElement != null)
				_slipChance = (int)slipChanceElement;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override IEnumerable<ComponentRender> GetTerrain()
		{
			if (!_isSecret)
			{
				foreach (var render in base.GetTerrain())
					yield return render;
			}
		}
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			if (_isSecret)
				element.Add(new XElement("isSecret", _isSecret));

			if (_slipChance > 0)
				element.Add(new XElement("slipChance", _slipChance));
			
			return element;
		}

		public override TerrainComponent Clone()
		{
			return new RopeComponent(GetXElement());
		}

		#endregion
	}
}
