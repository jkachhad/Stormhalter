using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.ServiceLocation;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.UI.Documents;

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
	
public class EntityIncomingPlayerScriptTemplate : ScriptTemplate
{
	public override IEnumerable<string> GetSegments()
	{
		yield return "#load \"WorldForge\"\nvoid OnIncomingPlayer(MobileEntity source, PlayerEntity player)\n{";
		yield return "}";
	}
}

public static class DependencyObjectExtensions
{
	public static T GetParentOfType<T>(this DependencyObject element) where T : DependencyObject
	{
		if (element == null)
		{
			return null;
		}

		var parent = VisualTreeHelper.GetParent(element);

		if (parent is T correctlyTyped)
		{
			return correctlyTyped;
		}

		return GetParentOfType<T>(parent);
	}
}
	
public partial class EntitiesDocument : UserControl
{
	private Entity _draggedEntity;
	
	private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		var viewModel = DataContext as EntitiesViewModel;
		if (viewModel != null)
		{
			viewModel.SelectedEntity = e.NewValue as Entity;
		}
	}
	
	private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		var treeView = (TreeView)sender;
		var hit = VisualTreeHelper.HitTest(treeView, e.GetPosition(treeView));
		var selectedItem = hit.VisualHit.GetParentOfType<TreeViewItem>();
		if (selectedItem != null)
		{
			if (selectedItem.DataContext is Entity entity)
			{
				_draggedEntity = (Entity)selectedItem.DataContext;
				DragDrop.DoDragDrop(treeView, _draggedEntity, DragDropEffects.Move);
			}
			else if (selectedItem.DataContext is EntitiesViewModel.WfGroup group)
			{
				// Handle the case where the selected item is a group, if needed
			}
		}
	}

	private void TreeView_DragOver(object sender, DragEventArgs e)
	{
		if (!e.Data.GetDataPresent(typeof(Entity)))
		{
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}
	}

	private void TreeView_Drop(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(typeof(Entity)))
		{
			var treeView = (TreeView)sender;
			var hit = VisualTreeHelper.HitTest(treeView, e.GetPosition(treeView));
			var targetItem = hit.VisualHit.GetParentOfType<TreeViewItem>();
			if (targetItem != null)
			{
				var targetGroup = (EntitiesViewModel.WfGroup)targetItem.DataContext;
				var viewModel = (EntitiesViewModel)DataContext;
				var sourceGroup = viewModel.Groups.First(g => g.Entities.Contains(_draggedEntity));
				viewModel.MoveEntity(_draggedEntity, sourceGroup, targetGroup);
			}
		}
	}

	private void TextBox_Loaded(object sender, RoutedEventArgs e)
	{
		var textBox = sender as TextBox;
		var dataContext = textBox.DataContext;

		if (dataContext is Entity)
		{
			// DataContext is correctly set to an instance of the Entity class
			Debug.WriteLine("DataContext is correctly set to an instance of the Entity class");
		}
		else
		{
			// DataContext is not set to an instance of the Entity class
			Debug.WriteLine("DataContext is not set to an instance of the Entity class");
		}
	}

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
	
	

	public class WfGroup : ObservableObject
	{
		private ObservableCollection<Entity> _entities;
		private string _name;
		public string Name 
		{ 
			get { return _name; }
			set
			{
				if (_name != value)
				{
					UpdateEntityGroupNames(value);
					_name = value;
					OnPropertyChanged("Name");
				}
			}
		}
		public ObservableCollection<Entity> Entities 
		{
			get { return _entities; }
			set
			{
				if (_entities != value)
				{
					_entities = value;
					OnPropertyChanged("Entities");
				}
			}
		}
		
		private void UpdateEntityGroupNames(string newGroupName)
		{
			foreach (var entity in Entities)
			{
				entity.Group = newGroupName;
			}
		}
		
		public WfGroup()
		{
			_entities = new ObservableCollection<Entity>();
		}
	}
	
	public class WfGroups : ObservableObject
	{

		public void ImportSegmentEntities(ObservableCollection<Entity> entities)
		{
			
			
			foreach (Entity entity in entities.OrderBy(e => e.Name))
			{
				if (entity.Group == null)
					entity.Group = "Unassigned";
				var group = Groups.Where(g => g.Name == entity.Group).FirstOrDefault();
				if (group is null)
				{
					group = new WfGroup()
					{
						Name = entity.Group,
						Entities = new ObservableCollection<Entity>()
					};
					Groups.Add(group);
				}
				group.Entities.Add(entity);
			}
			
			
		}
		public ObservableCollection<WfGroup> Groups { get; set; } = new();
	}
	public ObservableCollection<WfGroup> Groups
	{
		get { return _groups.Groups; }
	}
	
	public ObservableCollection<Entity> Entities
	{
		get { return _segment.Entities; }
	}
	public string Name => "(Entities)";

	private int _newEntityCount = 1;
	private int _newGroupCount = 1;

	private Entity _selectedEntity;
	private Segment _segment;
	private WfGroups _groups = new WfGroups();

	
	public Entity SelectedEntity
	{
		get => _selectedEntity;
		set
		{
			SetProperty(ref _selectedEntity, value, true);
			OnPropertyChanged("SelectedEntity");
			
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
	
	public RelayCommand AddGroupCommand { get; set; }
	
	public RelayCommand<WfGroup> RemoveGroupCommand { get; set; }

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
		
		AddGroupCommand = new RelayCommand(AddGroup);
		
		RemoveGroupCommand = new RelayCommand<WfGroup>(RemoveGroup,
			(group) => (_groups.Groups.Count > 0));
		
		_groups.ImportSegmentEntities(_segment.Entities);
		
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

	public void AddGroup()
	{
		var newGroup = new WfGroup()
		{
			Name = "Unassigned",
			Entities = new ObservableCollection<Entity>()
		};
		_groups.Groups.Add(newGroup);
		
	}
	
	public void RemoveGroup(WfGroup group)
	{
		var result = MessageBox.Show($"Are you sure you wish to delete '{group.Name}'?", 
			"WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

		if (result != MessageBoxResult.No && _groups.Groups.Count > 0)
			 _groups.Groups.Remove(group);
	}

	public void AddEntity()
	{
		var newEntity = new Entity()
		{
			Name = $"Entity {_newEntityCount++}",
			Group = "Unassigned"
		};
		
		var unassigned = _groups.Groups.Where((x => x.Name == "Unassigned")).FirstOrDefault();
		Source.Add(newEntity);
		if (unassigned is not null)
			unassigned.Entities.Add(newEntity);
		else
		{
			var group = new WfGroup()
			{
				Name = "Unassigned",
				Entities = new ObservableCollection<Entity>()
			};
			group.Entities.Add(newEntity);
			_groups.Groups.Add(group);
		}
		SelectedEntity = newEntity;
	}
	
	public void MoveEntity(Entity entity, WfGroup sourceGroup, WfGroup destinationGroup)
	{
		// Remove the entity from the source group
		sourceGroup.Entities.Remove(entity);

		// Add the entity to the destination group
		destinationGroup.Entities.Add(entity);

		// Update the group of the entity
		entity.Group = destinationGroup.Name;
	}

	public void RemoveEntity(Entity entity)
	{
		var result = MessageBox.Show($"Are you sure you wish to delete '{entity.Name}'?", 
			"WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

		if (result != MessageBoxResult.No)
		{
			_segment?.Entities.Remove(entity);
			var group = _groups.Groups.Where(g => g.Entities.Contains(entity)).FirstOrDefault();
			if (group != null)
				group.Entities.Remove(entity);
		}
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