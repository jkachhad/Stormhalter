using System;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI.Documents;

public partial class ComponentDocument : UserControl
{
	private SegmentComponent _segmentComponent;
	
	public ComponentDocument()
	{
		InitializeComponent();
		
		var messenger = WeakReferenceMessenger.Default;
		
		messenger.Register<ActiveContentChanged>(this, (_, message) =>
		{
			if (message.Value is not SegmentComponent segmentComponent)
				return;
			
			_segmentComponent = segmentComponent;

			// set the text editor content
			_editor.Text = segmentComponent.Element.ToString();
		});
	}

	private void OnEditorChanged(object sender, EventArgs args)
	{
		if (sender is not TextEditor editor)
			return;
		
		try
		{
			var componentElement = XElement.Parse(editor.Text);
			var componentTypeAttribute = componentElement.Attribute("type");
			
			if (componentTypeAttribute is null)
				throw new XmlException("Component element is missing type attribute.");
			
			var componentTypename = $"Kesmai.WorldForge.Models.{componentTypeAttribute.Name}";
			var componentType = Type.GetType(componentTypename);

			if (componentType is null)
				throw new XmlException($"Component type '{componentTypename}' not found.");

			var ctor = componentType.GetConstructor([typeof(XElement)]);

			if (ctor is null)
				throw new XmlException($"Component type '{componentTypename}' is missing constructor with XElement parameter.");

			var component = ctor.Invoke([componentElement]) as TerrainComponent;

			if (component is null)
				throw new XmlException($"Component type '{componentTypename}' failed to instantiate.");
			
			_segmentComponent.Element = componentElement;
			
			_presenter.Component = component;
		}
		catch (Exception exception)
		{
			// ignore
		}
	}
}

public class ComponentViewModel : ObservableRecipient
{
	public string Name => "(Component)";
}
