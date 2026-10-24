#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Editor.Assets;
using Kesmai.WorldForge.Editor.Items;

namespace Kesmai.WorldForge.UI.Documents;

public partial class ItemMakerDocument : UserControl
{
	public ItemMakerDocument()
	{
		InitializeComponent();
	}

	private ItemMakerViewModel? ViewModel => DataContext as ItemMakerViewModel;

	private void OnNew(object sender, RoutedEventArgs e) => ViewModel?.New();
	private void OnDelete(object sender, RoutedEventArgs e) => ViewModel?.Delete();
	private void OnRefreshFiles(object sender, RoutedEventArgs e) => ViewModel?.RefreshFiles();
	private void OnSave(object sender, RoutedEventArgs e) => ViewModel?.Save();
	private void OnCopyCode(object sender, RoutedEventArgs e) => ViewModel?.CopyCode();

	private void OnArtChosen(object sender, SelectionChangedEventArgs e)
	{
		if (ViewModel is not null && _art.SelectedItem is BodyAsset art)
			ViewModel.Item.ItemId = art.Id;
	}
}

/// <summary>A storage category in the category dropdown.</summary>
public record CategoryChoice(int Number, string Label);

/// <summary>An item file found in the segment's Source folder.</summary>
public record ItemFile(string Name, string Path)
{
	public override string ToString() => Name;
}

/// <summary>The Item Maker or Weapon Maker for one segment: one is open per kind.</summary>
public class ItemMakerViewModel : ObservableObject
{
	private Segment? _segment;
	private ItemDefinition _item;
	private ItemFile? _selectedFile;
	private string? _loadedPath;
	private string _preview = String.Empty;
	private string _problems = String.Empty;
	private string _status = String.Empty;
	private string _nameInfo = String.Empty;
	private string _artInfo = String.Empty;
	private string _artSearch = String.Empty;
	private ImageSource? _artPreview;
	private bool _loadingFile;
	private AssetCatalog? _catalog;

	private readonly DispatcherTimer _refresh;

	public ItemKind Kind { get; }

	public bool IsWeapon => Kind == ItemKind.Weapon;

	public string Name => IsWeapon ? "(Weapon Maker)" : "(Item Maker)";

	public ObservableCollection<ItemFile> ItemFiles { get; } = new();
	public ObservableCollection<BodyAsset> Art { get; } = new();

	public IReadOnlyList<string> WeaponTypes => ItemDefinition.WeaponTypes;
	public IReadOnlyList<CategoryChoice> Categories { get; } = ItemDefinition.Categories.Select(c => new CategoryChoice(c.Number, $"{c.Number}  {c.Name}")).ToList();

	private IReadOnlyList<string> _skills = ["Sword", "Mace", "Bow", "Dagger", "Staff", "Flail", "Rapier", "Throwing", "Halberd", "Threestaff", "Greatsword"];
	private IReadOnlyList<string> _penetrations = ["None", "VeryLight", "Light", "Medium", "Heavy", "VeryHeavy", "Full"];
	private IReadOnlyList<string> _professions = ["Fighter", "Thief", "Wizard", "Thaumaturge", "MartialArtist", "Knight"];
	private IReadOnlyList<string> _colors = [];

	public IReadOnlyList<string> Skills { get => _skills; private set => SetProperty(ref _skills, value); }
	public IReadOnlyList<string> Penetrations { get => _penetrations; private set => SetProperty(ref _penetrations, value); }
	public IReadOnlyList<string> Professions { get => _professions; private set => SetProperty(ref _professions, value); }
	public IReadOnlyList<string> Colors { get => _colors; private set => SetProperty(ref _colors, value); }

	public ItemMakerViewModel(ItemKind kind)
	{
		Kind = kind;
		_item = ItemDefinition.Create(kind);
		_item.PropertyChanged += OnItemChanged;

		_refresh = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, (_, _) =>
		{
			_refresh!.Stop();
			Refresh();
		}, Dispatcher.CurrentDispatcher);

		_ = LoadCatalogAsync();

		Refresh();
	}

	private async System.Threading.Tasks.Task LoadCatalogAsync()
	{
		_catalog = await AssetCatalog.LoadAsync();

		if (_catalog.Skills.Count > 0)
			Skills = _catalog.Skills;

		if (_catalog.ShieldPenetration.Count > 1)
			Penetrations = _catalog.ShieldPenetration;

		Professions = _catalog.Professions;
		Colors = _catalog.NamedColors;

		_item.SetFlagChoices(_catalog.WeaponFlags);
		FilterArt();
		Refresh();
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

	public ItemDefinition Item
	{
		get => _item;
		private set
		{
			_item.PropertyChanged -= OnItemChanged;

			if (SetProperty(ref _item, value))
			{
				_item.PropertyChanged += OnItemChanged;

				if (_catalog is not null)
					_item.SetFlagChoices(_catalog.WeaponFlags);

				Refresh();
			}
		}
	}

	private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
	{
		_refresh.Stop();
		_refresh.Start();
	}

	public ItemFile? SelectedFile
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
	public string NameInfo { get => _nameInfo; private set => SetProperty(ref _nameInfo, value); }
	public string ArtInfo { get => _artInfo; private set => SetProperty(ref _artInfo, value); }
	public ImageSource? ArtPreview { get => _artPreview; private set => SetProperty(ref _artPreview, value); }

	public string ArtSearch
	{
		get => _artSearch;
		set
		{
			if (SetProperty(ref _artSearch, value))
				FilterArt();
		}
	}

	private void FilterArt()
	{
		Art.Clear();

		var query = _artSearch.Trim();

		foreach (var art in _catalog?.ItemArt ?? [])
		{
			if (query.Length == 0 || art.Id.ToString().StartsWith(query) || art.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
			    || art.Texture.Contains(query, StringComparison.OrdinalIgnoreCase))
				Art.Add(art);
		}
	}

	private string SegmentName => String.IsNullOrWhiteSpace(_segment?.Name) ? "Segment" : _segment!.Name.Replace(" ", String.Empty);

	private string? ItemFolder => String.IsNullOrWhiteSpace(_segment?.Directory) ? null : Path.Combine(_segment!.Directory, "Source", "Items");

	private ItemNames? Names => _catalog is { ContentPath.Length: > 0 } ? new ItemNames(_catalog.ContentPath) : null;

	private void Refresh()
	{
		Preview = ItemCodeGenerator.Generate(_item, SegmentName);

		var problems = ItemCodeGenerator.Validate(_item);
		Problems = problems.Count == 0 ? "Ready to save." : String.Join(Environment.NewLine, problems.Select(p => "• " + p));

		var names = Names;

		if (names is null || !names.Exists)
			NameInfo = "The repository's Content\\Localization\\Enu.xml wasn't found, so the name can't be saved for players to see.";
		else if (_item.DisplayName.Trim().Length == 0)
			NameInfo = String.Empty;
		else if (names.Find(_item.DisplayName) is int existing)
			NameInfo = $"Uses the existing name entry {existing}.";
		else
			NameInfo = $"Saving adds \"{_item.DisplayName.Trim()}\" as name entry {names.Next()} in Content\\Localization\\Enu.xml.";

		var art = _catalog?.ItemArt.FirstOrDefault(a => a.Id == _item.ItemId);
		var hue = ItemCodeGenerator.HueExpression(_item.Hue) is null ? (Color?)null : ParseHue(_item.Hue);

		ArtPreview = art is null ? null : hue is Color color ? art.Tint(color) : art.Image;
		ArtInfo = art is null
			? $"Art {_item.ItemId} isn't one of the repository's custom items, so there's no preview. Game art ids still work."
			: $"Art {art.Id}{(String.IsNullOrEmpty(art.Name) ? String.Empty : " · " + art.Name)} · {art.Texture}";
	}

	private static Color? ParseHue(string hue)
	{
		var parts = hue.Split(',').Select(p => p.Trim()).ToArray();

		if (parts.Length is 3 or 4 && parts.All(p => Byte.TryParse(p, out _)))
		{
			var bytes = parts.Select(Byte.Parse).ToArray();
			return bytes.Length == 4 ? Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]) : Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
		}

		var named = System.Drawing.Color.FromName(hue.Trim());
		return named.IsKnownColor ? Color.FromArgb(named.A, named.R, named.G, named.B) : null;
	}

	public void RefreshFiles()
	{
		_loadingFile = true;

		ItemFiles.Clear();

		if (!String.IsNullOrWhiteSpace(_segment?.Directory) && Directory.Exists(Path.Combine(_segment!.Directory, "Source")))
		{
			foreach (var path in Directory.GetFiles(Path.Combine(_segment.Directory, "Source"), "*.cs", SearchOption.AllDirectories))
			{
				if (path.Contains(@"\bin\") || path.Contains(@"\obj\"))
					continue;

				var head = File.ReadLines(path).Take(10).ToList();

				if (!head.Any(line => line.TrimStart().StartsWith(ItemDefinition.Marker.Trim())))
					continue;

				if (ItemDefinition.FromSource(String.Join("\n", head)) is { } item && item.Kind == Kind)
					ItemFiles.Add(new ItemFile(Path.GetFileNameWithoutExtension(path), path));
			}
		}

		_selectedFile = ItemFiles.FirstOrDefault(f => String.Equals(f.Path, _loadedPath, StringComparison.OrdinalIgnoreCase));
		OnPropertyChanged(nameof(SelectedFile));

		_loadingFile = false;
	}

	private void Open(ItemFile file)
	{
		try
		{
			var item = ItemDefinition.FromSource(File.ReadAllText(file.Path));

			if (item is null)
			{
				Status = $"{file.Name} wasn't made here, so it can't be opened.";
				return;
			}

			_loadedPath = file.Path;
			Item = item;
			Status = $"Opened {file.Path}.";
		}
		catch (Exception e)
		{
			Status = $"Couldn't open {file.Name}: {e.Message}";
		}
	}

	public void New()
	{
		var item = ItemDefinition.Create(Kind);
		var taken = ItemFiles.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
		var name = item.ClassName;
		var number = 1;

		while (taken.Contains(item.ClassName))
			item.ClassName = $"{name}{++number}";

		_loadedPath = null;
		_selectedFile = null;
		OnPropertyChanged(nameof(SelectedFile));

		Item = item;
		Status = $"Started a new {(IsWeapon ? "weapon" : "item")}. Save it to add it to the segment.";
	}

	public void Delete()
	{
		if (_loadedPath is null || !File.Exists(_loadedPath))
		{
			Status = "This hasn't been saved, so there is no file to delete.";
			return;
		}

		if (MessageBox.Show($"Delete {_loadedPath}?", Name, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
			return;

		File.Delete(_loadedPath);
		Status = $"Deleted {_loadedPath}. Its name entry stays in Enu.xml.";
		_loadedPath = null;

		RefreshFiles();
	}

	public void Save()
	{
		var problems = ItemCodeGenerator.Validate(_item);

		if (problems.Count > 0)
		{
			Status = "Fix the problems listed above before saving.";
			return;
		}

		var folder = ItemFolder;

		if (folder is null)
		{
			Status = "Open a segment first. The item is saved into its Source\\Items folder.";
			return;
		}

		var path = Path.Combine(folder, $"{_item.ClassName}.cs");

		if (File.Exists(path) && !String.Equals(path, _loadedPath, StringComparison.OrdinalIgnoreCase)
		    && ItemDefinition.FromSource(File.ReadAllText(path)) is null)
		{
			Status = $"{path} already exists and wasn't made here. Rename the class.";
			return;
		}

		if (File.Exists(path) && !String.Equals(path, _loadedPath, StringComparison.OrdinalIgnoreCase)
		    && MessageBox.Show($"Replace {path}?", Name, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
			return;

		// the name players see is a localization entry: reuse the one with this text, or add one
		var names = Names;
		var nameNote = String.Empty;

		if (names is { Exists: true })
		{
			if (names.Find(_item.DisplayName) is int existing)
			{
				_item.LabelNumber = existing;
			}
			else
			{
				_item.LabelNumber = names.Add(_item.DisplayName);
				nameNote = $" Added \"{_item.DisplayName.Trim()}\" as name entry {_item.LabelNumber} in {names.Path}.";
			}
		}
		else
		{
			nameNote = " Enu.xml wasn't found, so the item keeps its weapon type's name.";
		}

		Directory.CreateDirectory(folder);
		File.WriteAllText(path, ItemCodeGenerator.Generate(_item, SegmentName));

		/* the class was renamed: offer to remove the file under the old name */
		if (_loadedPath is not null && !String.Equals(path, _loadedPath, StringComparison.OrdinalIgnoreCase) && File.Exists(_loadedPath)
		    && MessageBox.Show($"The class was renamed. Delete the old file {_loadedPath}?", Name, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
			File.Delete(_loadedPath);

		_loadedPath = path;
		RefreshFiles();
		Refresh();

		Status = $"Saved {path}.{nameNote} Scripts and loot can now use new {_item.ClassName}().";
	}

	public void CopyCode()
	{
		Clipboard.SetText(ItemCodeGenerator.Generate(_item, SegmentName));
		Status = "Copied the C# code.";
	}
}
