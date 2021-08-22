using System;
using System.Windows;
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
		}
	}
}
