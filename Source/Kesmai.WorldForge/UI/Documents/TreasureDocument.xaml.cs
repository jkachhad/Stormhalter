using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;

namespace Kesmai.WorldForge.UI.Documents;

public partial class TreasureDocument : UserControl
{
	private TreasureViewModel? _viewModel;

	public TreasureDocument()
	{
		InitializeComponent();

		DataContextChanged += OnDataContextChanged;
	}
	
	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is TreasureViewModel oldViewModel)
			oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = e.NewValue as TreasureViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
	}

	private void OnAddEntryClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Treasure is null)
			return;
		
		var entry = new TreasureEntry(_viewModel.Treasure);
		
		_viewModel.Treasure.Entries.Add(entry);
		_viewModel.AddStructuredEntry(entry);
	}

	private void OnRemoveEntryClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Treasure is null || _viewModel.SelectedTreasureEntry is null)
			return;
		
		var result = MessageBox.Show("Are you sure you want to delete the selected entry?", "Delete Entry",
			MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

		if (result != MessageBoxResult.Yes)
			return;

		var treasure = _viewModel.Treasure;
		var selectedEntry = _viewModel.SelectedTreasureEntry;

		var currentIndex = treasure.Entries.IndexOf(selectedEntry);

		if (currentIndex < 0)
			return;

		treasure.Entries.RemoveAt(currentIndex);
		_viewModel.RemoveStructuredEntry(selectedEntry);

		if (treasure.Entries.Count > 0)
		{
			if (currentIndex >= treasure.Entries.Count)
				currentIndex = treasure.Entries.Count - 1;

			_viewModel.SelectedTreasureEntry = treasure.Entries.ElementAt(currentIndex);
		}
		else
		{
			_viewModel.SelectedTreasureEntry = null;
		}
	}
}

public class TreasureViewModel : ObservableRecipient
{
	private SegmentTreasure? _treasure;
	private TreasureEntry? _selectedTreasureEntry;
	private StructuredTreasureEntry? _selectedStructuredEntry;
	public ObservableCollection<StructuredTreasureEntry> StructuredEntries { get; } = new ObservableCollection<StructuredTreasureEntry>();

	public string Name => "(Treasure)";
	
	public SegmentTreasure? Treasure
	{
		get => _treasure;
		set
		{
			if (SetProperty(ref _treasure, value))
			{
				StructuredEntries.Clear();
				if (_treasure != null)
					foreach (var entry in _treasure.Entries)
						StructuredEntries.Add(new StructuredTreasureEntry(entry, RefreshSelectedEntry));
				if (_treasure != null)
					SelectedTreasureEntry = _treasure.Entries.FirstOrDefault();
			}
		}
	}

	public TreasureEntry? SelectedTreasureEntry
	{
		get => _selectedTreasureEntry;
		set
		{
			if (!SetProperty(ref _selectedTreasureEntry, value)) return;
			var structured = StructuredEntries.FirstOrDefault(item => ReferenceEquals(item.Entry, value));
			if (!ReferenceEquals(_selectedStructuredEntry, structured))
			{
				_selectedStructuredEntry = structured;
				OnPropertyChanged(nameof(SelectedStructuredEntry));
			}
		}
	}

	public StructuredTreasureEntry? SelectedStructuredEntry
	{
		get => _selectedStructuredEntry;
		set
		{
			if (!SetProperty(ref _selectedStructuredEntry, value)) return;
			SelectedTreasureEntry = value?.Entry;
		}
	}

	public void AddStructuredEntry(TreasureEntry entry)
	{
		var structured = new StructuredTreasureEntry(entry, RefreshSelectedEntry);
		StructuredEntries.Add(structured);
		SelectedStructuredEntry = structured;
	}

	public void RemoveStructuredEntry(TreasureEntry entry)
	{
		var structured = StructuredEntries.FirstOrDefault(item => ReferenceEquals(item.Entry, entry));
		if (structured != null) StructuredEntries.Remove(structured);
	}

	private void RefreshSelectedEntry(TreasureEntry entry)
	{
		var structured = StructuredEntries.FirstOrDefault(item => ReferenceEquals(item.Entry, entry));
		if (structured != null && !ReferenceEquals(SelectedStructuredEntry, structured))
			SelectedStructuredEntry = structured;
		SelectedTreasureEntry = null;
		SelectedTreasureEntry = entry;
	}
}

public class StructuredTreasureEntry : ObservableObject
{
	private static readonly Regex ReturnExpression = new Regex(
		@"(?ms)\breturn\s+(?<expression>.*?);", RegexOptions.Compiled);
	public TreasureEntry Entry { get; }
	private readonly Action<TreasureEntry>? _refresh;
	private Script OnCreate => Entry.Scripts.First(script => script.Name == "OnCreate");

	public StructuredTreasureEntry(TreasureEntry entry, Action<TreasureEntry>? refresh = null)
	{
		Entry = entry;
		_refresh = refresh;
		Entry.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(TreasureEntry.Weight)) OnPropertyChanged(nameof(Weight));
			if (args.PropertyName == nameof(TreasureEntry.Chance)) OnPropertyChanged(nameof(Chance));
			if (args.PropertyName == nameof(TreasureEntry.Notes)) OnPropertyChanged(nameof(Notes));
		};
		OnCreate.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(Script.IsEnabled)) OnPropertyChanged(nameof(IsEnabled));
			if (args.PropertyName == nameof(Script.Body)) OnPropertyChanged(nameof(ItemExpression));
		};
	}

	public bool IsEnabled { get => OnCreate.IsEnabled; set { OnCreate.IsEnabled = value; _refresh?.Invoke(Entry); } }
	public int Weight { get => Entry.Weight; set { Entry.Weight = value; Entry.Treasure.InvalidateChance(); } }
	public double Chance => Entry.Chance;
	public virtual double OverallChance => Chance;
	public string Notes { get => Entry.Notes; set => Entry.Notes = value; }
	public string ItemExpression
	{
		get
		{
			var match = ReturnExpression.Matches(OnCreate.Body ?? String.Empty).Cast<Match>().LastOrDefault();
			return match != null ? match.Groups["expression"].Value.Trim() : "Custom/unsupported";
		}
		set
		{
			if (String.IsNullOrWhiteSpace(value) || value == "Custom/unsupported") return;
			var body = OnCreate.Body ?? String.Empty;
			var match = ReturnExpression.Matches(body).Cast<Match>().LastOrDefault();
			OnCreate.Body = match != null
				? body.Remove(match.Groups["expression"].Index, match.Groups["expression"].Length)
					.Insert(match.Groups["expression"].Index, value.Trim())
				: $"\treturn {value.Trim()};";
			OnPropertyChanged();
		}
	}
}
