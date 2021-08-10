using System.ComponentModel;
using System.IO;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models
{
	public class Web : StaticComponent
	{
		private bool _allowDispel;
		
		[Browsable(true)]
		public bool AllowDispel
		{
			get => _allowDispel;
			set => _allowDispel = value;
		}
		
		public Web(bool allowDispel) : base(131)
		{
			_allowDispel = allowDispel;
		}

		public Web(XElement element) : base(element)
		{
			var allowDispel = element.Element("allowDispel");

			if (allowDispel != null)
				_allowDispel = (bool)allowDispel;
		}
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			if (_allowDispel)
				element.Add(new XElement("allowDispel", _allowDispel));

			return element;
		}

		public override TerrainComponent Clone()
		{
			return new Web(GetXElement());
		}
	}
}