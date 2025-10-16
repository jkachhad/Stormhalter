using System;
using System.Windows.Controls;
using Kesmai.WorldForge.Editor;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge.UI.Documents;
	
public partial class TreasuresDocument : UserControl
{
	public TreasuresDocument()
	{
		InitializeComponent();
		
		var messenger = WeakReferenceMessenger.Default;
		
		messenger.Register<ActiveContentChanged>(this, (_, message) =>
		{
			if (message.Value is not SegmentTreasure)
				return;
		});
	}
}

public class TreasuresViewModel : ObservableRecipient
{
	public string Name => "(Treasures)";
}
