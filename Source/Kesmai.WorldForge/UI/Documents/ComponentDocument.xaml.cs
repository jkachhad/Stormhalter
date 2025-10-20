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
			_segmentComponent.Element = XElement.Parse(editor.Text);
			_presenter.Component = _segmentComponent.Component;
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
