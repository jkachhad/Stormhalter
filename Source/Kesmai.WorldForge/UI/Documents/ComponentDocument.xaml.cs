using System;
using System.Windows;
using System.Windows.Controls;
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

			_error.Visibility = Visibility.Hidden;
			_error.Text = String.Empty;
		}
		catch (Exception exception)
		{
			// ignore
			_error.Visibility = Visibility.Visible;
			_error.Text = exception.Message;
		}
	}
}

public class ComponentViewModel : ObservableRecipient
{
	public string Name => "(Component)";
}
