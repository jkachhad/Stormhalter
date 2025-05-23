﻿using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DigitalRune.Collections;
using DigitalRune.Graphics;
using DigitalRune.ServiceLocation;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.MVP;
using Kesmai.WorldForge.Roslyn;
using Kesmai.WorldForge.UI;
using Kesmai.WorldForge.UI.Documents;
using Kesmai.WorldForge.UI.Windows;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynPad.Roslyn;
using SharpDX.Direct3D9;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Editor;

public class GetActiveSegmentRequestMessage : RequestMessage<Segment>
{
}
	
public class UnregisterEvents
{
}
public class ApplicationPresenter : ObservableRecipient
{
	private int _unitSize = 55;
		
	private string _segmentFilePath;
	private DirectoryInfo _segmentFileFolder;
		
	private Segment _segment;
	private CustomRoslynHost _roslynHost;
	private Selection _selection;
	private TerrainSelector _filter;
	private Tool _selectedTool;

	private bool _showSubregions;

	private object _activeDocument;
	private object _previousDocument;

	private TeleportComponent _configuringTeleporter = null;
	public TeleportComponent ConfiguringTeleporter
	{
		get { return _configuringTeleporter; }
		set { _configuringTeleporter = value; }
	}

	public TerrainSelector SelectedFilter
	{
		get => _filter ?? TerrainSelector.Default;
		set => _filter = value;
	}
		
	public Selection Selection
	{
		get => _selection;
		set => _selection = value;
	}
		
	public Tool SelectedTool
	{
		get => _selectedTool ?? Tool.Default;
		set => _selectedTool = value;
	}

	public IRoslynHost RoslynHost => _roslynHost;

	private ComponentsCategory _selectedComponentCategory;
	private TerrainComponent _selectedComponent;
		
	public ComponentsCategory SelectedComponentCategory
	{
		get => _selectedComponentCategory;
		set => SetProperty(ref _selectedComponentCategory, value);
	}
		
	public TerrainComponent SelectedComponent
	{
		get => _selectedComponent;
		set => SetProperty(ref _selectedComponent, value);
	}
		
	public bool ShowSubregions
	{
		get => _showSubregions;
		set
		{
			SetProperty(ref _showSubregions, value);
			InvalidateRender();
		}
	}

	public int UnitSize => _unitSize;

	public Segment Segment
	{
		get => _segment;
		set
		{
			if (SetProperty(ref _segment, value, true))
			{
				if (value != null)
					_roslynHost = new CustomRoslynHost(_segment);

				ActiveDocument = Documents.FirstOrDefault();
			}
		}
	}

	public NotifyingCollection<TerrainSelector> Filters { get; set; }
	public NotifyingCollection<Tool> Tools { get; set; }
	public VisibilityOptions Visibility { get; set; }

	public RelayCommand CreateSegmentCommand { get; set; }
	public RelayCommand CloseSegmentCommand { get; set; }
	public RelayCommand CompileSegmentCommand { get; set; }
	public RelayCommand OpenSegmentCommand { get; set; }
	public RelayCommand<bool> SaveSegmentCommand { get; set; }

	public RelayCommand CreateRegionCommand { get; set; }
	public RelayCommand<object> DeleteRegionCommand { get; set; }
	public RelayCommand<object> ShiftRegionCommand { get; set; }

	public RelayCommand GenerateRegionCommand { get; set; }
		
	public RelayCommand<TerrainSelector> SelectFilterCommand { get; set; }
	public RelayCommand<Tool> SelectToolCommand { get; set; }

	public RelayCommand ExitApplicationCommand { get; set; }

	public RelayCommand ShowChangesWindow { get; set; }
	public RelayCommand LaunchWiki { get; set; }

	public RelayCommand<String> SwapDocumentCommand { get; set; }
		
	public ObservableCollection<object> Documents { get; private set; }
		
	public object ActiveDocument
	{
		get => _activeDocument;
		set
        {
            if (value != _activeDocument)
            {
                if (_activeDocument is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _previousDocument = _activeDocument;
                SetProperty(ref _activeDocument, value, true);
            }
		}
	}
    public RelayCommand ExportToPdfCommand { get; set; }
    public ApplicationPresenter()
	{
		var messenger = WeakReferenceMessenger.Default;

        ExportToPdfCommand = new RelayCommand(() =>
        {
            ((Func<Task>)(() => ExportToPdfAsync())).FireAndForget();
        }, () => (ActiveDocument is SegmentRegion));

        ExportToPdfCommand.DependsOn(() => ActiveDocument);
        messenger.Register<ApplicationPresenter, GetActiveSegmentRequestMessage>(this,
			(r, m) => m.Reply(r.Segment));

		Documents = new ObservableCollection<object>();
			
		CreateSegmentCommand = new RelayCommand(CreateSegment, () => (Segment == null));
		CreateSegmentCommand.DependsOn(() => Segment);
			
		CloseSegmentCommand = new RelayCommand(CloseSegment, () => (Segment != null));
		CloseSegmentCommand.DependsOn(() => Segment);
			
		CompileSegmentCommand = new RelayCommand(CompileSegment, () => (Segment != null && !Network.Disconnected));
		CompileSegmentCommand.DependsOn(() => Segment);
			
		OpenSegmentCommand = new RelayCommand(OpenSegment, () => (Segment == null));
		OpenSegmentCommand.DependsOn(() => Segment);
			
		SaveSegmentCommand = new RelayCommand<bool>(SaveSegment, (queryPath) => (Segment != null));
		SaveSegmentCommand.DependsOn(() => Segment);
			

		CreateRegionCommand = new RelayCommand(CreateRegion, () => (Segment != null));
		CreateRegionCommand.DependsOn(() => Segment);
			
		DeleteRegionCommand = new RelayCommand<object>(DeleteRegion, 
			(o) => (ActiveDocument is SegmentRegion));
		DeleteRegionCommand.DependsOn(() => Segment, () => ActiveDocument);

		ShiftRegionCommand = new RelayCommand<object>(ShiftRegion, 
			(o) => (ActiveDocument is SegmentRegion));
		ShiftRegionCommand.DependsOn(() => Segment, () => ActiveDocument);

		GenerateRegionCommand = new RelayCommand(GenerateRegions, () => (Segment != null));
		GenerateRegionCommand.DependsOn(() => Segment);

		ShowChangesWindow = new RelayCommand(() => { new Kesmai.WorldForge.UI.Windows.WhatsNew().ShowDialog(); });
		LaunchWiki = new RelayCommand(() => {
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
			{
				FileName = "http://www.stormhalter.com/wiki/WorldForge",
				UseShellExecute = true
			}); 
		});

		SwapDocumentCommand = new RelayCommand<string>(SwapDocument);

		WeakReferenceMessenger.Default
			.Register<ApplicationPresenter, VisibilityOptionsChanged>(
				this, (r, m) => {
					if (Segment is not null)
						Segment.UpdateTiles();
					var graphicsService = ServiceLocator.Current.GetInstance<IGraphicsService>();
					if (graphicsService != null)
					{
						foreach (var presenter in graphicsService.PresentationTargets.OfType<PresentationTarget>())
							presenter.InvalidateRender();
					}
				});

		SelectFilterCommand = new RelayCommand<TerrainSelector>(SelectFilter, 
			(filter) => (Segment != null));
		SelectFilterCommand.DependsOn(() => Segment, () => ActiveDocument);
			
		SelectToolCommand = new RelayCommand<Tool>(SelectTool, 
			(tool) => (Segment != null) && (ActiveDocument is SegmentRegion || ActiveDocument is ComponentsPanel));
		SelectToolCommand.DependsOn(() => Segment, () => ActiveDocument);

		Filters = new NotifyingCollection<TerrainSelector>()
		{
			TerrainSelector.Default,
				
			new FloorSelector(),
			new StaticSelector(),
			new WaterSelector(),
			new WallSelector(),
			new StructureSelector(),
		};
			
		Tools = new NotifyingCollection<Tool>()
		{
			Tool.Default,
				
			new DrawTool(),
			new EraseTool(),
			new PaintTool(),
			new HammerTool(),
		};

		Visibility = new VisibilityOptions();
			
		ExitApplicationCommand = new RelayCommand(() => Application.Current.Shutdown());
			
		ShowSubregions = true;

		Selection = new Selection();
	}

	public void SwapDocument(String Target)
	{
		switch (Target)
		{
			case "Spawn": //Ctrl-P
			{
				var sel = _selection.FirstOrDefault();
				LocationSpawner targetLS = null;
				RegionSpawner targetRS = null;
				if (ActiveDocument is EntitiesViewModel)
				{
					var spawnRequest = WeakReferenceMessenger.Default.Send<EntitiesDocument.GetSelectedSpawner>();
					if (spawnRequest.HasReceivedResponse)
					{
						Spawner target = spawnRequest.Response;
						ActiveDocument = Documents.Where(d => d is SpawnsViewModel).FirstOrDefault() as SpawnsViewModel;
						if (target is LocationSpawner)
							(ActiveDocument as SpawnsViewModel).SelectedLocationSpawner = target as LocationSpawner;
						if (target is RegionSpawner)
							(ActiveDocument as SpawnsViewModel).SelectedRegionSpawner = target as RegionSpawner;
						WeakReferenceMessenger.Default.Send(target as Spawner);
						break;
					}
				}
				if (sel is { Width: 1, Height: 1 })
				{
					targetLS = Segment.Spawns.Location.Where(s => s.Region == _selection.Region.ID && s.X == sel.Left && s.Y == sel.Top).LastOrDefault();
					targetRS = Segment.Spawns.Region.Where(s => s.Region == _selection.Region.ID && s.Inclusions.Any(i => i.ToRectangle().Contains(sel.Left, sel.Top))).LastOrDefault();
				}
				ActiveDocument = Documents.Where(d => d is SpawnsViewModel).FirstOrDefault() as SpawnsViewModel;
				if (targetRS is not null && targetLS is null)
				{
					(ActiveDocument as SpawnsViewModel).SelectedRegionSpawner = targetRS;
					WeakReferenceMessenger.Default.Send(targetRS as Spawner);
				}
				if (targetLS is not null) { 
					(ActiveDocument as SpawnsViewModel).SelectedLocationSpawner = targetLS;
					WeakReferenceMessenger.Default.Send(targetLS as Spawner);
				}
				break;
			}
			case "Segment": //Ctrl-G
				ActiveDocument = Documents.Where(d => d is SegmentViewModel).FirstOrDefault() as SegmentViewModel;
				break;
			case "Entity": //Ctrl-E
			{
				Entity target = null;
				//There's probably a better way to do this. but I can't search one up. Can I send a more generic message and have both documents register for it but decline to respond if they are not active?
				var spawnEntityRequest = WeakReferenceMessenger.Default.Send<SpawnsDocument.GetActiveEntity>();
				var treasureEntityRequest = WeakReferenceMessenger.Default.Send<TreasuresDocument.GetActiveEntity>();
				if (spawnEntityRequest.HasReceivedResponse && spawnEntityRequest.Response != null) 
					target = spawnEntityRequest.Response;
				if (treasureEntityRequest.HasReceivedResponse && treasureEntityRequest.Response != null)
					target = treasureEntityRequest.Response;
				ActiveDocument = Documents.Where(d => d is EntitiesViewModel).FirstOrDefault() as EntitiesViewModel;
				if (target is not null)
					(ActiveDocument as EntitiesViewModel).SelectedEntity = target;
				break;
			}
			case "Treasure": //Ctrl-T
				String targetTreasure = null;
				if (ActiveDocument is EntitiesViewModel e)
				{
					var treasureRequest = WeakReferenceMessenger.Default.Send<EntitiesDocument.GetCurrentScriptSelection>();
					if (treasureRequest.HasReceivedResponse)
						targetTreasure = treasureRequest.Response;
				}
				ActiveDocument = Documents.Where(d => d is TreasuresViewModel).FirstOrDefault() as TreasuresViewModel;
				if (targetTreasure is not null)
				{
					var targetTreasureObject = (ActiveDocument as TreasuresViewModel).Treasures.Where(t => t.Name == targetTreasure).FirstOrDefault();
					if (targetTreasureObject is not null)
						(ActiveDocument as TreasuresViewModel).SelectedTreasure = targetTreasureObject;
				}
				break;
			case "Location": //Ctrl-L
			{
				var sel = _selection.FirstOrDefault();
				SegmentLocation target = null;
				if (sel is { Width: 1, Height: 1 })
				{
					target = Segment.Locations.Where(l => l.Region == _selection.Region.ID && l.X == sel.Left && l.Y == sel.Top).LastOrDefault();
				}
				ActiveDocument = Documents.Where(d => d is LocationsViewModel).FirstOrDefault() as LocationsViewModel;
				if (target is not null)
					(ActiveDocument as LocationsViewModel).SelectedLocation = target;
				break;
			}
			case "Subregion": //Ctrl-U
			{
				var sel = _selection.FirstOrDefault();
				SegmentSubregion target = null;
				if (sel is { Width: 1, Height: 1 })
				{
					target = Segment.Subregions.Where(s => s.Region == _selection.Region.ID && s.Rectangles.Any(rect => rect.ToRectangle().Contains(sel.Left, sel.Top))).LastOrDefault();
				}
				ActiveDocument = Documents.Where(d => d is SubregionViewModel).FirstOrDefault() as SubregionViewModel;
				if (target is not null)
					(ActiveDocument as SubregionViewModel).SelectedSubregion = target;
				break;
			}
			case "Teleporter": //Ctrl-D
			{
				var sel = _selection.FirstOrDefault();
				TeleportComponent target = null;
				if (sel is { Width:1, Height: 1 })
				{
					var tile = _selection.Region.GetTile(sel.Left, sel.Top);
					target = tile.GetComponents<TeleportComponent>().FirstOrDefault();
				}
				if (target is not null)
				{
					ActiveDocument = Documents.Where(d => d is SegmentRegion dr && dr.ID == target.DestinationRegion).FirstOrDefault();
					WeakReferenceMessenger.Default.Send(new JumpSegmentRegionLocation(target.DestinationRegion, target.DestinationX, target.DestinationY));
					_selection.Select(new Microsoft.Xna.Framework.Rectangle(target.DestinationX,target.DestinationY,1,1), Segment.GetRegion(target.DestinationRegion));
				}
				break;
			}
			case "RegionLeft": //Ctrl-Left
			{
				if (ActiveDocument is SegmentRegion)
				{
					var sortedRegions = Segment.Regions.OrderBy(r => r.ID);
					int regionID = (ActiveDocument as SegmentRegion).ID;
					SegmentRegion nextRegion = sortedRegions.LastOrDefault(r => r.ID < regionID);
					if (nextRegion != null) { ActiveDocument = nextRegion; } else { ActiveDocument = sortedRegions.Last(); }
				}
				break;
			}
			case "RegionRight": //Ctrl-Right
			{
				if (ActiveDocument is SegmentRegion)
				{
					var sortedRegions = Segment.Regions.OrderBy(r => r.ID);
					int regionID = (ActiveDocument as SegmentRegion).ID;
					SegmentRegion nextRegion = sortedRegions.FirstOrDefault(r => r.ID > regionID);
					if (nextRegion != null) { ActiveDocument = nextRegion; } else { ActiveDocument = sortedRegions.First(); }
				}
				break;
			}
		}
	}
	public void SelectFilter(TerrainSelector nextFilter)
	{
		foreach (var filter in Filters)
			filter.IsActive = false;

		SelectedFilter = nextFilter;
			
		if (nextFilter != null)
		{
			nextFilter.IsActive = true;

			if (nextFilter.IsActive)
				_segment.UpdateTiles();
				
			var services = (ServiceContainer)ServiceLocator.Current;
			var graphicsService = services.GetInstance<IGraphicsService>();

			foreach (var presenter in graphicsService.PresentationTargets.OfType<PresentationTarget>())
				presenter.InvalidateRender();
		}
	}
		
	public void SelectTool(Tool nextTool)
	{
		if (nextTool == default(Tool))
			nextTool = Tool.Default;
			
		foreach (var tool in Tools)
		{
			tool.OnDeactivate();
			tool.IsActive = false;
		}

		SelectedTool = nextTool;
			
		if (nextTool != null)
		{
			nextTool.IsActive = true;
			nextTool.OnActivate();

			var worldScreen = ServiceLocator.Current.GetInstance<WorldGraphicsScreen>();

			if (worldScreen != null)
				worldScreen.InvalidateRender();
		}
	}
		
	private void JumpSpawner (Spawner spawner)
	{
		var viewmodel = ActiveDocument as SpawnsViewModel;

		if (spawner is LocationSpawner)
		{
			viewmodel.SelectedLocationSpawner = spawner as LocationSpawner;
		}

		if (spawner is RegionSpawner)
		{
			viewmodel.SelectedRegionSpawner = spawner as RegionSpawner;
		}
	}

	private void JumpSubregion(SegmentSubregion subregion)
	{
		var viewmodel = ActiveDocument as SubregionViewModel;
		viewmodel.SelectedSubregion = subregion;
	}

	private void JumpEntity(Entity entity)
	{
		var viewmodel = ActiveDocument as EntitiesViewModel;
		viewmodel.SelectedEntity = entity;

	}

	private void JumpLocation (SegmentLocation location)
	{
		var viewmodel = ActiveDocument as LocationsViewModel;
		viewmodel.SelectedLocation = location;
	}

	private void JumpTreasure (SegmentTreasure treasure)
	{
		var viewmodel = ActiveDocument as TreasuresViewModel;
		viewmodel.SelectedTreasure = treasure;
	}

	public void JumpPrevious ()
	{
		if (_previousDocument != _activeDocument && _previousDocument != null)
		{
			ActiveDocument = _previousDocument;
		}
	}

	private void CreateSegment()
	{
		if (_segment != null)
			throw new InvalidOperationException("Attempt to create a segment when an active segment already exists.");

		Segment = new Segment();
			
		Documents.Add(new SegmentViewModel(Segment));
		Documents.Add(new LocationsViewModel(Segment));
		Documents.Add(new SubregionViewModel(Segment));
		Documents.Add(new EntitiesViewModel(Segment));
		Documents.Add(new SpawnsViewModel(Segment));
		Documents.Add(new TreasuresViewModel(Segment));

		_segmentFilePath = String.Empty;
	}

	private void CloseSegment()
	{
		if (_segment == null)
			throw new InvalidOperationException("Attempt to close a segment when an active segment does not exist.");

		Segment = null;
		WeakReferenceMessenger.Default.Send(new UnregisterEvents());
		Documents.Clear();
			
		_segmentFilePath = String.Empty;
	}

	private void CompileSegment()
	{
		if (_segment is null || Network.Disconnected)
			return;

		new CompileWindow().ShowDialog();
	}

	private void OpenSegment()
	{
		var overwrite = true;

		if (Segment != null)
		{
			var overwriteResult = MessageBox.Show("You may lose changes to the existing segment project, continue?", "Open Segment", MessageBoxButton.YesNo);

			if (overwriteResult != MessageBoxResult.Yes)
				overwrite = false;
		}

		if (!overwrite)
			return;

		// TODO: Set open file path for Save
		var dialog = new Microsoft.Win32.OpenFileDialog()
		{
			DefaultExt = ".mapproj",
			Filter = "WorldForge - Map Project (*.mapproj)|*.mapproj",
		};

		var openResult = dialog.ShowDialog();

		if (!openResult.HasValue || openResult != true)
			return;

		var targetFile = dialog.FileName;
		var targetFileInfo = new FileInfo(targetFile);
		
		_segmentFilePath = targetFile;
		_segmentFileFolder = targetFileInfo.Directory;
			
		var segment = new Segment();
		
		XElement rootElement = null;
		try
		{
			var document = XDocument.Load(targetFile);
			rootElement = document.Root;
		} catch (System.Xml.XmlException e)
		{
			MessageBox.Show($"Segment File is incorrectly formatted:\n{e.Message}", "Open Segment Error", MessageBoxButton.OK);
			return;
		}
		if (rootElement != null)
		{
			if (rootElement.Name != "segment")
			{
				MessageBox.Show($"Provided file is not a WorldForge Segment file.", "Open Segment Error", MessageBoxButton.OK);
				return;
			}
			segment.Load(rootElement);
		}
			
		var definitionFilePath = $@"{_segmentFileFolder.FullName}\{segment.Name}.cs";

		if (File.Exists(definitionFilePath))
		{
			using (var stream = new FileStream(definitionFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
			using (var reader = new StreamReader(stream))
				segment.Definition.Blocks[1] = reader.ReadToEnd();
		}

		Documents.Add(new SegmentViewModel(segment));
		Documents.Add(new LocationsViewModel(segment));
		Documents.Add(new SubregionViewModel(segment));
		Documents.Add(new EntitiesViewModel(segment));
		Documents.Add(new SpawnsViewModel(segment));
		Documents.Add(new TreasuresViewModel(segment));
		
		Segment = segment;
		Segment.UpdateTiles();
		
		SelectFilter(Filters.FirstOrDefault());
		SelectTool(Tools.FirstOrDefault());
	}

	private void SaveSegment(bool queryPath)
	{
		var targetFile = String.Empty;

		if (!CheckScriptSyntax())
			return;

		if (!queryPath && String.IsNullOrEmpty(_segmentFilePath))
			queryPath = true;

		if (queryPath)
		{
			var dialog = new Microsoft.Win32.SaveFileDialog()
			{
				DefaultExt = ".mapproj",
				Filter = "WorldForge - Map Project (*.mapproj)|*.mapproj",
			};

			var saveResult = dialog.ShowDialog();

			if (!saveResult.HasValue || saveResult != true)
				return;

			targetFile = dialog.FileName;
		}
		else
		{
			targetFile = _segmentFilePath;
		}

		try
		{
			if (File.Exists(targetFile))
				File.Delete(targetFile);

			_segmentFilePath = targetFile;

			var projectFile = new XDocument();
			var segmentElement = new XElement("segment",
				new XAttribute("name", _segment.Name ?? "(Unknown)"),
				new XAttribute("version", Core.Version));

			_segment.Save(segmentElement);

			projectFile.Add(segmentElement);

			projectFile.Save(targetFile);

			var definitionFilePath = $@"{_segmentFileFolder.FullName}\{_segment.Name}.cs";

			if (File.Exists(definitionFilePath))
				File.Delete(definitionFilePath);
			
			using (var stream = new FileStream(definitionFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
			using (var writer = new StreamWriter(stream))
				writer.Write(_segment.Definition.Blocks[1]);
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error when saving project: {ex.Message}", "Unable to save", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private bool CheckScriptSyntax() //Enumerate all script segments and verify that they pass syntax checks
	{
		//Segment code:
		var syntaxErrors = CSharpSyntaxTree.ParseText(Segment.Internal.Blocks[1], new CSharpParseOptions(kind: SourceCodeKind.Script)).GetDiagnostics();
		var syntaxErrorRefined = syntaxErrors.Where(e => e.Severity == DiagnosticSeverity.Error).ToList();
		if (syntaxErrorRefined.Count()>0)
		{
			var errorList = String.Join('\n', syntaxErrorRefined.Take(6).Select(err => (err.Location.GetLineSpan().StartLinePosition.Line+2) + ":" + err.GetMessage()));
			if (syntaxErrorRefined.Count() > 3)
				errorList += "\n...";
			var messageResult = MessageBox.Show($"Segment code has syntax errors.\nDo you wish to continue?\n\n{errorList}", "Syntax Errors in scripts", MessageBoxButton.YesNo);
			if (messageResult == MessageBoxResult.No)
			{
				ActiveDocument = Documents.Where(d => d is SegmentViewModel).FirstOrDefault() as SegmentViewModel;
				return false;
			}
		}

		//Entity scripts:
		foreach (Entity entity in Segment.Entities)
		{
			foreach (Scripting.Script script in entity.Scripts.Where(s=>s.IsEnabled))
			{
				syntaxErrors = CSharpSyntaxTree.ParseText("void OnSpawn(){" + script.Blocks[1] + "}").GetDiagnostics();
				if (syntaxErrors.Count() > 0)
				{
					var errorList = String.Join('\n', syntaxErrors.Take(3).Select(err => (err.Location.GetLineSpan().StartLinePosition.Line + 3) + ":" + err.GetMessage()));
					if (syntaxErrors.Count() > 3)
						errorList += "\n...";
					var messageResult = MessageBox.Show($"Entity '{entity.Name}' has syntax errors in script '{script.Name}'.\nDo you wish to continue?\n\n{errorList}", "Syntax Errors in scripts", MessageBoxButton.YesNo);
					if (messageResult == MessageBoxResult.No)
					{
						ActiveDocument = Documents.Where(d => d is EntitiesViewModel).FirstOrDefault() as EntitiesViewModel;
						(ActiveDocument as EntitiesViewModel).SelectedEntity = entity;
						return false;
					}
				}
			}
		}
		//Entity duplicate name check
		var entityDuplicates = Segment.Entities.GroupBy(e => e.Name, StringComparer.InvariantCultureIgnoreCase).Where(g => g.Count() > 1);
		foreach (var duplicate in entityDuplicates)
		{
			var messageResult = MessageBox.Show($"Entity '{duplicate.Key}' has {duplicate.Count()} definitions.\nDo you wish to continue?", "Duplicate Entities found", MessageBoxButton.YesNo);
			if (messageResult == MessageBoxResult.No)
			{
				ActiveDocument = Documents.Where(d => d is EntitiesViewModel).FirstOrDefault() as EntitiesViewModel;
				(ActiveDocument as EntitiesViewModel).SelectedEntity = duplicate.First();
				return false;
			}
		}

		//Spawner scripts:
		foreach (LocationSpawner spawner in Segment.Spawns.Location)
		{
			foreach (Scripting.Script script in spawner.Scripts.Where(script=>script.IsEnabled))
			{
				syntaxErrors = CSharpSyntaxTree.ParseText("void OnSpawn(){" + script.Blocks[1] + "}").GetDiagnostics();
				if (syntaxErrors.Count() > 0)
				{
					var errorList = String.Join('\n', syntaxErrors.Take(3).Select(err => (err.Location.GetLineSpan().StartLinePosition.Line + 3) + ":" + err.GetMessage()));
					if (syntaxErrors.Count() > 3)
						errorList += "\n...";
					var messageResult = MessageBox.Show($"Location Spawner '{spawner.Name}' has syntax errors in script '{script.Name}'.\nDo you wish to continue?\n\n{errorList}", "Syntax Errors in scripts", MessageBoxButton.YesNo);
					if (messageResult == MessageBoxResult.No)
					{
							
						ActiveDocument = Documents.Where(d => d is SpawnsViewModel).FirstOrDefault() as SpawnsViewModel;
						(ActiveDocument as SpawnsViewModel).SelectedLocationSpawner = spawner;
						WeakReferenceMessenger.Default.Send(spawner as Spawner);
						return false;
					}
				}
			}
		}
		//Spawner duplicate name checks
		var locationSpawnerDuplicates = Segment.Spawns.Location.GroupBy(e => e.Name, StringComparer.InvariantCultureIgnoreCase).Where(g => g.Count() > 1);
		foreach (var duplicate in locationSpawnerDuplicates)
		{
			var messageResult = MessageBox.Show($"Spawner '{duplicate.Key}' has {duplicate.Count()} definitions.\nDo you wish to continue?", "Duplicate Spawners found", MessageBoxButton.YesNo);
			if (messageResult == MessageBoxResult.No)
			{
				ActiveDocument = Documents.Where(d => d is SpawnsViewModel).FirstOrDefault() as SpawnsViewModel;
				(ActiveDocument as SpawnsViewModel).SelectedLocationSpawner = duplicate.First();
				WeakReferenceMessenger.Default.Send(duplicate.First() as Spawner);
				return false;
			}
		}
		var regionSpawnerDuplicates = Segment.Spawns.Region.GroupBy(e => e.Name, StringComparer.InvariantCultureIgnoreCase).Where(g => g.Count() > 1);
		foreach (var duplicate in regionSpawnerDuplicates)
		{
			var messageResult = MessageBox.Show($"Spawner '{duplicate.Key}' has {duplicate.Count()} definitions.\nDo you wish to continue?", "Duplicate Spawners found", MessageBoxButton.YesNo);
			if (messageResult == MessageBoxResult.No)
			{
				ActiveDocument = Documents.Where(d => d is SpawnsViewModel).FirstOrDefault() as SpawnsViewModel;
				(ActiveDocument as SpawnsViewModel).SelectedRegionSpawner = duplicate.First();
				WeakReferenceMessenger.Default.Send(duplicate.First() as Spawner);
				return false;
			}
		}

		//Treasure scripts:
		foreach (SegmentTreasure treasurePool in Segment.Treasures)
		{
			foreach (TreasureEntry entry in treasurePool.Entries)
			{
				foreach (Scripting.Script script in entry.Scripts.Where(s=>s.IsEnabled))
				{
					syntaxErrors = CSharpSyntaxTree.ParseText("void OnCreate(){" + script.Blocks[1] + "}").GetDiagnostics();
					if (syntaxErrors.Count() > 0)
					{
						var errorList = String.Join('\n', syntaxErrors.Take(3).Select(err => (err.Location.GetLineSpan().StartLinePosition.Line + 3) + ":" + err.GetMessage()));
						if (syntaxErrors.Count() > 3)
							errorList += "\n...";
						var messageResult = MessageBox.Show($"Treasure '{treasurePool.Name}' has syntax errors in script '{treasurePool.Entries.IndexOf(entry)+1}'.\nDo you wish to continue?\n\n{errorList}", "Syntax Errors in scripts", MessageBoxButton.YesNo);
						if (messageResult == MessageBoxResult.No) {
							ActiveDocument = Documents.Where(d => d is TreasuresViewModel).FirstOrDefault() as TreasuresViewModel;
							(ActiveDocument as TreasuresViewModel).SelectedTreasure = treasurePool;
							return false;
						}
					}
				}
			}
		}
		//Treasure duplicate name check
		var treasureDuplicates = Segment.Treasures.GroupBy(e => e.Name, StringComparer.InvariantCultureIgnoreCase).Where(g => g.Count() > 1);
		foreach (var duplicate in treasureDuplicates)
		{
			var messageResult = MessageBox.Show($"Treasure '{duplicate.Key}' has {duplicate.Count()} definitions.\nDo you wish to continue?", "Duplicate Treasures found", MessageBoxButton.YesNo);
			if (messageResult == MessageBoxResult.No)
			{
				ActiveDocument = Documents.Where(d => d is TreasuresViewModel).FirstOrDefault() as TreasuresViewModel;
				(ActiveDocument as TreasuresViewModel).SelectedTreasure = duplicate.First();
				return false;
			}
		}

		//Location duplicate name check
		var locationDuplicates = Segment.Locations.GroupBy(e => e.Name, StringComparer.InvariantCultureIgnoreCase).Where(g => g.Count() > 1);
		foreach (var duplicate in locationDuplicates)
		{
			var messageResult = MessageBox.Show($"Location '{duplicate.Key}' has {duplicate.Count()} definitions.\nDo you wish to continue?", "Duplicate Locations found", MessageBoxButton.YesNo);
			if (messageResult == MessageBoxResult.No)
			{
				ActiveDocument = Documents.Where(d => d is LocationsViewModel).FirstOrDefault() as LocationsViewModel;
				(ActiveDocument as LocationsViewModel).SelectedLocation = duplicate.First();
				return false;
			}
		}

		//Subregion duplicate name check
		var subregionDuplicates = Segment.Subregions.GroupBy(e => e.Name, StringComparer.InvariantCultureIgnoreCase).Where(g => g.Count() > 1);
		foreach (var duplicate in subregionDuplicates)
		{
			var messageResult = MessageBox.Show($"Subregion '{duplicate.Key}' has {duplicate.Count()} definitions.\nDo you wish to continue?", "Duplicate Subregions found", MessageBoxButton.YesNo);
			if (messageResult == MessageBoxResult.No)
			{
				ActiveDocument = Documents.Where(d => d is SubregionViewModel).FirstOrDefault() as SubregionViewModel;
				(ActiveDocument as SubregionViewModel).SelectedSubregion = duplicate.First();
				return false;
			}
		}

		/* Prevent any region with id = 0 */
		foreach (var region in Segment.Regions)
		{
			if (region.ID is not 0)
				continue;
			
			MessageBox.Show($"Unable to save: Region '{region.Name}' has ID = 0.", "Invalid Region ID", MessageBoxButton.OK);

			ActiveDocument = region;
			return false;
		}

		return true;
	}

	private void CreateRegion()
	{
		if (_segment == null)
			throw new InvalidOperationException("Attempt to create a region when an segment does not exists.");

		var index = 0;
		var freeIndex = 0;

		while (index is 0)
		{
			freeIndex++;
			
			if (_segment.Regions.All(r => r.ID != freeIndex))
				index = freeIndex;
		}

		if (index <= 0)
			throw new ArgumentOutOfRangeException(nameof(index));

		var region = new SegmentRegion(index);

		_segment.Regions.Add(region);

		ActiveDocument = Documents.LastOrDefault();
	}
		
	private void DeleteRegion(object o)
	{
		if (o is SegmentRegion region)
		{
			var messageBoxResult = MessageBox.Show($"Are you sure you wish to delete the region '{region.Name}'?", 
				"WorldForge", MessageBoxButton.YesNo, MessageBoxImage.Question);
				
			if (messageBoxResult == MessageBoxResult.Yes)
				_segment.Regions.Remove(region);
		}

		ActiveDocument = Documents.FirstOrDefault();
	}
		
	public void ShiftRegion(object o)
	{
		// TODO
	}
		
	public void GenerateRegions()
	{
		var generator = new GenerateRegionWindow();
		var result = generator.ShowDialog();
	}
		
	public void InvalidateRender()
	{
		var graphicsScreen = ServiceLocator.Current.GetInstance<WorldGraphicsScreen>();

		if (graphicsScreen != null)
			graphicsScreen.InvalidateRender();
	}

    private async Task ExportToPdfAsync()
    {
        if (ActiveDocument is SegmentRegion region)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog()
            {
                DefaultExt = ".pdf",
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();
                var pdfExportService = new PdfExportService(terrainManager);
                await pdfExportService.ExportCurrentViewAsync(region, dialog.FileName);
            }
        }
    }
}
// Extension method to handle async void safely
public static class TaskExtensions
{
    // Original extension for Task
    public static void FireAndForget(this Task task, Action<Exception> onException = null)
    {
        _ = FireAndHandleExceptionsAsync(() => task, onException);
    }

    // Alternative extension for Func<Task>, which avoids VSTHRD003
    public static void FireAndForget(this Func<Task> taskFactory, Action<Exception> onException = null)
    {
        _ = FireAndHandleExceptionsAsync(taskFactory, onException);
    }

    private static async Task FireAndHandleExceptionsAsync(Func<Task> taskFactory, Action<Exception> onException)
    {
        if (taskFactory == null)
            throw new ArgumentNullException(nameof(taskFactory));

        try
        {
            await taskFactory().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            onException?.Invoke(ex);
        }
    }
}

