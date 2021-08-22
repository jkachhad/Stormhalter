using System.Collections.ObjectModel;
using System.Windows.Controls;
using Kesmai.WorldForge.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.UI
{
	public partial class ComponentGrid : ListBox
	{
		public ComponentGrid()
		{
			InitializeComponent();
		}
	}
	
	public class ComponentsCategory : ObservableObject
	{
		public string Name { get; set; }

		private ObservableCollection<TerrainComponent> _components;

		public ObservableCollection<TerrainComponent> Components
		{
			get => _components;
			set => SetProperty(ref _components, value);
		}

		public ComponentsCategory()
		{
			_components = new ObservableCollection<TerrainComponent>();
		}
	}
}