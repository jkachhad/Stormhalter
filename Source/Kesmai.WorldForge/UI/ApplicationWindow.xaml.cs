using System.Windows;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace Kesmai.WorldForge
{
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
		}
	}
}
