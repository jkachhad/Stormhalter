using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using AvalonDock;
using AvalonDock.Layout;

namespace Kesmai.WorldForge;

public record DocumentClosed(LayoutDocument Document);
public record DocumentActivate(object Content);

/// <summary>
/// Interaction logic for ApplicationWindow.xaml
/// </summary>
public partial class ApplicationWindow : Window
{
	private Game _game;

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationWindow"/> class.
	/// </summary>
	public ApplicationWindow()
	{
		_game = new Game(this);
			
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

		WeakReferenceMessenger.Default.Register<ApplicationWindow, DocumentActivate>(this, (r, m) =>
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
	}

	private void OnDocumentClosed(object sender, DocumentClosedEventArgs args)
	{
		if (args.Document != null)
			WeakReferenceMessenger.Default.Send(new DocumentClosed(args.Document));
	}
}