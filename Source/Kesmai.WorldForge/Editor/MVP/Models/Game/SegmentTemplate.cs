using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.UI.Documents;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge.Editor;

public class SegmentTemplateChanged(SegmentTemplate template) : ValueChangedMessage<SegmentTemplate>(template);

public class SegmentTemplate : ObservableObject, ISegmentObject, IComponentProvider
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                WeakReferenceMessenger.Default.Send(new SegmentTemplateChanged(this));
        }
    }
    
    [Browsable(false)]
    public ObservableCollection<IComponentProvider> Providers { get; }

    public SegmentTemplate(XElement element) : this()
    {
        if (element is null)
            throw new ArgumentNullException(nameof(element));

        var nameAttribute = element.Attribute("name");

        if (nameAttribute is null)
            throw new ArgumentNullException(nameof(nameAttribute));
        
        _name = nameAttribute.Value;
        
        var componentPalette = ServiceLocator.Current.GetInstance<ComponentPalette>();
        
        if (componentPalette is null)
            throw new InvalidOperationException("Could not get component palette from service locator.");
        
        foreach (var providerElement in element.Elements())
        {
            if (componentPalette.TryGetComponent(providerElement, out var provider))
                Providers.Add(provider);
        }
    }
    
    public SegmentTemplate()
    {
        Providers = new ObservableCollection<IComponentProvider>();
        Providers.CollectionChanged += OnProvidersChanged;
    }
    
    public void Present(ApplicationPresenter presenter)
    {
        var viewModel = presenter.Documents.OfType<SegmentTemplateViewModel>().FirstOrDefault();

        if (viewModel is null)
            presenter.Documents.Add(viewModel = new SegmentTemplateViewModel());

        viewModel.Template = this;

        presenter.SetActiveDocument(viewModel, this);
    }

    public void Copy(Segment segment)
    {
        if (Clone() is SegmentTemplate segmentTemplate)
            segment.Templates.Add(segmentTemplate);
    }

    public object Clone()
    {
        return new SegmentTemplate(GetSerializingElement())
        {
            Name = $"Copy of {_name}"
        };
    }

    public XElement GetSerializingElement()
    {
        var element = new XElement("template", new XAttribute("name", _name));

        foreach (var provider in Providers)
        {
            var providerElement = provider.GetReferencingElement();

            if (providerElement is null)
                continue;

            element.Add(providerElement);
        }

        return element;
    }

    public XElement GetReferencingElement()
    {
        return new XElement("template", new XAttribute("name", _name));
    }

    public ComponentFrame GetComponentFrame()
    {
        return new SegmentTemplateComponentFrame(this);
    }

    public void AddComponent(ObservableCollection<IComponentProvider> collection)
    {
        collection.Add(this);
    }

    public void RemoveComponent(ObservableCollection<IComponentProvider> collection)
    {
        collection.Remove(this);
    }

    public IEnumerable<IComponentProvider> GetComponents()
    {
        foreach (var provider in Providers)
            yield return provider;
    }

    public IEnumerable<ComponentRender> GetRenders(int mx, int my) => GetRenders();
    
    public IEnumerable<ComponentRender> GetRenders()
    {
        foreach (var provider in Providers)
            foreach (var render in provider.GetRenders())
                yield return render;
    }

    private void OnProvidersChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new SegmentTemplateChanged(this));
    }
}
