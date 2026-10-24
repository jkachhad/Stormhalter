#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Editor.Assets;
using Kesmai.WorldForge.Editor.Quests;

namespace Kesmai.WorldForge.UI.Documents;

public partial class QuestMakerDocument : UserControl
{
	private QuestMakerViewModel? _viewModel;

	public QuestMakerDocument()
	{
		InitializeComponent();

		_kind.ItemsSource = Enum.GetValues(typeof(QuestStageKind));

		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (_viewModel is not null)
			_viewModel.Refreshed -= BuildBranchMap;

		_viewModel = e.NewValue as QuestMakerViewModel;

		if (_viewModel is not null)
		{
			_viewModel.Refreshed += BuildBranchMap;
			BuildBranchMap();
		}
	}

	/// <summary>Draws the quest as a tree of paths from the first stage: each choice leads into the stage it opens.</summary>
	private void BuildBranchMap()
	{
		_branches.Items.Clear();

		var quest = _viewModel?.Quest;

		if (quest is null)
			return;

		var shown = new HashSet<string>();

		TreeViewItem StageItem(string id, string? via)
		{
			var stage = quest.Stages.FirstOrDefault(s => s.Id == id);
			var label = stage is null ? $"{id} (missing!)" : $"{id} · {Describe(stage)}";

			var item = new TreeViewItem
			{
				Header = via is null ? label : $"\"{via}\" → {label}",
				Tag = stage,
				IsExpanded = true,
			};

			if (stage is null)
			{
				item.Foreground = System.Windows.Media.Brushes.Red;
				return item;
			}

			if (!shown.Add(id))
			{
				item.Header += "  (see above)";
				return item;
			}

			foreach (var choice in stage.Choices)
				item.Items.Add(StageItem(choice.Next, choice.Label));

			return item;
		}

		_branches.Items.Add(StageItem(quest.StartStage, null));

		foreach (var id in QuestCodeGenerator.Unreachable(quest))
		{
			var item = StageItem(id, null);
			item.Header = $"{item.Header}  (unreachable)";
			item.Foreground = System.Windows.Media.Brushes.Gray;
			_branches.Items.Add(item);
		}
	}

	private static string Describe(QuestStage stage) => stage.Kind switch
	{
		QuestStageKind.Kill => $"kill {stage.Count} {stage.Target}",
		QuestStageKind.Collect => $"bring {stage.Count} {stage.Target}",
		QuestStageKind.Escort => $"escort {stage.EscortName}",
		QuestStageKind.End => "end",
		_ => "talk",
	};

	private void OnBranchSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (_viewModel is not null && e.NewValue is TreeViewItem { Tag: QuestStage stage })
			_viewModel.SelectedStage = stage;
	}

	private void OnNewQuest(object sender, RoutedEventArgs e) => _viewModel?.NewQuest(sample: false);
	private void OnSampleQuest(object sender, RoutedEventArgs e) => _viewModel?.NewQuest(sample: true);
	private void OnDeleteQuest(object sender, RoutedEventArgs e) => _viewModel?.DeleteQuest();
	private void OnRefreshQuests(object sender, RoutedEventArgs e) => _viewModel?.RefreshFiles();
	private void OnAddStage(object sender, RoutedEventArgs e) => _viewModel?.AddStage();
	private void OnRemoveStage(object sender, RoutedEventArgs e) => _viewModel?.RemoveStage();
	private void OnMoveStageUp(object sender, RoutedEventArgs e) => _viewModel?.MoveStage(-1);
	private void OnMoveStageDown(object sender, RoutedEventArgs e) => _viewModel?.MoveStage(1);
	private void OnAddChoice(object sender, RoutedEventArgs e) => _viewModel?.AddChoice();
	private void OnRemoveChoice(object sender, RoutedEventArgs e) => _viewModel?.RemoveChoice(_choices.SelectedItem as QuestChoice);
	private void OnSave(object sender, RoutedEventArgs e) => _viewModel?.Save();
	private void OnCopyCode(object sender, RoutedEventArgs e) => _viewModel?.CopyCode();
	private void OnAddKillHooks(object sender, RoutedEventArgs e) => _viewModel?.AddKillHooks();
	private void OnCreateGiver(object sender, RoutedEventArgs e) => _viewModel?.CreateGiverEntity();
}

/// <summary>A quest file found in the segment's Source folder.</summary>
public record QuestFile(string Name, string Path)
{
	public override string ToString() => Name;
}

public class QuestMakerViewModel : ObservableObject
{
	private Segment? _segment;
	private QuestDefinition _quest = QuestDefinition.CreateSample();
	private QuestStage? _selectedStage;
	private QuestFile? _selectedFile;
	private string? _loadedPath;
	private string _preview = String.Empty;
	private string _problems = String.Empty;
	private string _status = String.Empty;
	private bool _loadingFile;

	private readonly DispatcherTimer _refresh;
	private readonly Dictionary<QuestStage, string> _idsBeforeChange = new();

	public string Name => "(Quest Maker)";

	/// <summary>Raised after the preview, problems and stage list have been brought up to date.</summary>
	public event Action? Refreshed;

	public ObservableCollection<QuestFile> QuestFiles { get; } = new();
	public ObservableCollection<string> StageIds { get; } = new();
	public ObservableCollection<string> EntityNames { get; } = new();
	public ObservableCollection<string> ItemTypes { get; } = new();

	public QuestMakerViewModel()
	{
		_refresh = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, (_, _) =>
		{
			_refresh!.Stop();
			Refresh();
		}, Dispatcher.CurrentDispatcher);

		Attach(_quest);
		SelectedStage = _quest.Stages.FirstOrDefault();

		_ = LoadItemTypesAsync();

		Refresh();
	}

	private async System.Threading.Tasks.Task LoadItemTypesAsync()
	{
		var catalog = await AssetCatalog.LoadAsync();

		foreach (var item in catalog.Items)
			ItemTypes.Add(item);
	}

	public Segment? Segment
	{
		get => _segment;
		set
		{
			if (SetProperty(ref _segment, value))
			{
				RefreshFiles();
				Refresh();
			}
		}
	}

	public QuestDefinition Quest
	{
		get => _quest;
		private set
		{
			Detach(_quest);

			if (SetProperty(ref _quest, value))
			{
				Attach(_quest);
				SelectedStage = _quest.Stages.FirstOrDefault();
				Refresh();
			}
		}
	}

	public QuestStage? SelectedStage
	{
		get => _selectedStage;
		set => SetProperty(ref _selectedStage, value);
	}

	public QuestFile? SelectedFile
	{
		get => _selectedFile;
		set
		{
			if (SetProperty(ref _selectedFile, value) && value is not null && !_loadingFile)
				Open(value);
		}
	}

	public string Preview { get => _preview; private set => SetProperty(ref _preview, value); }
	public string Problems { get => _problems; private set => SetProperty(ref _problems, value); }
	public string Status { get => _status; private set => SetProperty(ref _status, value); }

	private string SegmentName => String.IsNullOrWhiteSpace(_segment?.Name) ? "Segment" : _segment!.Name;

	private string? QuestFolder => String.IsNullOrWhiteSpace(_segment?.Directory) ? null : Path.Combine(_segment!.Directory, "Source", "Quests");

	public void RefreshFiles()
	{
		_loadingFile = true;

		QuestFiles.Clear();

		if (!String.IsNullOrWhiteSpace(_segment?.Directory) && Directory.Exists(Path.Combine(_segment!.Directory, "Source")))
		{
			foreach (var path in Directory.GetFiles(Path.Combine(_segment.Directory, "Source"), "*.cs", SearchOption.AllDirectories))
			{
				if (path.Contains(@"\bin\") || path.Contains(@"\obj\"))
					continue;

				var head = File.ReadLines(path).Take(10).ToList();

				if (head.Any(line => line.TrimStart().StartsWith(QuestDefinition.Marker.Trim())))
					QuestFiles.Add(new QuestFile(Path.GetFileNameWithoutExtension(path), path));
			}
		}

		_selectedFile = QuestFiles.FirstOrDefault(f => String.Equals(f.Path, _loadedPath, StringComparison.OrdinalIgnoreCase));
		OnPropertyChanged(nameof(SelectedFile));

		_loadingFile = false;

		EntityNames.Clear();

		if (_segment is not null)
		{
			foreach (var name in _segment.Entities.Select(e => e.Name).Where(n => !String.IsNullOrWhiteSpace(n)).OrderBy(n => n))
				EntityNames.Add(name);
		}
	}

	private void Open(QuestFile file)
	{
		try
		{
			var quest = QuestDefinition.FromSource(File.ReadAllText(file.Path));

			if (quest is null)
			{
				Status = $"{file.Name} wasn't made by the Quest Maker, so it can't be opened here.";
				return;
			}

			_loadedPath = file.Path;
			Quest = quest;
			Status = $"Opened {file.Path}.";
		}
		catch (Exception e)
		{
			Status = $"Couldn't open {file.Name}: {e.Message}";
		}
	}

	public void NewQuest(bool sample)
	{
		var quest = sample ? QuestDefinition.CreateSample() : new QuestDefinition();

		if (!sample)
		{
			var start = new QuestStage { Id = "Start", Kind = QuestStageKind.Talk, Text = "Here is what I need." };
			start.Choices.Add(new QuestChoice { Label = "I'll do it", Next = "Done" });

			quest.Stages.Add(start);
			quest.Stages.Add(new QuestStage { Id = "Done", Kind = QuestStageKind.End, Text = "Thank you!" });
		}

		var name = sample ? "SampleQuest" : "NewQuest";
		var number = 1;
		var taken = QuestFiles.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

		quest.Name = name;

		while (taken.Contains(quest.Name))
			quest.Name = $"{name}{++number}";

		_loadedPath = null;
		_selectedFile = null;
		OnPropertyChanged(nameof(SelectedFile));

		Quest = quest;
		Status = sample
			? "Loaded a sample with a kill, a collect and an escort path. Save it to add it to the segment."
			: "Started a new quest. Save it to add it to the segment.";
	}

	public void DeleteQuest()
	{
		if (_loadedPath is null || !File.Exists(_loadedPath))
		{
			Status = "This quest hasn't been saved, so there is no file to delete.";
			return;
		}

		if (MessageBox.Show($"Delete {_loadedPath}?", "Delete quest", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
			return;

		File.Delete(_loadedPath);
		Status = $"Deleted {_loadedPath}.";
		_loadedPath = null;

		RefreshFiles();
	}

	public void AddStage()
	{
		var id = "Stage";
		var number = 1;

		while (_quest.Stages.Any(s => s.Id == id))
			id = $"Stage{++number}";

		var stage = new QuestStage { Id = id, Kind = QuestStageKind.Talk };
		var index = SelectedStage is null ? _quest.Stages.Count : _quest.Stages.IndexOf(SelectedStage) + 1;

		_quest.Stages.Insert(index, stage);
		SelectedStage = stage;
	}

	public void RemoveStage()
	{
		if (SelectedStage is null)
			return;

		var index = _quest.Stages.IndexOf(SelectedStage);

		_quest.Stages.Remove(SelectedStage);
		SelectedStage = _quest.Stages.Count == 0 ? null : _quest.Stages[Math.Min(index, _quest.Stages.Count - 1)];
	}

	public void MoveStage(int offset)
	{
		if (SelectedStage is null)
			return;

		var index = _quest.Stages.IndexOf(SelectedStage);
		var target = index + offset;

		if (target < 0 || target >= _quest.Stages.Count)
			return;

		var stage = SelectedStage;

		_quest.Stages.Move(index, target);
		SelectedStage = stage;
	}

	public void AddChoice()
	{
		if (SelectedStage is null)
			return;

		var next = _quest.Stages.Select(s => s.Id).FirstOrDefault(id => id != SelectedStage.Id) ?? String.Empty;

		SelectedStage.Choices.Add(new QuestChoice { Label = "Continue", Next = next });
	}

	public void RemoveChoice(QuestChoice? choice)
	{
		if (SelectedStage is not null && choice is not null)
			SelectedStage.Choices.Remove(choice);
	}

	public void Save()
	{
		var problems = QuestCodeGenerator.Validate(_quest);

		if (problems.Count > 0)
		{
			Status = "Fix the problems listed above before saving.";
			return;
		}

		var folder = QuestFolder;

		if (folder is null)
		{
			Status = "Open a segment first. The quest is saved into its Source\\Quests folder.";
			return;
		}

		var path = Path.Combine(folder, $"{_quest.Name}.cs");

		if (File.Exists(path) && !String.Equals(path, _loadedPath, StringComparison.OrdinalIgnoreCase)
		    && QuestDefinition.FromSource(File.ReadAllText(path)) is null)
		{
			Status = $"{path} already exists and wasn't made by the Quest Maker. Rename the quest.";
			return;
		}

		if (File.Exists(path) && !String.Equals(path, _loadedPath, StringComparison.OrdinalIgnoreCase)
		    && MessageBox.Show($"Replace the quest in {path}?", "Save quest", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
			return;

		Directory.CreateDirectory(folder);
		File.WriteAllText(path, QuestCodeGenerator.Generate(_quest, SegmentName));

		/* the quest was renamed: offer to remove the file under the old name */
		if (_loadedPath is not null && !String.Equals(path, _loadedPath, StringComparison.OrdinalIgnoreCase) && File.Exists(_loadedPath)
		    && MessageBox.Show($"The quest was renamed. Delete the old file {_loadedPath}?", "Save quest", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			File.Delete(_loadedPath);

		_loadedPath = path;
		RefreshFiles();

		Status = $"Saved {path}." + (_quest.Stages.Any(s => s.Kind == QuestStageKind.Kill) ? " Use \"Add kill hooks\" so kills count." : String.Empty);
	}

	public void CopyCode()
	{
		Clipboard.SetText(QuestCodeGenerator.Generate(_quest, SegmentName));
		Status = "Copied the quest's C# code.";
	}

	/// <summary>Adds the line that counts kills to the OnDeath script of every kill stage's entity.</summary>
	public void AddKillHooks()
	{
		if (_segment is null)
		{
			Status = "Open a segment first.";
			return;
		}

		var added = new List<string>();
		var present = new List<string>();
		var missing = new List<string>();

		foreach (var target in _quest.Stages.Where(s => s.Kind == QuestStageKind.Kill).Select(s => s.Target).Distinct())
		{
			var entity = _segment.Entities.FirstOrDefault(e => e.Name == target);
			var script = entity?.Scripts.FirstOrDefault(s => s.Name == "OnDeath");

			if (entity is null || script is null)
			{
				missing.Add(target);
				continue;
			}

			var hook = QuestCodeGenerator.KillHook(_quest, target);

			if (script.Body.Contains(hook))
			{
				present.Add(target);
				continue;
			}

			script.Body = String.IsNullOrWhiteSpace(script.Body) ? $"\t{hook}" : $"{script.Body.TrimEnd()}{Environment.NewLine}\t{hook}";
			script.IsEnabled = true;

			WeakReferenceMessenger.Default.Send(new SegmentEntityChanged(entity));
			added.Add(target);
		}

		var parts = new List<string>();

		if (added.Count > 0)
			parts.Add($"Added the kill hook to OnDeath of {String.Join(", ", added)}. Save the segment to keep it.");
		if (present.Count > 0)
			parts.Add($"Already hooked: {String.Join(", ", present)}.");
		if (missing.Count > 0)
			parts.Add($"No entity named {String.Join(", ", missing)}.");
		if (parts.Count == 0)
			parts.Add("This quest has no kill stages.");

		Status = String.Join(" ", parts);
	}

	/// <summary>Adds an entity whose OnSpawn creates the quest giver, so it can be placed with a spawner.</summary>
	public void CreateGiverEntity()
	{
		if (_segment is null)
		{
			Status = "Open a segment first.";
			return;
		}

		var name = $"{_quest.Name}Giver";
		var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
		var entity = _segment.Entities.FirstOrDefault(e => e.Name == name);

		if (entity is null)
		{
			entity = new SegmentEntity { Name = name, Notes = $"Quest giver for {_quest.Title}, made by the Quest Maker." };

			var onSpawn = entity.Scripts.FirstOrDefault(s => s.Name == "OnSpawn");

			if (onSpawn is not null)
			{
				onSpawn.Body = $"\treturn new {_quest.Name}QuestGiver();";
				onSpawn.IsEnabled = true;
			}

			_segment.Entities.Add(entity);
			Status = $"Added the entity {name}. Place it with a spawner, and save the quest so {_quest.Name}QuestGiver exists.";
		}
		else
		{
			Status = $"The entity {name} already exists.";
		}

		entity.Present(presenter);
	}

	private void Attach(QuestDefinition quest)
	{
		quest.PropertyChanged += OnChanged;
		quest.Stages.CollectionChanged += OnStagesChanged;

		foreach (var stage in quest.Stages)
			AttachStage(stage);
	}

	private void Detach(QuestDefinition quest)
	{
		quest.PropertyChanged -= OnChanged;
		quest.Stages.CollectionChanged -= OnStagesChanged;

		foreach (var stage in quest.Stages)
			DetachStage(stage);
	}

	private void AttachStage(QuestStage stage)
	{
		stage.PropertyChanging += OnStageChanging;
		stage.PropertyChanged += OnStageChanged;
		stage.Choices.CollectionChanged += OnChoicesChanged;

		foreach (var choice in stage.Choices)
			choice.PropertyChanged += OnChanged;
	}

	private void DetachStage(QuestStage stage)
	{
		stage.PropertyChanging -= OnStageChanging;
		stage.PropertyChanged -= OnStageChanged;
		stage.Choices.CollectionChanged -= OnChoicesChanged;

		foreach (var choice in stage.Choices)
			choice.PropertyChanged -= OnChanged;
	}

	private void OnStagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems is not null)
			foreach (QuestStage stage in e.OldItems)
				DetachStage(stage);

		if (e.NewItems is not null)
			foreach (QuestStage stage in e.NewItems)
				AttachStage(stage);

		ScheduleRefresh();
	}

	private void OnChoicesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems is not null)
			foreach (QuestChoice choice in e.OldItems)
				choice.PropertyChanged -= OnChanged;

		if (e.NewItems is not null)
			foreach (QuestChoice choice in e.NewItems)
				choice.PropertyChanged += OnChanged;

		ScheduleRefresh();
	}

	private void OnStageChanging(object? sender, PropertyChangingEventArgs e)
	{
		if (sender is QuestStage stage && e.PropertyName == nameof(QuestStage.Id))
			_idsBeforeChange[stage] = stage.Id;
	}

	/// <summary>Renaming a stage also renames it in every choice that leads to it, and in the first stage.</summary>
	private void OnStageChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (sender is QuestStage stage && e.PropertyName == nameof(QuestStage.Id) && _idsBeforeChange.Remove(stage, out var oldId)
		    && !String.IsNullOrEmpty(oldId) && _quest.Stages.Count(s => s.Id == oldId) == 0)
		{
			foreach (var choice in _quest.Stages.SelectMany(s => s.Choices).Where(c => c.Next == oldId))
				choice.Next = stage.Id;

			if (_quest.StartStage == oldId)
				_quest.StartStage = stage.Id;
		}

		ScheduleRefresh();
	}

	private void OnChanged(object? sender, PropertyChangedEventArgs e) => ScheduleRefresh();

	private void ScheduleRefresh()
	{
		_refresh.Stop();
		_refresh.Start();
	}

	private void Refresh()
	{
		var ids = _quest.Stages.Select(s => s.Id).ToList();

		/* update in place so editable combo boxes keep their text */
		for (var i = StageIds.Count - 1; i >= 0; i--)
		{
			if (!ids.Contains(StageIds[i]))
				StageIds.RemoveAt(i);
		}

		for (var i = 0; i < ids.Count; i++)
		{
			if (!StageIds.Contains(ids[i]))
				StageIds.Insert(Math.Min(i, StageIds.Count), ids[i]);
		}

		var problems = QuestCodeGenerator.Validate(_quest);
		var unreachable = QuestCodeGenerator.Unreachable(_quest).Where(ids.Contains).ToList();

		if (unreachable.Count > 0)
			problems.Add($"No path reaches {String.Join(", ", unreachable)}. (Not an error, just a warning.)");

		Problems = problems.Count == 0 ? "Ready to save." : String.Join(Environment.NewLine, problems.Select(p => "• " + p));
		Preview = QuestCodeGenerator.Generate(_quest, SegmentName);

		Refreshed?.Invoke();
	}
}
