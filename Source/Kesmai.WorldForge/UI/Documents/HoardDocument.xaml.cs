using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents;

public partial class HoardDocument : UserControl
{
	private HoardViewModel? _viewModel;
	
	public HoardDocument()
	{
		InitializeComponent();

		DataContextChanged += OnDataContextChanged;
	}
	
	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is HoardViewModel oldViewModel)
			oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = e.NewValue as HoardViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
	}
	
	private void OnAddEntryClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Hoard is null)
			return;

		var entry = new TreasureEntry(_viewModel.Hoard);
		
		_viewModel.Hoard.Entries.Add(entry);
		_viewModel.AddStructuredEntry(entry);
	}

	private void OnRemoveEntryClick(object sender, RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Hoard is null || _viewModel.SelectedTreasureEntry is null)
			return;

		var result = MessageBox.Show("Are you sure you want to delete the selected entry?", "Delete Entry",
			MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

		if (result != MessageBoxResult.Yes)
			return;

		var hoard = _viewModel.Hoard;
		var selectedEntry = _viewModel.SelectedTreasureEntry;

		var currentIndex = hoard.Entries.IndexOf(selectedEntry);

		if (currentIndex < 0)
			return;

		hoard.Entries.RemoveAt(currentIndex);
		_viewModel.RemoveStructuredEntry(selectedEntry);

		if (hoard.Entries.Count > 0)
		{
			if (currentIndex >= hoard.Entries.Count)
				currentIndex = hoard.Entries.Count - 1;

			_viewModel.SelectedTreasureEntry = hoard.Entries.ElementAt(currentIndex);
		}
		else
		{
			_viewModel.SelectedTreasureEntry = null;
		}
	}
}

public class HoardViewModel : ObservableRecipient
{
	private SegmentHoard? _hoard;
	private TreasureEntry? _selectedTreasureEntry;
	private StructuredTreasureEntry? _selectedStructuredEntry;
	public ObservableCollection<StructuredTreasureEntry> StructuredEntries { get; } = new ObservableCollection<StructuredTreasureEntry>();

	public string Name => "(Hoard)";

	public SegmentHoard? Hoard
	{
		get => _hoard;
		set
		{
			if (SetProperty(ref _hoard, value))
			{
				StructuredEntries.Clear();
				if (_hoard != null)
					foreach (var entry in _hoard.Entries)
						StructuredEntries.Add(new StructuredHoardEntry(entry, OverallPackageChance, RefreshSelectedEntry));
				if (_hoard != null)
					SelectedTreasureEntry = _hoard.Entries.FirstOrDefault();
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
		set { if (SetProperty(ref _selectedStructuredEntry, value)) SelectedTreasureEntry = value?.Entry; }
	}

	public double OverallPackageChance
	{
		get
		{
			var body = Hoard?.Scripts.FirstOrDefault(script => script.Name == "GetChance")?.Body ?? String.Empty;
			var matches = Regex.Matches(body,
				@"\breturn\s+(?<chance>[0-9]+(?:\.[0-9]+)?)(?:[dDfFmM])?\s*;");
			if (matches.Count == 0) return 100;
			return Double.TryParse(matches[matches.Count - 1].Groups["chance"].Value,
				NumberStyles.Float, CultureInfo.InvariantCulture, out var chance) ? chance : 100;
		}
		set
		{
			var script = Hoard?.Scripts.FirstOrDefault(item => item.Name == "GetChance");
			if (script == null) return;
			var body = script.Body ?? String.Empty;
			var matches = Regex.Matches(body,
				@"\breturn\s+(?<chance>[0-9]+(?:\.[0-9]+)?)(?<suffix>[dDfFmM])?\s*;");
			if (matches.Count == 0) return;
			var match = matches[matches.Count - 1];
			var formatted = value.ToString("0.####", CultureInfo.InvariantCulture);
			script.Body = body.Remove(match.Groups["chance"].Index, match.Groups["chance"].Length)
				.Insert(match.Groups["chance"].Index, formatted);
			RebuildStructuredEntries();
			OnPropertyChanged();
		}
	}

	private void RebuildStructuredEntries()
	{
		var selected = SelectedTreasureEntry;
		StructuredEntries.Clear();
		if (Hoard != null)
			foreach (var entry in Hoard.Entries)
				StructuredEntries.Add(new StructuredHoardEntry(entry, OverallPackageChance, RefreshSelectedEntry));
		SelectedTreasureEntry = null;
		SelectedTreasureEntry = selected;
	}

	public void AddStructuredEntry(TreasureEntry entry)
	{
		var structured = new StructuredHoardEntry(entry, OverallPackageChance, RefreshSelectedEntry);
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

public class StructuredHoardEntry : StructuredTreasureEntry
{
	private readonly double _packageChance;
	public StructuredHoardEntry(TreasureEntry entry, double packageChance, Action<TreasureEntry>? refresh = null)
		: base(entry, refresh) => _packageChance = packageChance;
	public override double OverallChance => Chance * (_packageChance / 100d);
}
