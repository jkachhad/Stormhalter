using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.UI.Documents
{
	public class TreasureItemScriptTemplate : ScriptTemplate
	{
		public override IEnumerable<string> GetSegments()
		{
			yield return "#load \"WorldForge\"\nItemEntity OnCreate(MobileEntity from, Container container)\n{";
			yield return "}";
		}
	}
	
	public partial class TreasuresDocument : UserControl
	{
		public class GetActiveEntity : RequestMessage<Entity>
		{
		}
		public TreasuresDocument()
		{
			InitializeComponent();

			WeakReferenceMessenger.Default
				.Register<TreasuresDocument, TreasuresViewModel.SelectedTreasureChangedMessage>(this, (r, m) => { _treasuresList.ScrollIntoView(_treasuresList.SelectedItem); });
			
			WeakReferenceMessenger.Default.Register<TreasuresDocument, GetActiveEntity>(this,
				(r, m) => m.Reply(GetSelectedEntity()));

			WeakReferenceMessenger.Default.Register<TreasuresDocument, UnregisterEvents>(this,
				(r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });
		}

		public Entity GetSelectedEntity()
        {
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			if (presenter.ActiveDocument is not TreasuresViewModel)
				return null;
			if (_entitiesList.SelectedItem != null)
				return _entitiesList.SelectedItem as Entity;
			return null;
		}
	}

	public class TreasuresViewModel : ObservableRecipient
	{
		public class SelectedTreasureChangedMessage : ValueChangedMessage<SegmentTreasure>
		{
			public SelectedTreasureChangedMessage(SegmentTreasure value) : base(value)
			{
			}
		}
		public string Name => "(Treasures)";
		
		private Segment _segment;
		
		private SegmentTreasure _selectedTreasure;
		private TreasureEntry _selectedTreasureEntry;

		public SegmentTreasures Treasures => _segment.Treasures;
		
		private static int _newTreasureCount = 1;

		public SegmentTreasure SelectedTreasure
		{
			get => _selectedTreasure;
			set
			{
				SetProperty(ref _selectedTreasure, value, true);

				if (value != null)
					WeakReferenceMessenger.Default.Send(new SelectedTreasureChangedMessage(value));

				_relatedEntities.Clear();

				foreach (Entity entity in _segment.Entities)
                {
					foreach (Script script in entity.Scripts)
                    {
						if (script.Blocks[1].Contains(_selectedTreasure.Name))
							_relatedEntities.Add(entity);
                    }
                }
			}
		}
		
		public TreasureEntry SelectedTreasureEntry
		{
			get => _selectedTreasureEntry;
			set => SetProperty(ref _selectedTreasureEntry, value, true);
		}

		private ObservableCollection<Entity> _relatedEntities = new ObservableCollection<Entity>();

		public ObservableCollection<Entity> RelatedEntities { get => _relatedEntities; }

		public RelayCommand AddTreasureCommand { get; set; }
		public RelayCommand<SegmentTreasure> RemoveTreasureCommand { get; set; }
		public RelayCommand<SegmentTreasure> CopyTreasureCommand { get; set; }
		public RelayCommand ImportTreasureCommand { get; set; }
		public RelayCommand<SegmentTreasure> ExportTreasureCommand { get; set; }
		public RelayCommand AddTreasureEntryCommand { get; set; }
		public RelayCommand<TreasureEntry> RemoveTreasureEntryCommand { get; set; }
		public RelayCommand<TreasureEntry> CopyTreasureEntryCommand { get; set; }
		public RelayCommand JumpEntityCommand { get; set; }

		public TreasuresViewModel(Segment segment)
		{
			_segment = segment;
			
			AddTreasureCommand = new RelayCommand(AddTreasure);
			
			RemoveTreasureCommand = new RelayCommand<SegmentTreasure>(RemoveTreasure,
				(treasure) => (SelectedTreasure != null));
			RemoveTreasureCommand.DependsOn(() => SelectedTreasure);
			
			CopyTreasureCommand = new RelayCommand<SegmentTreasure>(CopyTreasure, 
				(treasure) => (SelectedTreasure != null));
			CopyTreasureCommand.DependsOn(() => SelectedTreasure);

			ImportTreasureCommand = new RelayCommand(ImportTreasure);

			ExportTreasureCommand = new RelayCommand<SegmentTreasure>(ExportTreasure,
				(treasure) => (SelectedTreasure != null));
			ExportTreasureCommand.DependsOn(() => SelectedTreasure);

			AddTreasureEntryCommand = new RelayCommand(AddTreasureEntry);
			
			RemoveTreasureEntryCommand = new RelayCommand<TreasureEntry>(RemoveTreasureEntry,
				(entry) => (SelectedTreasure != null && SelectedTreasureEntry != null));
			RemoveTreasureEntryCommand.DependsOn(() => SelectedTreasureEntry);
			
			CopyTreasureEntryCommand = new RelayCommand<TreasureEntry>(CopyTreasureEntry, 
				(entry) => (SelectedTreasure != null && SelectedTreasureEntry != null));
			CopyTreasureEntryCommand.DependsOn(() => SelectedTreasureEntry);
			
			WeakReferenceMessenger.Default.Register<TreasuresViewModel, TreasureEntry.TreasureEntryWeightChanged>
				(this, OnWeightChanged);

			JumpEntityCommand = new RelayCommand(JumpEntity);

		}

		public void JumpEntity()
		{
			var entityRequest = WeakReferenceMessenger.Default.Send<TreasuresDocument.GetActiveEntity>();
			var entity = entityRequest.Response;
			WeakReferenceMessenger.Default.Send(entity);
		}
		private void OnWeightChanged(TreasuresViewModel recipient, TreasureEntry.TreasureEntryWeightChanged message)
		{
			_selectedTreasure.InvalidateChance();
		}

		public void AddTreasure()
		{
			var newTreasure = new SegmentTreasure()
			{
				Name = $"Treasure {_newTreasureCount++}"
			};
			
			Treasures.Add(newTreasure);
			SelectedTreasure = newTreasure;
		}

		public void RemoveTreasure(SegmentTreasure treasure)
		{
			var result = MessageBox.Show($"Are you sure you wish to delete '{treasure.Name}'?", 
				"WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result != MessageBoxResult.No)
				Treasures.Remove(treasure);
		}
		
		public void CopyTreasure(SegmentTreasure treasure)
		{
			var cloneTreasure = new SegmentTreasure(treasure)
			{
				Name = $"Copy of {treasure.Name}"
			};
			
			Treasures.Add(cloneTreasure);
			SelectedTreasure = cloneTreasure;
		}

		public void AddTreasureEntry()
		{
			var newTreasureEntry = new TreasureEntry(SelectedTreasure);
			
			SelectedTreasure.Entries.Add(newTreasureEntry);
			SelectedTreasureEntry = newTreasureEntry;
		}

		public void RemoveTreasureEntry(TreasureEntry entry)
		{
			var result = MessageBox.Show($"Are you sure you wish to delete this entry?", 
				"WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result != MessageBoxResult.No)
				SelectedTreasure.Entries.Remove(entry);
		}
		
		public void CopyTreasureEntry(TreasureEntry entry)
		{
			var clonedEntry = new TreasureEntry(entry);
			
			SelectedTreasure.Entries.Add(clonedEntry);
			SelectedTreasureEntry = clonedEntry;
		}

		public void ImportTreasure()
        {
			XDocument clipboard = null;
			try
			{
				clipboard = XDocument.Parse(Clipboard.GetText());
			}
			catch { }
			if (clipboard is null || clipboard.Root.Name.ToString() != "treasure")
				return;
			var newTreasure = new SegmentTreasure(clipboard.Root);
            bool isNameTaken;
			do // Why doesn't Treasures have linq support? Treasures.Any would have simplified this.
			{
				isNameTaken = false;
				foreach (var treasure in Treasures)
				{
					if (treasure.Name == newTreasure.Name)
					{
						isNameTaken = true;
						newTreasure.Name = $"Copy of {newTreasure.Name}";
					}
				}
			} while (isNameTaken);
			Treasures.Add(newTreasure);
		}

		public void ExportTreasure(SegmentTreasure treasure)
        {
			Clipboard.SetText(treasure.GetXElement().ToString());
        }
	}
}