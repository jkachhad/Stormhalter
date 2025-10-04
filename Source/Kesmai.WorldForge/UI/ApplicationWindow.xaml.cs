using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using AvalonDock;
using AvalonDock.Layout;
using CommonServiceLocator;
using DigitalRune.Game.Interop;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Content;
using DigitalRune.Mathematics.Content;
using DigitalRune.Storages;
using Microsoft.Xna.Framework.Content;

namespace Kesmai.WorldForge;

public record DocumentClosed(LayoutDocument Document);
public record ActiveDocumentChanged(object Content);

public record ActivateDocument(object Content);

/// <summary>
/// Interaction logic for ApplicationWindow.xaml
/// </summary>
public partial class ApplicationWindow : Window
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationWindow"/> class.
	/// </summary>
	public ApplicationWindow()
	{
		InitializeComponent();

		WeakReferenceMessenger.Default
			.Register<ApplicationWindow, ToolStartMessage>(
				this, (r, m) => {
					if (m.NewTool is Kesmai.WorldForge.DrawTool || m.NewTool is Kesmai.WorldForge.PaintTool)
					{
						_componentsWindow.Show();
					}
					else
					{
						_componentsWindow.Hide();
					}
				});
		WeakReferenceMessenger.Default
			.Register<ApplicationWindow, ToolStopMessage>(
				this, (r, m) => {
				});

		WeakReferenceMessenger.Default.Register<ApplicationWindow, ActivateDocument>(this, (r, m) =>
		{
			if (m.Content is null)
				return;

			foreach (var document in _dockingManager.Layout.Descendents().OfType<LayoutDocument>())
			{
				if (document.Content != m.Content)
					continue;
				
				document.IsActive = true;
				break;
			}
		});
		
		_dockingManager.DocumentClosed += OnDocumentClosed;
		_dockingManager.ActiveContentChanged += (s, e) =>
		{
			// When a document is activated in the layout panel, notify interested parties.
			if (_dockingManager.Layout.ActiveContent is LayoutDocument layoutDocument)
				WeakReferenceMessenger.Default.Send(new ActiveDocumentChanged(layoutDocument.Content));
		};
	}

	private void OnDocumentClosed(object sender, DocumentClosedEventArgs args)
	{
		if (args.Document != null)
			WeakReferenceMessenger.Default.Send(new DocumentClosed(args.Document));
	}
}