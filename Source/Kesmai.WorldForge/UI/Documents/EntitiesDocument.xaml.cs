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
	public class GetSelectedSpawner : RequestMessage<Spawner>
	{
	}
	public class GetCurrentScriptSelection : RequestMessage<String>
	{
	}

	public EntitiesDocument()
	{
		InitializeComponent();
			
		WeakReferenceMessenger.Default
			.Register<EntitiesDocument, EntitiesViewModel.SelectedEntityChangedMessage>(this, OnEntityChanged);
			
		WeakReferenceMessenger.Default
			.Register<EntitiesDocument, Entity>(
				this, (r, m) => { ChangeEntity(m); });
		
		WeakReferenceMessenger.Default.Register<EntitiesDocument, GetCurrentScriptSelection>(this,
			(r, m) => m.Reply(GetScriptSelection()));

		WeakReferenceMessenger.Default.Register<EntitiesDocument, UnregisterEvents>(this,
			(r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });
	}

	private void ChangeEntity(Entity entity)
	{
		var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
		var viewmodel = presenter.Documents.Where(d => d is EntitiesViewModel).FirstOrDefault() as EntitiesViewModel;
		presenter.ActiveDocument = viewmodel;
		if (entity is not null)
		{
			// Set the previous SelectedEntity to null
			viewmodel.SelectedEntity = null;

			// Assign the new value
			viewmodel.SelectedEntity = entity;
		}
	}
	private void OnEntityChanged(EntitiesDocument recipient, EntitiesViewModel.SelectedEntityChangedMessage message)
	{
		_scriptsTabControl.SelectedIndex = 0;
	}

	public String GetScriptSelection ()
	{
		if (_scriptsTabControl.HasItems)
		{
			ContentPresenter cp = _scriptsTabControl.Template.FindName("PART_SelectedContentHost", _scriptsTabControl) as ContentPresenter;
			ScriptEditor editor = _scriptsTabControl.ContentTemplate.FindName("_editor", cp) as ScriptEditor;
			return editor.SelectedText;
		} else 
			return null;

	}
}

public class EntitiesViewModel : ObservableRecipient, IDisposable
{
    private bool _isDisposed = false;
    public void Dispose()
    {
        _selectedEntity = null;
        //_groups = null;
        _isDisposed = true;
    }
    public class SelectedEntityChangedMessage : ValueChangedMessage<Entity>
	{
		public SelectedEntityChangedMessage(Entity value) : base(value)
		{
		}
	}

	public string Name => "(Entities)";

	private Entity _selectedEntity;
	private Segment _segment;
	
	public Entity SelectedEntity
	{
		get => _selectedEntity;
		set
        {
            if (_isDisposed)
            {
            }

            // Clear the previous selected entity
            _selectedEntity = null;

            // Set the new selected entity
            SetProperty(ref _selectedEntity, value, true);
    
            if (_selectedEntity != null && _segment != null)
            {
                // Send message only if the selected entity is not null
                WeakReferenceMessenger.Default.Send(new SelectedEntityChangedMessage(_selectedEntity));
            }
        }
    }

    public SegmentEntities Source => _segment.Entities;
    
	public EntitiesViewModel(Segment segment)
	{
		_segment = segment ?? throw new ArgumentNullException(nameof(segment));
	}
}