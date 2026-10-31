using System;
using System.ComponentModel;
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
using Kesmai.WorldForge.Editor;
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
	/// <summary>
	/// Rebuilds the toolbar of the Project list's sections. A section with contents opens a grid of them; one without
	/// (Segment) opens straight away.
	/// </summary>
	private void OnSegmentTreeRootsChanged(object sender, EventArgs args)
	{
		if (sender is not UI.SegmentTreeControl tree)
			return;

		tree.FillToolbar(_segmentToolbar);
		_segmentToolbarTray.Visibility = _segmentToolbar.Items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
	}

	/// <summary>View > Program background: one of the listed colors, any color from Custom, or Default.</summary>
	private void OnProgramBackground(object sender, RoutedEventArgs args)
	{
		if (sender is System.Windows.Controls.MenuItem { Tag: string tag })
			UI.PaletteBackgroundPicker.ApplyProgram(tag, this);
	}

	/// <summary>View > Palette background: one of the listed colors, or any color from Custom.</summary>
	private void OnPaletteBackground(object sender, RoutedEventArgs args)
	{
		if (sender is System.Windows.Controls.MenuItem { Tag: string tag })
			UI.PaletteBackgroundPicker.Apply(tag, this);
	}

	/// <summary>View menu: shows a closed or hidden panel again, back in its place beside the documents.</summary>
	private void OnShowPanel(object sender, RoutedEventArgs args)
	{
		if (sender is not System.Windows.Controls.MenuItem { Tag: string which })
			return;

		var panels = which switch
		{
			"Project" => new[] { _projectPanel },
			"Properties" => new[] { _propertiesPanel },
			"Problems" => new[] { _problemsPanel },
			_ => new[] { _projectPanel, _propertiesPanel, _problemsPanel },
		};

		foreach (var panel in panels)
		{
			// a closed panel has left the layout; it goes back on the left, where the panels start
			if (panel.Parent is null)
				panel.AddToLayout(_dockingManager, AnchorableShowStrategy.Left);
			else
				panel.Show();

			panel.IsActive = true;
		}
	}

	public ApplicationWindow()
	{
		InitializeComponent();

		// the toolbar's region and tile show while a region is the document being edited
		if (DataContext is ApplicationPresenter presenter)
		{
			presenter.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == nameof(ApplicationPresenter.ActiveDocument))
					_regionInfo.Visibility = presenter.ActiveDocument is SegmentRegion ? Visibility.Visible : Visibility.Collapsed;
			};
		}

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

		Closing += OnClosing;

		_dockingManager.ActiveContentChanged += (s, e) =>
		{
			// When a document is activated in the layout panel, notify interested parties.
			if (_dockingManager.Layout.ActiveContent is LayoutDocument layoutDocument)
				WeakReferenceMessenger.Default.Send(new ActiveDocumentChanged(layoutDocument.Content));
		};
	}

	private void OnClosing(object sender, CancelEventArgs args)
	{
		if (DataContext is ApplicationPresenter presenter && !presenter.ConfirmDiscardChanges("exiting"))
			args.Cancel = true;
	}

	private void OnDocumentClosed(object sender, DocumentClosedEventArgs args)
	{
		if (args.Document != null)
			WeakReferenceMessenger.Default.Send(new DocumentClosed(args.Document));
	}
}