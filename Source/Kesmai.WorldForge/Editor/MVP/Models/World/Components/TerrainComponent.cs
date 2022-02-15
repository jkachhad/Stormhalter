using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Models
{
	public abstract class TerrainComponent : ObservableObject
    {
		#region Static

	    private static Color _defaultColor = Color.White;

		#endregion

		#region Fields

		#endregion

		#region Properties and Events

		/// <summary>
		/// Gets the name descriptor for this component. This properly should not be closed or serialized.
		/// It's primarily internal for tooltips.
		/// </summary>
		[Browsable(false)]
		public string Name { get; set; } = "(unknown)";

		/// <summary>
		/// Gets or sets the color.
		/// </summary>
		[Browsable(true)]
		public Color Color { get; set; }

		/// <summary>
		/// Gets or sets a comment
		/// </summary>
		[Browsable(true)]
		public String Comment { get; set; }

		#endregion

		#region Constructors and Cleanup

	    /// <summary>
	    /// Initializes a new instance of the <see cref="TerrainComponent"/> class.
	    /// </summary>
	    protected TerrainComponent()
	    {
		    Color = _defaultColor;
	    }
	    
	    protected TerrainComponent(XElement element)
	    {
		    /* The components palette includes a name attribute for the purposes of describing components in the
		     * editor. It it not serialized. */
		    var nameAttribute = element.Attribute("name");

		    if (nameAttribute != null)
			    Name = nameAttribute.Value;
		    
		    var colorElement = element.Element("color");

		    if (colorElement != null)
		    {
			    var colorR = (int)colorElement.Attribute("r");
			    var colorG = (int)colorElement.Attribute("g");
			    var colorB = (int)colorElement.Attribute("b");
			    var colorA = (int)colorElement.Attribute("a");

			    Color = new Color(colorR, colorG, colorB, colorA);
		    }
		    else
		    {
			    Color = _defaultColor;
		    }

			var commentElement = element.Element("comment");
			if (commentElement != null)
            {
				Comment = commentElement.Value;
            }
	    }

		#endregion

		#region Methods
		
		/// <summary>
		/// Gets an XML element that describes this component.
		/// </summary>
		public virtual XElement GetXElement()
		{
			var element = new XElement("component");

			element.Add(new XAttribute("type", GetTypeAlias()));

			if (Color != Color.White)
			{
				element.Add(new XElement("color",
					new XAttribute("r", Color.R), new XAttribute("g", Color.G), new XAttribute("b", Color.B),
					new XAttribute("a", Color.A)
				));
			}
			if (!String.IsNullOrWhiteSpace(Comment))
            {
				element.Add(new XElement("comment", Comment));
            }


			return element;
		}

		protected virtual string GetTypeAlias()
		{
			return GetType().Name;
		}
		
		/// <summary>
		/// Gets the rendered terrain.
		/// </summary>
		public virtual IEnumerable<ComponentRender> GetTerrain()
		{
			yield break;
		}
		
		public void SetColor(int r, int g, int b)
	    {
		    Color = new Color(r, g, b);
	    }
	    
	    public void SetColor(int r, int g, int b, int a)
	    {
		    Color = new Color(r, g, b, a);
	    }

	    public abstract TerrainComponent Clone();

	    public virtual IEnumerable<Button> GetInspectorActions()
	    {
			var templateButton = new Button()
		    {
			    Content = new TextBlock()
			    {
				    Foreground = Color.OrangeRed,
				    Shadow = Color.Black,

				    Font = "Tahoma14Bold",
				    Text = "Template",

				    Margin = new Vector4F(3, 3, 3, 3)
			    }
		    };
			
			templateButton.Click += (o, args) =>
			{
				var element = GetXElement();
				var typeAttribute = element.Attribute("type");

				if (typeAttribute != null)
				{
					element.Name = typeAttribute.Value;
					typeAttribute.Remove();
				}

				Clipboard.SetText(element.ToString());
			};

			yield return templateButton;
	    }

	    #endregion
    }
}