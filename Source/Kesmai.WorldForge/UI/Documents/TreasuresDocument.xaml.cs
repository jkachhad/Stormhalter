using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

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
		public TreasuresDocument()
		{
			InitializeComponent();
		}
	}

	public class TreasuresViewModel : ObservableRecipient
	{
		public string Name => "(Treasures)";
		
		private Segment _segment;
		
		private SegmentTreasure _selectedTreasure;
		private TreasureEntry _selectedTreasureEntry;

		public SegmentTreasures Treasures => _segment.Treasures;
		
		private static int _newTreasureCount = 1;

		public SegmentTreasure SelectedTreasure
		{
			get => _selectedTreasure;
			set => SetProperty(ref _selectedTreasure, value, true);
		}
		
		public TreasureEntry SelectedTreasureEntry
		{
			get => _selectedTreasureEntry;
			set => SetProperty(ref _selectedTreasureEntry, value, true);
		}

		public RelayCommand AddTreasureCommand { get; set; }
		public RelayCommand<SegmentTreasure> RemoveTreasureCommand { get; set; }
		public RelayCommand<SegmentTreasure> CopyTreasureCommand { get; set; }
		
		public RelayCommand AddTreasureEntryCommand { get; set; }
		public RelayCommand<TreasureEntry> RemoveTreasureEntryCommand { get; set; }
		public RelayCommand<TreasureEntry> CopyTreasureEntryCommand { get; set; }
		
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
			
			AddTreasureEntryCommand = new RelayCommand(AddTreasureEntry);
			
			RemoveTreasureEntryCommand = new RelayCommand<TreasureEntry>(RemoveTreasureEntry,
				(entry) => (SelectedTreasure != null && SelectedTreasureEntry != null));
			RemoveTreasureEntryCommand.DependsOn(() => SelectedTreasureEntry);
			
			CopyTreasureEntryCommand = new RelayCommand<TreasureEntry>(CopyTreasureEntry, 
				(entry) => (SelectedTreasure != null && SelectedTreasureEntry != null));
			CopyTreasureEntryCommand.DependsOn(() => SelectedTreasureEntry);
			
			WeakReferenceMessenger.Default.Register<TreasuresViewModel, TreasureEntry.TreasureEntryWeightChanged>
				(this, OnWeightChanged);
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
	}
}