using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.UI.Documents;

public partial class EntitiesDocument : UserControl
{
	public EntitiesDocument()
	{
		InitializeComponent();
		
		var messenger = WeakReferenceMessenger.Default;
		
		messenger.Register<ActiveContentChanged>(this, (_, message) =>
		{
			if (message.Value is not Entity segmentEntity)
				return;

			_scriptsTabControl.SelectedItem = segmentEntity.Scripts.FirstOrDefault();
		});
	}
}

public class EntitiesViewModel : ObservableRecipient
{
	public string Name => "(Entities)";
}