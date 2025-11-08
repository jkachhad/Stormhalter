using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.UI.Documents;

public partial class EntitiesDocument : UserControl
{
	public EntitiesDocument()
	{
		InitializeComponent();
	}

	private void SpawnerButtonClick(object sender, RoutedEventArgs e)
	{
		if (sender is not Button { DataContext: SegmentSpawner spawner })
			return;
		
		var applicationPresenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

		if (applicationPresenter != null)
			spawner.Present(applicationPresenter);
	}
}

public class EntitiesViewModel : ObservableRecipient
{
	public string Name => "(Entities)";

	private SegmentEntity? _entity;
	private Script? _selectedScript;

	public SegmentEntity? Entity
	{
		get => _entity;
		set
		{
			if (!SetProperty(ref _entity, value))
				return;

			if (_entity != null)
			{
				if (_selectedScript is null || !_entity.Scripts.Contains(_selectedScript))
					SelectedScript = _entity.Scripts.FirstOrDefault(s => s.IsEnabled);
			}
			else
			{
				SelectedScript = null;
			}
		}
	}

	public Script? SelectedScript
	{
		get => _selectedScript;
		set => SetProperty(ref _selectedScript, value);
	}
}
