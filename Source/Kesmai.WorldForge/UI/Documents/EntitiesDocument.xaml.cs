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
		
		WeakReferenceMessenger.Default.Register<EntitiesDocument, UnregisterEvents>(this,
			(r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });
	}
}

public class EntitiesViewModel : ObservableRecipient, IDisposable
{
    private bool _isDisposed = false;
    
    public void Dispose()
    {
        _isDisposed = true;
    }

	public string Name => "(Entities)";
}