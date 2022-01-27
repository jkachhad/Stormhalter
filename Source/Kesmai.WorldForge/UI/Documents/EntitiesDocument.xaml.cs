using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.ServiceLocation;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.UI.Documents
{
	public class EntitySpawnScriptTemplate : ScriptTemplate
	{
		public override IEnumerable<string> GetSegments()
		{
			yield return "#load \"WorldForge\"\nCreatureEntity OnSpawn()\n{";
			yield return "}";
		}
	}
	
	public class EntityDeathScriptTemplate : ScriptTemplate
	{
		public override IEnumerable<string> GetSegments()
		{
			yield return "#load \"WorldForge\"\nvoid OnDeath(MobileEntity source, MobileEntity killer)\n{";
			yield return "}";
		}
	}
	
	public partial class EntitiesDocument : UserControl
	{
		public class GetSelectedSpawner : RequestMessage<Spawner>
		{
		}
		public class GetCurrentScriptSelection : RequestMessage<String>
		{
		}

		public EntitiesDocument()
		{
			InitializeComponent();
			
			WeakReferenceMessenger.Default
				.Register<EntitiesDocument, EntitiesViewModel.SelectedEntityChangedMessage>(this, OnEntityChanged);
			
			WeakReferenceMessenger.Default
				.Register<EntitiesDocument, Entity>(
					this, (r, m) => { ChangeEntity(m); });

			WeakReferenceMessenger.Default.Register<EntitiesDocument, GetSelectedSpawner>(this,
					(r, m) => m.Reply(_spawnersList.SelectedItem as Spawner));

			WeakReferenceMessenger.Default.Register<EntitiesDocument, GetCurrentScriptSelection>(this,
				(r, m) => m.Reply(GetScriptSelection()));

			WeakReferenceMessenger.Default.Register<EntitiesDocument, UnregisterEvents>(this,
				(r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });
		}

		private void ChangeEntity(Entity entity)
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var viewmodel = presenter.Documents.Where(d => d is EntitiesViewModel).FirstOrDefault() as EntitiesViewModel;
			presenter.ActiveDocument = viewmodel;
			if (entity is not null)
            {
				viewmodel.SelectedEntity = entity;
            }
		}
			private void OnEntityChanged(EntitiesDocument recipient, EntitiesViewModel.SelectedEntityChangedMessage message)
		{
			_scriptsTabControl.SelectedIndex = 0;
			_entityList.ScrollIntoView(_entityList.SelectedItem);
		}

		public String GetScriptSelection ()
        {
			if (_scriptsTabControl.HasItems)
			{
				ContentPresenter cp = _scriptsTabControl.Template.FindName("PART_SelectedContentHost", _scriptsTabControl) as ContentPresenter;
				ScriptEditor editor = _scriptsTabControl.ContentTemplate.FindName("_editor", cp) as ScriptEditor;
				return editor.SelectedText;
			} else 
				return null;

		}
	}

	public class EntitiesViewModel : ObservableRecipient
	{
		public class SelectedEntityChangedMessage : ValueChangedMessage<Entity>
		{
			public SelectedEntityChangedMessage(Entity value) : base(value)
			{
			}
		}

		public string Name => "(Entities)";

		private int _newEntityCount = 1;

		private Entity _selectedEntity;
		private Segment _segment;

		public Entity SelectedEntity
		{
			get => _selectedEntity;
			set
			{
				SetProperty(ref _selectedEntity, value, true);

				_relatedSpawners.Clear();
				foreach (Spawner spawner in _segment.Spawns.Location.Where(s => s.Entries.Any(e => e.Entity == SelectedEntity)))
				{
					_relatedSpawners.Add(spawner);
				}
				foreach (Spawner spawner in _segment.Spawns.Region.Where(s => s.Entries.Any(e => e.Entity == SelectedEntity)))
				{
					_relatedSpawners.Add(spawner);
				}


				if (value != null)
					WeakReferenceMessenger.Default.Send(new SelectedEntityChangedMessage(value));
			}
		}

		public SegmentEntities Source => _segment.Entities;

		private ObservableCollection<Spawner> _relatedSpawners = new ObservableCollection<Spawner>();

		public ObservableCollection<Spawner> RelatedSpawners { get=> _relatedSpawners;}
		
		public RelayCommand AddEntityCommand { get; set; }
		public RelayCommand<Entity> RemoveEntityCommand { get; set; }
		public RelayCommand<Entity> CopyEntityCommand { get; set; }
		public RelayCommand<Entity> ExportEntityCommand { get; set; }
		public RelayCommand ImportEntityComamnd { get; set; }
		public RelayCommand JumpSpawnerCommand { get; set; }

		public EntitiesViewModel(Segment segment)
		{
			_segment = segment;

			AddEntityCommand = new RelayCommand(AddEntity);
			
			RemoveEntityCommand = new RelayCommand<Entity>(RemoveEntity, 
				(entity) => (SelectedEntity != null));
			RemoveEntityCommand.DependsOn(() => SelectedEntity);
			
			CopyEntityCommand = new RelayCommand<Entity>(CopyEntity,
				(entity) => (SelectedEntity != null));
			CopyEntityCommand.DependsOn(() => SelectedEntity);

			ExportEntityCommand = new RelayCommand<Entity>(ExportEntity,
				(entity) => (SelectedEntity != null));
			ExportEntityCommand.DependsOn(() => SelectedEntity);

			ImportEntityComamnd = new RelayCommand(ImportEntity);


			JumpSpawnerCommand = new RelayCommand(JumpSpawner);

		}

		public void JumpSpawner()
		{
			var spawnRequest = WeakReferenceMessenger.Default.Send<EntitiesDocument.GetSelectedSpawner>();
			var spawn = spawnRequest.Response;
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			if (spawnRequest.HasReceivedResponse)
			{
				Spawner target = spawnRequest.Response;
				var ActiveDocument = presenter.Documents.Where(d => d is SpawnsViewModel).FirstOrDefault() as SpawnsViewModel;
				presenter.ActiveDocument = ActiveDocument;
				if (target is LocationSpawner)
					(ActiveDocument as SpawnsViewModel).SelectedLocationSpawner = target as LocationSpawner;
				if (target is RegionSpawner)
					(ActiveDocument as SpawnsViewModel).SelectedRegionSpawner = target as RegionSpawner;
				WeakReferenceMessenger.Default.Send(target as Spawner);
			}
			WeakReferenceMessenger.Default.Send(spawn as Spawner);
		}

		public void AddEntity()
		{
			var newEntity = new Entity()
			{
				Name = $"Entity {_newEntityCount++}"
			};
			
			Source.Add(newEntity);
			SelectedEntity = newEntity;
		}

		public void RemoveEntity(Entity entity)
		{
			var result = MessageBox.Show($"Are you sure you wish to delete '{entity.Name}'?", 
				"WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result != MessageBoxResult.No)
				_segment?.Entities.Remove(entity);
		}

		public void CopyEntity(Entity entity)
		{
			if (entity.Clone() is Entity clonedEntity)
			{
				Source.Add(clonedEntity);
				SelectedEntity = clonedEntity;
			}
		}

		public void ExportEntity(Entity entity)
        {
			Clipboard.SetText(entity.GetXElement().ToString());
        }

		public void ImportEntity()
		{
			XDocument clipboard = null;
			try
			{
				clipboard = XDocument.Parse(Clipboard.GetText());
			}
			catch { }
			if (clipboard is null || clipboard.Root.Name.ToString() != "entity")
				return;

			var newEntity = new Entity(clipboard.Root);

			while (Source.Any(e => e.Name == newEntity.Name))
				newEntity.Name = $"Copy of {newEntity.Name}";

			Source.Add(newEntity);
			SelectedEntity = newEntity;
		}
	}
}