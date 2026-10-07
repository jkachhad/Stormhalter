using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DigitalRune.Collections;
using DigitalRune.Graphics;
using DigitalRune.ServiceLocation;
using Kesmai.WorldForge.Models;
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Editor;

public class GetActiveSegmentRequestMessage : RequestMessage<Segment>
{
}

public class ActiveSegmentChanged(Segment Segment) : ValueChangedMessage<Segment>(Segment);

/// <summary>
/// A request to show a tile in its region view; the view for that region applies it on its next update.
/// </summary>
public record RegionNavigation(SegmentRegion Region, int X, int Y);

public class ApplicationPresenter : ObservableRecipient
{
	private int _unitSize = 55;
	
	private Segment _segment;
	
	private Selection _selection;

	private object _activeDocument;

	private ISegmentObject _activeContent;
	private string _tileCoordinateDisplay = "(-, -)";

	private bool _isModified;
	private byte[] _savedFingerprint;

	private string _problemsSummary = "Click Check to scan the segment for problems.";

	public Selection Selection
	{
		get => _selection;
		set => _selection = value;
	}

	public int UnitSize => _unitSize;

	public Segment Segment
	{
		get => _segment;
		set
		{
			if (SetProperty(ref _segment, value, true))
			{
				// edits to the previous segment can't be undone in this one.
				History.Clear();

				Problems.Clear();
				ProblemsSummary = "Click Check to scan the segment for problems.";
				PendingNavigation = null;

				OnPropertyChanged(nameof(WindowTitle));
				
				WeakReferenceMessenger.Default.Send(new ActiveSegmentChanged(Segment));
			}
		}
	}

	/// <summary>
	/// Gets the undo and redo history for edits to region tiles.
	/// </summary>
	public MapEditHistory History { get; } = new MapEditHistory();

	/// <summary>
	/// Gets a value indicating whether the segment may have changed since it was opened or saved. This drives
	/// the title marker; <see cref="ConfirmDiscardChanges"/> compares the segment itself before prompting.
	/// </summary>
	public bool IsModified
	{
		get => _isModified;
		private set
		{
			if (SetProperty(ref _isModified, value))
				OnPropertyChanged(nameof(WindowTitle));
		}
	}

	public string WindowTitle
	{
		get
		{
			if (_segment is null)
				return "WorldForge";

			return $"WorldForge - {_segment.Name}{(_isModified ? " *" : String.Empty)}";
		}
	}

	public RelayCommand UndoCommand { get; }
	public RelayCommand RedoCommand { get; }

	public string UndoMenuHeader => History.CanUndo ? $"_Undo {History.UndoDescription}" : "_Undo";
	public string RedoMenuHeader => History.CanRedo ? $"_Redo {History.RedoDescription}" : "_Redo";

	/// <summary>
	/// Gets the problems found by the last check.
	/// </summary>
	public ObservableCollection<SegmentProblem> Problems { get; } = new ObservableCollection<SegmentProblem>();

	public string ProblemsSummary
	{
		get => _problemsSummary;
		private set => SetProperty(ref _problemsSummary, value);
	}

	public RelayCommand CheckProblemsCommand { get; }
	public RelayCommand<SegmentProblem> GoToProblemCommand { get; }

	/// <summary>
	/// Gets or sets a tile waiting to be shown by its region view.
	/// </summary>
	public RegionNavigation PendingNavigation { get; set; }

	public RelayCommand CreateSegmentCommand { get; set; }
	public RelayCommand CloseSegmentCommand { get; set; }
	public RelayCommand OpenSegmentCommand { get; set; }
	public RelayCommand<bool> SaveSegmentCommand { get; set; }

	public RelayCommand CreateRegionCommand { get; set; }
	public RelayCommand<object> DeleteRegionCommand { get; set; }

	public RelayCommand GenerateRegionCommand { get; set; }
		
	public RelayCommand ExitApplicationCommand { get; set; }

	public RelayCommand ShowChangesWindow { get; set; }
	public RelayCommand LaunchWiki { get; set; }
		
	public ObservableCollection<object> Documents { get; private set; }
		
	public object ActiveDocument
	{
		get => _activeDocument;
		set => SetProperty(ref _activeDocument, value, true);
	}

	public ISegmentObject ActiveContent
	{
		get => _activeContent;
		set => SetProperty(ref _activeContent, value);
	}

	public string TileCoordinateDisplay
	{
		get => _tileCoordinateDisplay;
		set => SetProperty(ref _tileCoordinateDisplay, value);
	}
	
    public RelayCommand ConvertSegmentCommand { get; }

    public ApplicationPresenter()
	{
		var messenger = WeakReferenceMessenger.Default;

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
		
		// close the main window rather than shutting down, so its closing check can offer to save changes.
		ExitApplicationCommand = new RelayCommand(() => Application.Current.MainWindow?.Close());

		UndoCommand = new RelayCommand(Undo, () => History.CanUndo);
		RedoCommand = new RelayCommand(Redo, () => History.CanRedo);

		CheckProblemsCommand = new RelayCommand(CheckProblems, () => (Segment != null));
		CheckProblemsCommand.DependsOn(() => Segment);

		GoToProblemCommand = new RelayCommand<SegmentProblem>(GoToProblem, (problem) => (problem != null));

		History.Changed += (_, _) =>
		{
			UndoCommand.NotifyCanExecuteChanged();
			RedoCommand.NotifyCanExecuteChanged();

			OnPropertyChanged(nameof(UndoMenuHeader));
			OnPropertyChanged(nameof(RedoMenuHeader));

			MarkModified();
		};

		// changes to segment data other than tiles mark the segment as modified.
		void markModifiedOn<TMessage>() where TMessage : class
		{
			messenger.Register<ApplicationPresenter, TMessage>(this, static (r, _) => r.MarkModified());
		}

		markModifiedOn<SegmentChanged>();
		markModifiedOn<SegmentRegionsChanged>();
		markModifiedOn<SegmentRegionChanged>();
		markModifiedOn<SegmentSubregionsChanged>();
		markModifiedOn<SegmentSubregionChanged>();
		markModifiedOn<SegmentLocationsChanged>();
		markModifiedOn<SegmentLocationChanged>();
		markModifiedOn<SegmentEntitiesChanged>();
		markModifiedOn<SegmentEntityChanged>();
		markModifiedOn<SegmentSpawnsChanged>();
		markModifiedOn<SegmentSpawnChanged>();
		markModifiedOn<SegmentTreasuresChanged>();
		markModifiedOn<SegmentTreasureChanged>();
		markModifiedOn<SegmentBrushesChanged>();
		markModifiedOn<SegmentBrushChanged>();
		markModifiedOn<SegmentComponentsChanged>();
		markModifiedOn<SegmentComponentChanged>();
		markModifiedOn<SegmentTemplatesChanged>();
		markModifiedOn<SegmentTemplateChanged>();

		Selection = new Selection();

		// update active document when the segment changes occurs.
		messenger.Register<ActiveSegmentChanged>(this, (_, message) =>
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
	
	private void CreateSegment()
	{
		if (_segment != null)
			throw new InvalidOperationException("Attempt to create a segment when an active segment already exists.");

		var dialog = new Microsoft.Win32.OpenFolderDialog()
		{
			Multiselect = false,
		};

		var openResult = dialog.ShowDialog();

		if (!openResult.HasValue || openResult != true)
			return;

		var targetDirectory = new DirectoryInfo(dialog.FolderName);

		if (!targetDirectory.Exists)
			targetDirectory.Create();

		var segment = new Segment()
		{
			Name = targetDirectory.Name,
			Directory = targetDirectory.FullName
		};

		Segment = segment;

		MarkSaved();
	}

	private void CloseSegment()
	{
		if (_segment == null)
			throw new InvalidOperationException("Attempt to close a segment when an active segment does not exist.");

		if (!ConfirmDiscardChanges("closing it"))
			return;
		
		Segment = null;

		Documents.Clear();

		_savedFingerprint = null;
		IsModified = false;
	}
	
	private void OpenSegment()
	{
		if (Segment != null && !ConfirmDiscardChanges("opening another segment"))
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
		
		Segment = segment;
		
		void process(string documentName, Action assignment, Action<XElement, Version> load)
		{
			var documentFile = new FileInfo(Path.Combine(targetDirectory.FullName, documentName));
		
			if (documentFile.Exists)
			{
				var document = XDocument.Load(documentFile.FullName);
				var documentRoot = document.Root;

				if (documentRoot is null)
					throw new Exception($"Location file {documentFile.Name} is not valid XML.");

				if (assignment != null)
					assignment();
				
				load(documentRoot, Core.Version);
			}
		}
		
		process("Segments.xml", null, (root, version) =>
		{
			var nameAttribute = root.Attribute("name");

			if (nameAttribute is not null)
				segment.Name = nameAttribute.Value;
		});
		
		process("Locations.xml", () => segment.Locations = new SegmentLocations(), 
			(root, version) => segment.Locations.Load(root, version));
		
		process("Components.xml", () => segment.Components = new SegmentComponents(),
			(root, version) => segment.Components.Load(root, version));
		
		process("Brushes.xml", () => segment.Brushes = new SegmentBrushes(),
			(root, version) => segment.Brushes.Load(root, version));
		
		process("Templates.xml", () => segment.Templates = new SegmentTemplates(),
			(root, version) => segment.Templates.Load(root, version));

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

		process("Subregions.xml", () => segment.Subregions = new SegmentSubregions(), 
			(root, version) => segment.Subregions.Load(root, version));
		
		process("Entities.xml", () => segment.Entities = new SegmentEntities(),
			(root, version) => segment.Entities.Load(root, version));
		
		process("Spawns.xml", () => segment.Spawns = new SegmentSpawns(),
			(root, version) => segment.Spawns.Load(segment.Entities, root, version));
		
		process("Treasures.xml", () => segment.Treasures = new SegmentTreasures(),
			(root, version) => segment.Treasures.Load(root, version));

		Segment.UpdateTiles();

		// loading raised change messages; the segment as loaded is the saved state.
		MarkSaved();
	}

	private void SaveSegment(bool queryPath)
	{
		TrySaveSegment(queryPath);
	}

	/// <returns>True if the segment was saved.</returns>
	private bool TrySaveSegment(bool queryPath)
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
				return false;

			targetPath = dialog.FolderName;
		}
		else
		{
			targetPath = _segment.Directory;
		}

		WeakReferenceMessenger.Default.Send(new SegmentSerialize(_segment));

		var saved = false;
		
		try
		{
			var regionsDirectory = new DirectoryInfo(Path.Combine(targetPath, "Regions"));
			
			if (!regionsDirectory.Exists)
				regionsDirectory.Create();

			var expectedRegionFiles = new HashSet<string>(_segment.Regions.Select(region => $"{region.ID}.xml"),
				StringComparer.OrdinalIgnoreCase);

			foreach (var existingFile in regionsDirectory.GetFiles("*.xml"))
			{
				if (!expectedRegionFiles.Contains(existingFile.Name))
					existingFile.Delete();
			}
			
			var documents = GetSegmentDocuments().ToList();

			foreach (var (relativePath, element) in documents)
				element.Save(Path.Combine(targetPath, relativePath));
			
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
						new XElement("PackageReference", new XAttribute("Include", "Kesmai.Server.Reference"), new XAttribute("Version", "*")),
						new XElement("PackageReference", new XAttribute("Include", "Kesmai.Server.Reference.Generator"), new XAttribute("Version", "*"))
					)
				);
		
				var additionalFiles = new []
				{
					"Locations.xml",
					"Subregions.xml",
					"Entities.xml",
					"Spawns.xml",
					"Treasures.xml",
					"Components.xml",
					"Brushes.xml",
					"Templates.xml",
					@"Regions\*.xml"
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

			_savedFingerprint = ComputeFingerprint(documents);
			IsModified = false;

			saved = true;
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error when saving project: {ex.Message}", "Unable to save", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		
		WeakReferenceMessenger.Default.Send(new SegmentSerialized(_segment));

		return saved;
	}

	/// <summary>
	/// Gets the files a save writes, as paths relative to the segment folder. Scripts are saved separately.
	/// </summary>
	private IEnumerable<(string RelativePath, XElement Element)> GetSegmentDocuments()
	{
		foreach (var region in _segment.Regions)
			yield return (Path.Combine("Regions", $"{region.ID}.xml"), region.GetSerializingElement());

		yield return ("Segment.xml", new XElement("segment",
			new XAttribute("name", _segment.Name),
			new XAttribute("version", Core.Version.ToString())));

		XElement build(Action<XElement> saveAction, string elementName)
		{
			var element = new XElement(elementName);
			saveAction(element);
			return element;
		}

		yield return ("Locations.xml", build(_segment.Locations.Save, "locations"));
		yield return ("Subregions.xml", build(_segment.Subregions.Save, "subregions"));
		yield return ("Entities.xml", build(_segment.Entities.Save, "entities"));
		yield return ("Spawns.xml", build(_segment.Spawns.Save, "spawns"));
		yield return ("Treasures.xml", build(_segment.Treasures.Save, "treasures"));
		yield return ("Brushes.xml", build(_segment.Brushes.Save, "brushes"));
		yield return ("Components.xml", build(_segment.Components.Save, "components"));
		yield return ("Templates.xml", build(_segment.Templates.Save, "templates"));
	}

	/// <summary>
	/// Hashes the documents a save would write, to tell whether the segment differs from its saved state.
	/// </summary>
	private static byte[] ComputeFingerprint(IEnumerable<(string RelativePath, XElement Element)> documents)
	{
		using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

		foreach (var (relativePath, element) in documents)
		{
			hash.AppendData(Encoding.UTF8.GetBytes(relativePath));
			hash.AppendData(new byte[] { 0 });
			hash.AppendData(Encoding.UTF8.GetBytes(element.ToString(SaveOptions.DisableFormatting)));
			hash.AppendData(new byte[] { 0 });
		}

		return hash.GetHashAndReset();
	}

	/// <summary>
	/// Records the segment's current state as saved.
	/// </summary>
	private void MarkSaved()
	{
		_savedFingerprint = (_segment != null) ? ComputeFingerprint(GetSegmentDocuments()) : null;
		IsModified = false;
	}

	private void MarkModified()
	{
		if (_segment != null)
			IsModified = true;
	}

	/// <summary>
	/// Determines whether the segment differs from its saved state, by comparing what a save would write.
	/// </summary>
	public bool HasUnsavedChanges()
	{
		if (_segment is null)
			return false;

		if (_savedFingerprint is null)
			return true;

		return !ComputeFingerprint(GetSegmentDocuments()).AsSpan().SequenceEqual(_savedFingerprint);
	}

	/// <summary>
	/// Offers to save unsaved changes before an action that would discard them.
	/// </summary>
	/// <param name="action">What is about to happen, e.g. "closing it".</param>
	/// <returns>True if the action can continue.</returns>
	public bool ConfirmDiscardChanges(string action)
	{
		if (!HasUnsavedChanges())
			return true;

		var result = MessageBox.Show($"Save changes to segment '{_segment.Name}' before {action}?",
			"Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

		return result switch
		{
			MessageBoxResult.Yes => TrySaveSegment(false),
			MessageBoxResult.No => true,
			_ => false,
		};
	}

	public void Undo()
	{
		History.Undo(_segment);
	}

	private void CheckProblems()
	{
		Problems.Clear();

		foreach (var problem in SegmentProblemChecker.Check(_segment))
			Problems.Add(problem);

		var errors = Problems.Count(problem => problem.Severity is ProblemSeverity.Error);
		var warnings = Problems.Count - errors;

		ProblemsSummary = (Problems.Count is 0)
			? "No problems found."
			: $"{errors} error{(errors is 1 ? "" : "s")}, {warnings} warning{(warnings is 1 ? "" : "s")}. Double-click one to go to it.";
	}

	private void GoToProblem(SegmentProblem problem)
	{
		if (problem is null || _segment is null)
			return;

		// the region may have been deleted since the check ran.
		if (problem.Region != null && _segment.Regions.Contains(problem.Region))
		{
			PendingNavigation = new RegionNavigation(problem.Region, problem.X, problem.Y);
			problem.Region.Present(this);
		}
		else
		{
			problem.Source?.Present(this);
		}
	}

	public void Redo()
	{
		History.Redo(_segment);
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
			if (rootElement is null)
				return;
			
			rootElement.Save(Path.Combine(segmentDirectory.FullName, fileName));
		}
		
		// clean up spawner names
		foreach (var element in segmentRoot.Elements("spawns"))
		{
			switch (element.Name.LocalName)
			{
				case "LocationSpawner": element.Name = "LocationRegionSpawner"; break;
				case "SubregionSpawner": element.Name = "SubregionRegionSpawner"; break;
			}
		}
		
		write(new XElement("segment", 
			new XAttribute("name", segmentName), 
			new XAttribute("version", Core.Version.ToString())), "Segment.xml");
		
		write(segmentRoot.Element("locations"), "Locations.xml");
		write(segmentRoot.Element("subregions"), "Subregions.xml");
		write(segmentRoot.Element("entities"), "Entities.xml");
		write(segmentRoot.Element("spawns"), "Spawns.xml");
		write(segmentRoot.Element("treasures"), "Treasures.xml");
		write(new XElement("components"), "Components.xml");
		write(new XElement("brushes"), "Brushes.xml");
		write(new XElement("templates"), "Templates.xml");
		
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
		cleanup("Treasures.xml", (root) => root.Elements("treasure").Elements("script"));
		
		// create the project file
		var segmentProject = new FileInfo(Path.Combine(segmentDirectory.FullName, $"{segmentName}.csproj"));
		
		var projectRoot = new XElement("Project",
			new XAttribute("Sdk", "Microsoft.NET.Sdk"),
			new XElement("PropertyGroup",
				new XElement("OutputType", "Library"),
				new XElement("TargetFramework", "net8.0-windows8.0"),
				new XElement("RootNamespace", segmentName),
				new XElement("AssemblyName", segmentName),
				new XElement("EnableDefaultItems", false),
				new XElement("GenerateAssemblyInfo", false)
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
			"Treasures.xml",
			"Components.xml",
			"Brushes.xml",
			"Templates.xml",
			@"Regions\*.xml"
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
