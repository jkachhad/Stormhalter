using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.UI.Documents
{
	public partial class SpawnsDocument : UserControl
	{
		public SpawnsDocument()
		{
			InitializeComponent();
			
			WeakReferenceMessenger.Default
				.Register<SpawnsDocument, SpawnsViewModel.SelectedLocationSpawnerChangedMessage>(
					this, OnLocationSpawnerChanged);
		}
		
		private void OnLocationSpawnerChanged(SpawnsDocument recipient, SpawnsViewModel.SelectedLocationSpawnerChangedMessage message)
		{
			_scriptsTabControl.SelectedIndex = 0;
		}
	}
	
	public class SpawnsViewModel : ObservableRecipient
	{
		private int _newSpawnerCount = 1;
		
		public class SelectedLocationSpawnerChangedMessage : ValueChangedMessage<LocationSpawner>
		{
			public SelectedLocationSpawnerChangedMessage(LocationSpawner value) : base(value)
			{
			}
		}
		
		public string Name => "(Spawns)";
		
		private Segment _segment;
		private LocationSpawner _selectedLocationSpawner;
		private RegionSpawner _selectedRegionSpawner;
		
		public LocationSpawner SelectedLocationSpawner
		{
			get => _selectedLocationSpawner;
			set
			{
				SetProperty(ref _selectedLocationSpawner, value, true);
				
				if (value != null)
					WeakReferenceMessenger.Default.Send(new SelectedLocationSpawnerChangedMessage(value));
			}
		}

		public RegionSpawner SelectedRegionSpawner
		{
			get => _selectedRegionSpawner;
			set => SetProperty(ref _selectedRegionSpawner, value, true);
		}

		public SegmentSpawns Source => _segment.Spawns;
		public ObservableCollection<Entity> Entities => _segment.Entities;
		
		public RelayCommand AddLocationSpawnerCommand { get; set; }
		public RelayCommand<LocationSpawner> RemoveLocationSpawnerCommand { get; set; }
		
		public RelayCommand AddRegionSpawnerCommand { get; set; }
		public RelayCommand<RegionSpawner> RemoveRegionSpawnerCommand { get; set; }

		public SpawnsViewModel(Segment segment)
		{
			_segment = segment;
			
			AddLocationSpawnerCommand = new RelayCommand(AddLocationSpawner);
			RemoveLocationSpawnerCommand = new RelayCommand<LocationSpawner>(RemoveLocationSpawner,
				(spawner) => SelectedLocationSpawner != null);
			RemoveLocationSpawnerCommand.DependsOn(() => SelectedLocationSpawner);
			
			AddRegionSpawnerCommand = new RelayCommand(AddRegionSpawner);
			RemoveRegionSpawnerCommand = new RelayCommand<RegionSpawner>(RemoveRegionSpawner,
				(spawner) => SelectedRegionSpawner != null);
			RemoveRegionSpawnerCommand.DependsOn(() => SelectedRegionSpawner);
		}
		
		public void AddLocationSpawner()
		{
			var newSpawner = new LocationSpawner()
			{
				Name = $"Spawner {_newSpawnerCount++}"
			};
			
			Source.Location.Add(newSpawner);
			SelectedLocationSpawner = newSpawner;
		}

		public void RemoveLocationSpawner(LocationSpawner spawner)
		{
			var result = MessageBox.Show($"Are you sure you wish to delete '{spawner.Name}'?", 
				"WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result != MessageBoxResult.No)
				Source.Location.Remove(spawner);
		}
		
		public void AddRegionSpawner()
		{
			var newSpawner = new RegionSpawner()
			{
				Name = $"Spawner {_newSpawnerCount++}"
			};
			
			Source.Region.Add(newSpawner);
			SelectedRegionSpawner = newSpawner;
		}

		public void RemoveRegionSpawner(RegionSpawner spawner)
		{
			var result = MessageBox.Show($"Are you sure you wish to delete '{spawner.Name}'?", 
				"WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result != MessageBoxResult.No)
				Source.Region.Remove(spawner);
		}
	}
}