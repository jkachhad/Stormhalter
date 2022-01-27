using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;
using DigitalRune.ServiceLocation;
using CommonServiceLocator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.Xna.Framework;
using System.Xml.Linq;

namespace Kesmai.WorldForge.UI.Documents
{
	public partial class SpawnsDocument : UserControl
	{
		public class GetActiveEntity : RequestMessage<Entity>
        {
        }
		public class GetCurrentTypeSelection : RequestMessage<int>
		{
		}
		public SpawnsDocument()
		{
			InitializeComponent();
			_regionPresenter.Initialize();
			_locationPresenter.Initialize();

			WeakReferenceMessenger.Default
				.Register<SpawnsDocument, SpawnsViewModel.SelectedLocationSpawnerChangedMessage>(
					this, OnLocationSpawnerChanged);

			WeakReferenceMessenger.Default
				.Register<SpawnsDocument, SpawnsViewModel.SelectedRegionSpawnerChangedMessage>(
					this, OnRegionSpawnerChanged);

			WeakReferenceMessenger.Default
				.Register<SpawnsDocument, Spawner>(
					this, (r,m) => { _typeSelector.SelectedIndex = m is LocationSpawner ? 0 : 1; });

			WeakReferenceMessenger.Default.Register<SpawnsDocument, GetActiveEntity>(this,
				(r, m) => m.Reply(GetSelectedEntity()));

			WeakReferenceMessenger.Default.Register<SpawnsDocument, GetCurrentTypeSelection>(this,
				(r, m) => m.Reply(_typeSelector.SelectedIndex));

			WeakReferenceMessenger.Default.Register<SpawnsDocument, UnregisterEvents>(this,
				(r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });
		}


		public Entity GetSelectedEntity()
		{
			SpawnEntry entry = null;
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			if (presenter.ActiveDocument is not SpawnsViewModel)
				return null;
			if (_typeSelector.SelectedIndex == 0)
			{
				entry = _locationEntities.SelectedItem as SpawnEntry;
			} else
            {
				entry = _regionEntities.SelectedItem as SpawnEntry;
			}
			if (entry is null)
				return null as Entity;

			return entry.Entity;
		}

		private void OnLocationSpawnerChanged(SpawnsDocument recipient, SpawnsViewModel.SelectedLocationSpawnerChangedMessage message)
		{
			_scriptsTabControl.SelectedIndex = 0;
			
			var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
			var segment = segmentRequest.Response;

			var spawn = message.Value;
			if (spawn != null)
            {
				_locationPresenter.Region = segment.GetRegion(spawn.Region);
				_locationPresenter.SetLocation(spawn);
			}
			_locationSpawnerList.ScrollIntoView(_locationSpawnerList.SelectedItem);
		}

		private void OnRegionSpawnerChanged(SpawnsDocument recipient, SpawnsViewModel.SelectedRegionSpawnerChangedMessage message)
		{
			_scriptsTabControl.SelectedIndex = 0;

			var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
			var segment = segmentRequest.Response;

			var spawn = message.Value;
			if (spawn != null)
			{
				_regionPresenter.Region = segment.GetRegion(spawn.Region);
				_regionPresenter.SetLocation(spawn);
			}
			_regionSpawnerList.ScrollIntoView(_regionSpawnerList.SelectedItem);
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

		public class SelectedRegionSpawnerChangedMessage : ValueChangedMessage<RegionSpawner>
		{
			public SelectedRegionSpawnerChangedMessage(RegionSpawner value) : base(value)
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
			set
			{
				SetProperty(ref _selectedRegionSpawner, value, true);

				if (value != null)
				{
					WeakReferenceMessenger.Default.Send(new SelectedRegionSpawnerChangedMessage(value));
					value.CalculateStats();
				}
			}
		}

		public SegmentSpawns Source => _segment.Spawns;
		public ObservableCollection<Entity> Entities => _segment.Entities;
		
		public RelayCommand AddLocationSpawnerCommand { get; set; }
		public RelayCommand<LocationSpawner> RemoveLocationSpawnerCommand { get; set; }
		public RelayCommand<LocationSpawner> CopyLocationSpawnerCommand { get; set; }
		public RelayCommand<LocationSpawner> CloneLocationSpawnerCommand { get; set; }

		public RelayCommand AddRegionSpawnerCommand { get; set; }
		public RelayCommand<RegionSpawner> RemoveRegionSpawnerCommand { get; set; }
		public RelayCommand<RegionSpawner> CopyRegionSpawnerCommand { get; set; }
		public RelayCommand<RegionSpawner> CloneRegionSpawnerCommand { get; set; }

		public RelayCommand PasteSpawnerCommand { get; set; }

		public RelayCommand JumpEntityCommand { get; set; }
		public SpawnsViewModel(Segment segment)
		{
			_segment = segment;
			
			AddLocationSpawnerCommand = new RelayCommand(AddLocationSpawner);
			RemoveLocationSpawnerCommand = new RelayCommand<LocationSpawner>(RemoveLocationSpawner,
				(spawner) => SelectedLocationSpawner != null);
			RemoveLocationSpawnerCommand.DependsOn(() => SelectedLocationSpawner);
			CopyLocationSpawnerCommand = new RelayCommand<LocationSpawner>(CopySpawner,
				(spawner) => SelectedLocationSpawner != null);
			CopyLocationSpawnerCommand.DependsOn(() => SelectedLocationSpawner);
			CloneLocationSpawnerCommand = new RelayCommand<LocationSpawner>(CloneSpawner,
				(spawner) => SelectedLocationSpawner != null);
			CloneLocationSpawnerCommand.DependsOn(() => SelectedLocationSpawner);

			AddRegionSpawnerCommand = new RelayCommand(AddRegionSpawner);
			RemoveRegionSpawnerCommand = new RelayCommand<RegionSpawner>(RemoveRegionSpawner,
				(spawner) => SelectedRegionSpawner != null);
			RemoveRegionSpawnerCommand.DependsOn(() => SelectedRegionSpawner);
			CopyRegionSpawnerCommand = new RelayCommand<RegionSpawner>(CopySpawner,
				(spawner) => SelectedRegionSpawner != null);
			CopyRegionSpawnerCommand.DependsOn(() => SelectedRegionSpawner);
			CloneRegionSpawnerCommand = new RelayCommand<RegionSpawner>(CloneSpawner,
				(spawner) => SelectedRegionSpawner != null);
			CloneRegionSpawnerCommand.DependsOn(() => SelectedRegionSpawner);

			PasteSpawnerCommand = new RelayCommand(PasteSpawner);

			JumpEntityCommand = new RelayCommand(JumpEntity);
		}

		public void JumpEntity ()
        {
			var entityRequest = WeakReferenceMessenger.Default.Send<SpawnsDocument.GetActiveEntity>();
			var entity = entityRequest.Response;
			WeakReferenceMessenger.Default.Send(entity);
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

		public void CopySpawner(Spawner spawner)
        {
			if (spawner is LocationSpawner l)
				Clipboard.SetText(l.GetXElement().ToString());
			else if (spawner is RegionSpawner r)
				Clipboard.SetText(r.GetXElement().ToString());
        }

		public void PasteSpawner()
		{
			XDocument clipboard = null;
			try
			{
				clipboard = XDocument.Parse(Clipboard.GetText());
			}
			catch { }
			if (clipboard is null || clipboard.Root.Name.ToString() != "spawn")
				return;

			var spawner = default(Spawner);
			if (clipboard.Root.Attribute("type").Value == "LocationSpawner")
			{
				spawner = new LocationSpawner(clipboard.Root);
				while (Source.Location.Any(s => s.Name == spawner.Name)) {
					spawner.Name = $"Copy of {spawner.Name}";
				}
				Source.Location.Add(spawner as LocationSpawner);
			}
            else
            {
				spawner = new RegionSpawner(clipboard.Root);
				while (Source.Region.Any(s => s.Name == spawner.Name))
				{
					spawner.Name = $"Copy of {spawner.Name}";
				}
				Source.Region.Add(spawner as RegionSpawner);
			}

			if (spawner != null)
			{
				foreach (var entryElement in clipboard.Root.Elements("entry"))
				{
					var entry = new SpawnEntry(entryElement);

					var entity = default(Entity);
					var entityName = (string)entryElement.Attribute("entity");

					if (!String.IsNullOrEmpty(entityName))
					{
						entity = _segment.Entities.FirstOrDefault(
							e => String.Equals(e.Name,
								entityName, StringComparison.Ordinal));
					}

					entry.Entity = entity;

					if (entry.Entity != null)
						spawner.Entries.Add(entry);
				}

			}

		}

		public void CloneSpawner(Spawner spawner)
        {
			var newSpawner = default(Spawner);
			if (spawner is LocationSpawner l) {
				newSpawner = new LocationSpawner(l.GetXElement());
				while (Source.Location.Any(s => s.Name == newSpawner.Name))
					newSpawner.Name = $"Copy of {newSpawner.Name}";
				Source.Location.Add(newSpawner as LocationSpawner);
			}
			else if (spawner is RegionSpawner r) {
				newSpawner = new RegionSpawner(r.GetXElement());
				while (Source.Region.Any(s => s.Name == newSpawner.Name))
					newSpawner.Name = $"Copy of {newSpawner.Name}";
				Source.Region.Add(newSpawner as RegionSpawner);
			}
			foreach (var entity in spawner.Entries)
            {
				newSpawner.Entries.Add(entity);
            }
        }
	}
}