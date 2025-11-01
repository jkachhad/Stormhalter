using System.Collections.ObjectModel;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI;

namespace Kesmai.WorldForge;

public class RegionToolChanged(Tool Tool) : ValueChangedMessage<Tool>(Tool);

public class RegionToolbar : ObservableRecipient
{
	private Tool _selectedTool;
	
	public Tool SelectedTool
	{
		get => _selectedTool;
		set
		{
			if (SetProperty(ref _selectedTool, value) && value != null)
			{
				foreach (var tool in Tools)
				{
					if (tool != value && tool.IsActive)
						tool.IsActive = false;
				}

				if (!value.IsActive)
					value.IsActive = true;
			}
		}
	}
	
	public ObservableCollection<Tool> Tools { get; }
	
	public RelayCommand<Tool> SelectToolCommand { get; set; }

	public RegionToolbar()
	{
		Tools = new ObservableCollection<Tool>()
		{
			Tool.Default,
				
			new DrawTool(),
			new EraseTool(),
			new PaintTool(),
			new HammerTool(),
		};

		SelectToolCommand = new RelayCommand<Tool>(SelectTool, (tool) =>
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

			if (presenter is null || presenter.Segment is null || presenter.ActiveContent is not SegmentRegion)
				return false;

			return true;
		});
		SelectToolCommand.DependsOn(() => ServiceLocator.Current.GetInstance<ApplicationPresenter>().Segment);
		
		SelectTool(Tool.Default);
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
			
			WeakReferenceMessenger.Default.Send(new RegionToolChanged(nextTool));

			var worldScreen = ServiceLocator.Current.GetInstance<WorldGraphicsScreen>();

			if (worldScreen != null)
				worldScreen.InvalidateRender();
		}
	}
}