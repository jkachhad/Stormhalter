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
	public ComponentDocument()
	{
		InitializeComponent();
		
		var messenger = WeakReferenceMessenger.Default;
		
		messenger.Register<ActiveContentChanged>(this, (_, message) =>
		{
			if (message.Value is not TerrainComponent segmentComponent)
				return;

			// set the text editor content
			_editor.Text = segmentComponent.GetXElement().ToString();
		});
	}

	private void OnEditorChanged(object sender, EventArgs args)
	{
		if (sender is not TextEditor editor)
			return;

		var componentElement = XElement.Parse(editor.Text);

		try
		{
			var componentTypename = $"Kesmai.WorldForge.Models.{componentElement.Name}";
			var componentType = Type.GetType(componentTypename);

			if (componentType is null)
				throw new XmlException($"Component type '{componentTypename}' not found.");

			var ctor = componentType.GetConstructor([typeof(XElement)]);

			if (ctor is null)
				throw new XmlException(
					$"Component type '{componentTypename}' is missing constructor with XElement parameter.");

			var component = ctor.Invoke([componentElement]) as TerrainComponent;

			if (component is null)
				throw new XmlException($"Component type '{componentTypename}' failed to instantiate.");
			
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
