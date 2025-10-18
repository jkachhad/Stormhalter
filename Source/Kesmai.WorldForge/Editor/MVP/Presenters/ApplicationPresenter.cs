using CommonServiceLocator;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Editor;

public class GetActiveSegmentRequestMessage : RequestMessage<Segment>
{
}

public class SegmentChangedMessage(Segment Segment) : ValueChangedMessage<Segment>(Segment);
public class ActiveContentChanged(ISegmentObject segmentObject) : ValueChangedMessage<ISegmentObject>(segmentObject);
	
public class UnregisterEvents
{
}

public class ApplicationPresenter : ObservableRecipient
{
	private int _unitSize = 55;
	
	private Segment _segment;
	
	private Selection _selection;
	private TerrainSelector _filter;

	private bool _showSubregions;

	private object _activeDocument;
	private object _previousDocument;

	private ISegmentObject _activeContent;

	public Selection Selection
	{
		get => _selection;
		set => _selection = value;
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
				WeakReferenceMessenger.Default.Send(new SegmentChangedMessage(Segment));
		}
	}

	public RelayCommand CreateSegmentCommand { get; set; }
	public RelayCommand CloseSegmentCommand { get; set; }
	public RelayCommand OpenSegmentCommand { get; set; }
	public RelayCommand<bool> SaveSegmentCommand { get; set; }

	public RelayCommand CreateRegionCommand { get; set; }
	public RelayCommand<object> DeleteRegionCommand { get; set; }
	public RelayCommand<object> ShiftRegionCommand { get; set; }

	public RelayCommand GenerateRegionCommand { get; set; }
		
	public RelayCommand ExitApplicationCommand { get; set; }

	public RelayCommand ShowChangesWindow { get; set; }
	public RelayCommand LaunchWiki { get; set; }

	public RelayCommand<String> SwapDocumentCommand { get; set; }
		
	public ObservableCollection<object> Documents { get; private set; }
		
	public object ActiveDocument
	{
		get => _activeDocument;
		set => SetProperty(ref _activeDocument, value, true);
	}

	public ISegmentObject ActiveContent
	{
		get => _activeContent;
		set
		{
			// only send the message if the content actually changes.
			// we use a delayed send to allow the UI to update first.
			if (value != _activeContent && SetProperty(ref _activeContent, value))
				WeakReferenceMessenger.Default.SendDelayed(new ActiveContentChanged(value), TimeSpan.FromMilliseconds(50));
		}
	}

	public RelayCommand ExportToPdfCommand { get; set; }
    public RelayCommand ConvertSegmentCommand { get; }

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
			
		OpenSegmentCommand = new RelayCommand(OpenSegment, () => (Segment == null));
		OpenSegmentCommand.DependsOn(() => Segment);
			
		SaveSegmentCommand = new RelayCommand<bool>(SaveSegment, (queryPath) => (Segment != null));
		SaveSegmentCommand.DependsOn(() => Segment);
		
		ConvertSegmentCommand = new RelayCommand(ConvertSegment, () => (Segment is null));
		ConvertSegmentCommand.DependsOn(() => Segment);

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
				this, (r, m) =>
				{
					if (Segment is not null)
						Segment.UpdateTiles();
				});
		
		ExitApplicationCommand = new RelayCommand(() => Application.Current.Shutdown());
			
		ShowSubregions = true;

		Selection = new Selection();

		// update active document when the segment changes occurs.
		messenger.Register<SegmentChangedMessage>(this, (_, message) =>
		{
			if (message.Value is not null)
				SetActiveDocument(Documents.FirstOrDefault());
		});
		
		// when content changes, ensure the appropriate document is active.
		messenger.Register<ActiveDocumentChanged>(this, (r, message) =>
		{
			ActiveDocument = message.Content;
		});

		// when a document is closed, remove it from the list of documents. Set the next document as active.
		messenger.Register<DocumentClosed>(this, (r, message) =>
		{
			var content = message.Document.Content;
			
			if (Documents.Contains(content))
				Documents.Remove(content);
		});

		// when a segment object is selected, find or create the appropriate document and set it active.
		// also set the active content to the selected object, if applicable.
		messenger.Register<SegmentObjectSelected>(this, (r, message) =>
		{
			if (message.Value != null)
				message.Value.Present(this);
		});
	}

	public void SetActiveDocument(object target, ISegmentObject content = default)
	{
		if (ActiveDocument != target)
		{
			if (!Documents.Contains(target))
				Documents.Add(target);

			WeakReferenceMessenger.Default.Send(new ActivateDocument(target));
		}

		if (content != default)
			SetActiveContent(content);
	}
	
	public void SetActiveContent(ISegmentObject content)
	{
		if (content != null && ActiveContent != content)
			ActiveContent = content;
	}

	public void SwapDocument(String Target)
	{
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
	}

	private void CloseSegment()
	{
		if (_segment == null)
			throw new InvalidOperationException("Attempt to close a segment when an active segment does not exist.");

		Segment = null;
		WeakReferenceMessenger.Default.Send(new UnregisterEvents());
		Documents.Clear();
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
		
		// show dialog for folder selection
		var dialog = new Microsoft.Win32.OpenFolderDialog()
		{
			Multiselect = false,
		};
		
		var openResult = dialog.ShowDialog();
		
		if (!openResult.HasValue || openResult != true)
			return;
		
		var targetDirectory = new DirectoryInfo(dialog.FolderName);
		var segment = new Segment()
		{
			Name = targetDirectory.Name,
			Directory = targetDirectory.FullName
		};
		
		var regionsFolder = new DirectoryInfo(Path.Combine(targetDirectory.FullName, "Regions"));

		if (regionsFolder.Exists)
		{
			foreach (var file in regionsFolder.GetFiles("*.xml"))
			{
				var regionDocument = XDocument.Load(file.FullName);
				var regionRoot = regionDocument.Root;

				if (regionRoot is null)
					throw new Exception($"Region file {file.Name} is not valid XML.");
				
				segment.Regions.Add(new SegmentRegion(regionRoot));
			}
		}

		void process(string documentName, Action assignment, Action<XElement, Version> load)
		{
			var documentFile = new FileInfo(Path.Combine(targetDirectory.FullName, documentName));
		
			if (documentFile.Exists)
			{
				var document = XDocument.Load(documentFile.FullName);
				var documentRoot = document.Root;

				if (documentRoot is null)
					throw new Exception($"Location file {documentFile.Name} is not valid XML.");

				assignment();
				load(documentRoot, Core.Version);
			}
		}
		
		process("Locations.xml", () => segment.Locations = new SegmentLocations(), 
			(root, version) => segment.Locations.Load(root, version));
		
		process("Subregions.xml", () => segment.Subregions = new SegmentSubregions(), 
			(root, version) => segment.Subregions.Load(root, version));
		
		process("Entities.xml", () => segment.Entities = new SegmentEntities(),
			(root, version) => segment.Entities.Load(root, version));
		
		process("Spawns.xml", () => segment.Spawns = new SegmentSpawns(),
			(root, version) => segment.Spawns.Load(segment.Entities, root, version));
		
		process("Treasures.xml", () => segment.Treasures = new SegmentTreasures(),
			(root, version) => segment.Treasures.Load(root, version));

		Segment = segment;
		Segment.UpdateTiles();
		
		/*SelectTool(Tools.FirstOrDefault());*/
	}

	private void SaveSegment(bool queryPath)
	{
		var targetPath = String.Empty;
		
		if (!queryPath && String.IsNullOrEmpty(_segment.Directory))
			queryPath = true;

		if (queryPath)
		{
			var dialog = new Microsoft.Win32.OpenFolderDialog()
			{
				Multiselect = false,
			};

			var saveResult = dialog.ShowDialog();

			if (!saveResult.HasValue || saveResult != true)
				return;

			targetPath = dialog.FolderName;
		}
		else
		{
			targetPath = _segment.Directory;
		}

		try
		{
			var regionsDirectory = new DirectoryInfo(Path.Combine(targetPath, "Regions"));
			
			if (!regionsDirectory.Exists)
				regionsDirectory.Create();
			
			foreach (var region in _segment.Regions)
				region.GetXElement().Save(Path.Combine(regionsDirectory.FullName, $"{region.ID}.xml"));
			
			void write(Action<XElement> saveAction, string elementName, string fileName)
			{
				var element = new XElement(elementName);
				saveAction(element);
				element.Save(Path.Combine(targetPath, fileName));
			}

			write(_segment.Locations.Save, "locations", "Locations.xml");
			write(_segment.Subregions.Save, "subregions", "Subregions.xml");
			write(_segment.Entities.Save, "entities", "Entities.xml");
			write(_segment.Spawns.Save, "spawns", "Spawns.xml");
			write(_segment.Treasures.Save, "treasures", "Treasures.xml");
			
			// find the project file and save it
			var segmentProject = new FileInfo(Path.Combine(targetPath, $"{_segment.Name}.csproj"));

			if (!segmentProject.Exists)
			{
				var projectRoot = new XElement("Project",
					new XAttribute("Sdk", "Microsoft.NET.Sdk"),
					new XElement("PropertyGroup",
						new XElement("OutputType", "Library"),
						new XElement("TargetFramework", "net8.0-windows8.0"),
						new XElement("RootNamespace", _segment.Name),
						new XElement("AssemblyName", _segment.Name),
						new XElement("EnableDefaultItems", false)
					),
					new XElement("ItemGroup",
						new XElement("Compile", new XAttribute("Include", "Source/**/*.cs"))
					),
					new XElement("ItemGroup",
						new XElement("PackageReference", new XAttribute("Include", "Kesmai.Server.Reference"), new XAttribute("Version", "*"))
					)
				);
		
				var additionalFiles = new []
				{
					"Locations.xml",
					"Subregions.xml",
					"Entities.xml",
					"Spawns.xml",
					"Treasures.xml"
				};

				if (additionalFiles.Any())
				{
					projectRoot.Add(
						new XElement("ItemGroup",
							additionalFiles.Select(f =>
								new XElement("AdditionalFiles", new XAttribute("Include", f.Replace('\\', '/'))))
						)
					);
				}
				
				new XDocument(projectRoot).Save(segmentProject.FullName);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error when saving project: {ex.Message}", "Unable to save", MessageBoxButton.OK, MessageBoxImage.Error);
		}
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

    private void ConvertSegment()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog()
        {
            DefaultExt = ".mapproj",
            Filter = "WorldForge - Map Project (*.mapproj)|*.mapproj",
            Title = "Convert Segment"
        };

        var result = dialog.ShowDialog();

        if (!result.HasValue || !result.Value)
	        return;

        var targetFile = new FileInfo(dialog.FileName);
        var targetPath = targetFile.Directory;

        if (targetPath is null)
	        throw new InvalidOperationException("Target path is invalid.");
       
		// convert the segment from xml to a directory based format
		var segmentDocument = XDocument.Load(dialog.FileName);
		var segmentDirectory = targetPath.CreateSubdirectory(Path.GetFileNameWithoutExtension(targetFile.Name));

		// load the segment object, we may use it to populate the definition file
		var segmentRoot = segmentDocument.Root;
		
		if (segmentRoot is null)
			throw new InvalidOperationException("Segment file is invalid.");

		var segmentNameAttribute = segmentRoot.Attribute("name");
		var versionAttribute = segmentRoot.Attribute("version");

		if (segmentNameAttribute is null || versionAttribute is null)
			throw new InvalidOperationException("Segment file is invalid.");
		
		var segmentName = segmentNameAttribute.Value;
		var segmentVersion = Version.Parse(versionAttribute.Value);

		var segmentScriptElement = segmentRoot.Element("script");
		
		// create the internal source directory
		var sourceDirectory = segmentDirectory.CreateSubdirectory("Source");
		var definitionScript = new FileInfo(Path.Combine(targetPath.FullName, $"{segmentName}.cs"));

		if (segmentScriptElement != null)
		{
			var internalScript = segmentScriptElement.Elements("block").ToArray()[1];
			
			File.WriteAllText(Path.Combine(sourceDirectory.FullName, "Internal.cs"),
				$"namespace Kesmai.Server.Segments;\r\n\r\npublic partial class {segmentName}\n{{\n{internalScript.Value.Trim('\r', '\n')}\n}}");
		}

		if (definitionScript.Exists)
			definitionScript.CopyTo(Path.Combine(sourceDirectory.FullName, $"{segmentName}.cs"));
		
		// convert regions to individual files.
		var regionsDirectory = segmentDirectory.CreateSubdirectory("Regions");
		var regionsElement = segmentRoot.Element("regions");
		
		if (regionsElement is null)
			throw new InvalidOperationException("Segment file is invalid.");
		
		var regionElements = regionsElement.Elements("region");

		foreach (var region in regionElements)
		{
			var idElement = region.Element("id");
			
			if (idElement is null)
				throw new InvalidOperationException("Region is missing an ID.");
			
			region.Save(Path.Combine(regionsDirectory.FullName, $"{idElement.Value}.xml"));
		}

		// other data
		void write(XElement rootElement, string fileName)
		{
			rootElement.Save(Path.Combine(segmentDirectory.FullName, fileName));
		}

		write(segmentRoot.Element("locations"), "Locations.xml");
		write(segmentRoot.Element("subregions"), "Subregions.xml");
		write(segmentRoot.Element("entities"), "Entities.xml");
		write(segmentRoot.Element("spawns"), "Spawns.xml");
		write(segmentRoot.Element("treasures"), "Treasures.xml");
		
		void cleanup(string documentName, Func<XElement, IEnumerable<XElement>> scriptSelector)
		{
			var documentPath = Path.Combine(segmentDirectory.FullName, documentName);
			var document = XDocument.Load(documentPath);
			var documentRoot = document.Root;
		
			if (documentRoot is null)
				throw new InvalidOperationException($"{documentName} document is invalid.");

			var scripts = scriptSelector(documentRoot).ToList();
		
			foreach (var scriptElement in scripts)
			{
				var blocks = scriptElement.Elements("block").ToArray();
				var body = blocks[1].Value;
				
				// trim leading/trailing new line
				body = body.Trim('\r', '\n');
			
				scriptElement.ReplaceWith(new XElement("script",
					new XAttribute("name", scriptElement.Attribute("name")?.Value ?? "(Unnamed)"),
					new XAttribute("enabled", scriptElement.Attribute("enabled")?.Value ?? "true"),
					new XCData(body))
				);
			}
		
			document.Save(documentPath);
		}

		// go through and clean up scripts.
		cleanup("Spawns.xml", (root) => root.Elements("spawn").Elements("script"));
		cleanup("Entities.xml", (root) => root.Elements("entity").Elements("script"));
		cleanup("Treasures.xml", (root) => root.Elements("treasure").Elements("entry").Elements("script"));
		
		// create the project file
		var projectRoot = new XElement("Project",
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
			),
			new XElement("ItemGroup",
				new XElement("PackageReference", new XAttribute("Include", "Kesmai.Server.Reference"), new XAttribute("Version", "*"))
			)
		);
		
		var additionalFiles = new []
		{
			"Locations.xml",
			"Subregions.xml",
			"Entities.xml",
			"Spawns.xml",
			"Treasures.xml"
		};

		if (additionalFiles.Any())
		{
			projectRoot.Add(
				new XElement("ItemGroup",
					additionalFiles.Select(f =>
						new XElement("AdditionalFiles", new XAttribute("Include", f.Replace('\\', '/'))))
				)
			);
		}

		new XDocument(projectRoot).Save(Path.Combine(segmentDirectory.FullName, $"{segmentName}.csproj"));
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
