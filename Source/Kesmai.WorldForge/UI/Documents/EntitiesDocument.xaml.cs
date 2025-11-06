using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge.UI.Documents;

public partial class EntitiesDocument : UserControl
{
	public EntitiesDocument()
	{
		InitializeComponent();
		
		var messenger = WeakReferenceMessenger.Default;
		
		messenger.Register<ActiveContentChanged>(this, (_, message) =>
		{
			if (message.Value is not SegmentEntity segmentEntity)
				return;

			_scriptsTabControl.SelectedItem = segmentEntity.Scripts.FirstOrDefault();
		});
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
}
