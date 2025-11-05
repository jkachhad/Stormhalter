using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentTemplateAdded(SegmentTemplate template) : ValueChangedMessage<SegmentTemplate>(template);
public class SegmentTemplateRemoved(SegmentTemplate template) : ValueChangedMessage<SegmentTemplate>(template);

public class SegmentTemplatesReset();
public class SegmentTemplatesChanged(SegmentTemplates templates) : ValueChangedMessage<SegmentTemplates>(templates);

public class SegmentTemplates : ObservableCollection<SegmentTemplate>
{
    public string Name => "(Templates)";

    public void Load(XElement element, Version version)
    {
        Clear();

        if (element is null)
            return;

        foreach (var templateElement in element.Elements("template"))
            Add(new SegmentTemplate(templateElement));
    }

    public void Save(XElement element)
    {
        if (element is null)
            return;

        foreach (var template in this)
            element.Add(template.GetSerializingElement());
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        base.OnCollectionChanged(args);

        if (args.NewItems != null)
        {
            foreach (var newItem in args.NewItems.OfType<SegmentTemplate>())
                WeakReferenceMessenger.Default.Send(new SegmentTemplateAdded(newItem));
        }

        if (args.OldItems != null)
        {
            foreach (var oldItem in args.OldItems.OfType<SegmentTemplate>())
                WeakReferenceMessenger.Default.Send(new SegmentTemplateRemoved(oldItem));
        }
        
        if (args.Action is NotifyCollectionChangedAction.Reset)
            WeakReferenceMessenger.Default.Send(new SegmentTemplatesReset());

        WeakReferenceMessenger.Default.Send(new SegmentTemplatesChanged(this));
    }
}
