using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Collections;
using DigitalRune.Graphics;
using DigitalRune.ServiceLocation;
using Ionic.Zip;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.Roslyn;
using Kesmai.WorldForge.UI;
using Kesmai.WorldForge.UI.Documents;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Editor
{
	public class GetActiveSegmentRequestMessage : RequestMessage<Segment>
	{
	}
	
	public class ApplicationPresenter : ObservableRecipient
	{
		private int _unitSize = 55;
		
		private string _segmentFilePath;
		
		private Segment _segment;
		private CustomRoslynHost _roslynHost;
		private Selection _selection;
		private TerrainSelector _filter;
		private Tool _selectedTool;

		private bool _showSubregions;

		private object _activeDocument;

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

		public RelayCommand CreateSegmentCommand { get; set; }
		public RelayCommand CloseSegmentCommand { get; set; }
		public RelayCommand OpenSegmentCommand { get; set; }
		public RelayCommand<bool> SaveSegmentCommand { get; set; }

		public RelayCommand CreateRegionCommand { get; set; }
		public RelayCommand<object> DeleteRegionCommand { get; set; }
		public RelayCommand<object> ShiftRegionCommand { get; set; }

		public RelayCommand GenerateRegionCommand { get; set; }
		
		public RelayCommand<TerrainSelector> SelectFilterCommand { get; set; }
		public RelayCommand<Tool> SelectToolCommand { get; set; }

		public RelayCommand ExitApplicationCommand { get; set; }
		
		public ObservableCollection<object> Documents { get; private set; }
		
		public object ActiveDocument
		{
			get => _activeDocument;
			set => SetProperty(ref _activeDocument, value, true);
		}
		
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


			WeakReferenceMessenger.Default
				.Register<ApplicationPresenter, SegmentLocation>(
					this, (r, m) => { this.ActiveDocument = this.Documents.Where(d => d is LocationsViewModel).FirstOrDefault() as LocationsViewModel; JumpLocation(m); });
			WeakReferenceMessenger.Default
				.Register<ApplicationPresenter, SegmentSubregion>(
					this, (r, m) => { this.ActiveDocument = this.Documents.Where(d => d is SubregionViewModel).FirstOrDefault() as SubregionViewModel; JumpSubregion(m); });
			WeakReferenceMessenger.Default
				.Register<ApplicationPresenter, Entity>(
					this, (r, m) => { this.ActiveDocument = this.Documents.Where(d => d is EntitiesViewModel).FirstOrDefault() as EntitiesViewModel; JumpEntity(m); });
			WeakReferenceMessenger.Default
				.Register<ApplicationPresenter, Spawner>(
					this, (r, m) => { this.ActiveDocument = this.Documents.Where(d => d is SpawnsViewModel).FirstOrDefault() as SpawnsViewModel; JumpSpawner(m); });
			WeakReferenceMessenger.Default
				.Register<ApplicationPresenter, SegmentTreasure>(
					this, (r, m) => { this.ActiveDocument = this.Documents.Where(d => d is TreasuresViewModel).FirstOrDefault() as TreasuresViewModel; JumpTreasure(m); });
			WeakReferenceMessenger.Default
				.Register<ApplicationPresenter, Segment>(
					this, (r, m) => { this.ActiveDocument = this.Documents.Where(d => d is SegmentViewModel).FirstOrDefault() as SegmentViewModel; });

			SelectFilterCommand = new RelayCommand<TerrainSelector>(SelectFilter, 
				(filter) => (Segment != null) && (ActiveDocument is SegmentRegion));
			SelectFilterCommand.DependsOn(() => Segment, () => ActiveDocument);
			
			SelectToolCommand = new RelayCommand<Tool>(SelectTool, 
				(tool) => (Segment != null) && (ActiveDocument is SegmentRegion));
			SelectToolCommand.DependsOn(() => Segment, () => ActiveDocument);

			Filters = new NotifyingCollection<TerrainSelector>()
			{
				TerrainSelector.Default,
				
				new FloorSelector(),
				new StaticSelector(),
				new WaterSelector(),
				new WallSelector(),
				new StructureSelector(),
				new TeleporterSelector(),
				new SpawnSelector(),
			};
			
			Tools = new NotifyingCollection<Tool>()
			{
				Tool.Default,
				
				new DrawTool(),
				new EraseTool(),
				new PaintTool(),
				new HammerTool(),
			};
			
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
				//_typeSelector.SelectedIndex = 0;
				viewmodel.SelectedLocationSpawner = spawner as LocationSpawner;
			}

			if (spawner is RegionSpawner)
			{
				//_typeSelector.SelectedIndex = 1;
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
			Documents.Clear();
			
			_segmentFilePath = String.Empty;
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
			
			var segment = new Segment();

			if (!targetFileInfo.IsZipFile())
			{
				var document = XDocument.Load(targetFile);
				var rootElement = document.Root;

				if (rootElement != null)
					segment.Load(rootElement);
			}
			else
			{
				using (var archive = new ZipFile(targetFile))
					segment.Load(archive);
			}
			
			Documents.Add(new SegmentViewModel(segment));
			Documents.Add(new LocationsViewModel(segment));
			Documents.Add(new SubregionViewModel(segment));
			Documents.Add(new EntitiesViewModel(segment));
			Documents.Add(new SpawnsViewModel(segment));
			Documents.Add(new TreasuresViewModel(segment));

			

			Segment = segment;
			Segment.UpdateTiles();
			
			_segmentFilePath = targetFile;
			
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
			
			if (File.Exists(targetFile))
				File.Delete(targetFile);

			_segmentFilePath = targetFile;

#if (ArchiveStorage)
			var projectFile = new ZipFile(targetFile)
			{
				Comment = Core.Version.ToString()
			};
			
			_segment.Save(projectFile);
#else
			var projectFile = new XDocument();
			var segmentElement = new XElement("segment",
				new XAttribute("name", _segment.Name ?? "(Unknown)"),
				new XAttribute("version", Core.Version));
			
			_segment.Save(segmentElement);
			
			projectFile.Add(segmentElement);
#endif
			
			projectFile.Save(targetFile);
		}
		
		private void CreateRegion()
		{
			if (_segment == null)
				throw new InvalidOperationException("Attempt to create a region when an segment does not exists.");

			var index = 0;
			var freeIndex = 0;

			do
			{
				freeIndex++;
				
				if (_segment.Regions.FirstOrDefault<SegmentRegion>((r) => r.ID == freeIndex) == default(SegmentRegion))
					index = freeIndex;

			} while (index == 0);

			var region = new SegmentRegion(freeIndex);

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
	}
}
