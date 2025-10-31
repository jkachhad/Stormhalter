using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Documents;
using Kesmai.WorldForge.Windows;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Models;

public abstract class TerrainComponent : ObservableObject, IComponentProvider
{
    #region Static

    private static Color _defaultColor = Color.White;

    #endregion
    public Guid Id { get; } = Guid.NewGuid();

    #region Fields

    #endregion

    #region Properties and Events

    /// <summary>
    /// Gets the name descriptor for this component. This properly should not be closed or serialized.
    /// It's primarily internal for tooltips.
    /// </summary>
    [Browsable( false )]
    public string Name { get; set; } = "(unknown)";

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    [Browsable( true )]
    public Color Color { get; set; }

    /// <summary>
    /// Gets or sets a comment
    /// </summary>
    [Browsable( true )]
    public String Comment { get; set; }

    public bool IsEditable => true;

    #endregion

    #region Constructors and Cleanup

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainComponent"/> class.
    /// </summary>
    protected TerrainComponent()
    {
        Color = _defaultColor;
    }

    protected TerrainComponent( XElement element )
    {
        /* The components palette includes a name attribute for the purposes of describing components in the
		 * editor. It it not serialized. */
        var nameAttribute = element.Attribute( "name" );

        if( nameAttribute != null )
            Name = nameAttribute.Value;

        var colorElement = element.Element( "color" );

        if( colorElement != null )
        {
            var colorR = (int)colorElement.Attribute( "r" );
            var colorG = (int)colorElement.Attribute( "g" );
            var colorB = (int)colorElement.Attribute( "b" );
            var colorA = (int)colorElement.Attribute( "a" );

            Color = new Color( colorR, colorG, colorB, colorA );
        }
        else
        {
            Color = _defaultColor;
        }

        var commentElement = element.Element( "comment" );
        if( commentElement != null )
        {
            Comment = commentElement.Value;
        }
    }

    #endregion

    #region Methods
    
    /// <summary>
    /// Gets an XML element that describes this component.
    /// </summary>
    public virtual XElement GetSerializingElement()
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

    public XElement GetReferencingElement() => GetSerializingElement();

    protected virtual string GetTypeAlias()
    {
        return GetType().Name;
    }

    public void SetColor( int r, int g, int b )
    {
        Color = new Color( r, g, b );
    }

    public void SetColor( int r, int g, int b, int a )
    {
        Color = new Color( r, g, b, a );
    }

    public abstract IComponentProvider Clone();

    #endregion

    public void AddComponent(ObservableCollection<IComponentProvider> collection)
    {
        // create a clone of this component and add it to the tile.
        collection.Add(Clone());
    }

    public void RemoveComponent(ObservableCollection<IComponentProvider> collection)
    {
        // remove this specific component.
        collection.Remove(this);
    }
    
    public IEnumerable<IComponentProvider> GetComponents()
    {
        yield return this;
    }
    
    public IEnumerable<ComponentRender> GetRenders(int mx, int my) => GetRenders();

    public virtual IEnumerable<ComponentRender> GetRenders()
    {
        yield break;
    }
    
    public ComponentFrame GetComponentFrame()
    {
        return new TerrainComponentFrame(this);
    }
}