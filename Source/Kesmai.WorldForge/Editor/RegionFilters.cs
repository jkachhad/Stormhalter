using System.Collections.ObjectModel;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class RegionFilterChanged(TerrainSelector Filter) : ValueChangedMessage<TerrainSelector>(Filter);

public class RegionFilters : ObservableRecipient
{
	private TerrainSelector _selectedFilter;
	public TerrainSelector SelectedFilter
	{
		get => _selectedFilter ?? TerrainSelector.Default;
		set => SetProperty(ref _selectedFilter, value);
	}

	public ObservableCollection<TerrainSelector> Filters { get; }
	public RelayCommand<TerrainSelector> SelectFilterCommand { get; set; }

	public RegionFilters()
	{
		Filters = new ObservableCollection<TerrainSelector>()
		{
			TerrainSelector.Default,
			
			new FloorSelector(),
			new StaticSelector(),
			new WaterSelector(),
			new WallSelector(),
			new StructureSelector(),
		};
		
		SelectFilterCommand = new RelayCommand<TerrainSelector>(SelectFilter, (filter) =>
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			
			if (presenter is null || presenter.Segment is null || presenter.ActiveDocument is not SegmentRegion)
				return false;

			return true;
		});
		SelectFilterCommand.DependsOn(() => ServiceLocator.Current.GetInstance<ApplicationPresenter>().Segment);
	}

	public void SelectFilter(TerrainSelector filter)
	{
		SelectedFilter = filter;
		
		foreach (var f in Filters)
		{
			if (f != filter && f.IsActive)
				f.IsActive = false;
		}

		filter.IsActive = true;

		WeakReferenceMessenger.Default.Send(new RegionFilterChanged(filter));
	}
}