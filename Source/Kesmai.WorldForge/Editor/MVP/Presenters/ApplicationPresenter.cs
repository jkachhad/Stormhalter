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
using Kesmai.WorldForge.UI;
using Kesmai.WorldForge.UI.Documents;
using Kesmai.WorldForge.UI.Windows;
using Microsoft.CodeAnalysis;
using SharpDX.Direct3D9;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;

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
                var old = _segment;
                if (SetProperty(ref _segment, value, true))
                {
                        old?.Dispose();
                        ActiveDocument = value != null ? Documents.FirstOrDefault() : null;
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
                        segment.RootPath = $@"{_segmentFileFolder.FullName}\{segment.Name}";
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
			
			var saveFolder = new FileInfo(targetFile).Directory;

			SaveAsDirectory(@$"{saveFolder}\{_segment.Name}");
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error when saving project: {ex.Message}", "Unable to save", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
	
	public void SaveAsDirectory(string path)
	{
		if (String.IsNullOrWhiteSpace(path))
		        throw new ArgumentException(nameof(path));

		Directory.CreateDirectory(path);

		string Sanitize(string name)
		{
		        var invalid = Path.GetInvalidFileNameChars();
		        return String.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
		}

		var additionalFiles = new List<string>();

		#region Regions
		var regionDir = Path.Combine(path, "Region");
		Directory.CreateDirectory(regionDir);

		foreach (var region in _segment.Regions)
		{
			var fileName = Sanitize(region.Name) + ".xml";
			region.GetXElement().Save(Path.Combine(regionDir, fileName));
		}
		#endregion

		void WriteCategory(Action<XElement> saveAction, string elementName, string fileName)
		{
			var element = new XElement(elementName);
			saveAction(element);
			element.Save(Path.Combine(path, fileName));
			additionalFiles.Add(fileName);
		}

		WriteCategory(_segment.Locations.Save, "locations", "Locations.xml");
		WriteCategory(_segment.Subregions.Save, "subregions", "Subregions.xml");
		WriteCategory(_segment.Entities.Save, "entities", "Entities.xml");
		WriteCategory(_segment.Spawns.Save, "spawns", "Spawns.xml");
		WriteCategory(_segment.Treasures.Save, "treasures", "Treasures.xml");

		var segmentName = Sanitize(_segment.Name);
		var project = new XDocument(
			new XElement("Project",
				new XAttribute("Sdk", "Microsoft.NET.Sdk"),
				new XElement("PropertyGroup",
					new XElement("OutputType", "Library"),
					new XElement("TargetFramework", "net8.0-windows8.0"),
					new XElement("RootNamespace", segmentName),
					new XElement("AssemblyName", segmentName),
					new XElement("EnableDefaultItems", false)
				),
				new XElement("ItemGroup",
					new XElement("Compile", new XAttribute("Include", "Source/**/*.cs"))
				)
			)
		);

		if (additionalFiles.Any())
		{
			project.Root?.Add(
				new XElement("ItemGroup",
					additionalFiles.Select(f =>
						new XElement("AdditionalFiles", new XAttribute("Include", f.Replace('\\', '/'))))
				)
			);
		}
		
                project.Root?.Add(
                        new XElement("ItemGroup",
                                new XElement("PackageReference", new XAttribute("Include", "Kesmai.Server.Reference"), new XAttribute("Version", "*"))
                        )
                );

                project.Save(Path.Combine(path, $"{segmentName}.csproj"));
        }

        public Segment LoadFromDirectory(string path)
        {
                if (String.IsNullOrWhiteSpace(path))
                        throw new ArgumentException(nameof(path));

                var directory = new DirectoryInfo(path);
                var segment = new Segment
                {
                        Name = directory.Name
                };

                var segmentElement = new XElement("segment",
                        new XAttribute("name", segment.Name ?? "(Unknown)"),
                        new XAttribute("version", Core.Version));

                var regionDir = Path.Combine(path, "Region");
                if (Directory.Exists(regionDir))
                {
                        var regionsElement = new XElement("regions");
                        foreach (var file in Directory.EnumerateFiles(regionDir, "*.xml"))
                                regionsElement.Add(XElement.Load(file));

                        segmentElement.Add(regionsElement);
                }

                void AddCategory(string fileName)
                {
                        var file = Path.Combine(path, fileName);
                        if (File.Exists(file))
                                segmentElement.Add(XElement.Load(file));
                }

                AddCategory("Locations.xml");
                AddCategory("Subregions.xml");
                AddCategory("Entities.xml");
                AddCategory("Spawns.xml");
                AddCategory("Treasures.xml");

                segment.Load(segmentElement, Core.Version);

                // initialize the workspace and pull in any scripts from the Source folder
                segment.RootPath = path;

                return segment;
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
