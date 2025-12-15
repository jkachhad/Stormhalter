using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Controls;

namespace Kesmai.WorldForge.UI;

internal sealed class TemplatesTreeViewItem : TreeViewItem, IDisposable
{
    private readonly Segment _segment;
    private readonly Dictionary<SegmentTemplate, SegmentTreeViewItem> _templateItems = new();
    
    private int _nextTemplateId;

    public TemplatesTreeViewItem(Segment segment, object header)
    {
        _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Tag = "category:Templates";

        ContextMenu = new ContextMenu();
        ContextMenu.AddItem("Add Template", "Add.png", (s, e) =>
        {
            var template = new SegmentTemplate
            {
                Name = $"Template {_nextTemplateId++}"
            };

            _segment.Templates.Add(template);

            template.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
        });

        _nextTemplateId = 0;

        var messenger = WeakReferenceMessenger.Default;

        messenger.Register<SegmentTemplateAdded>(this, (_, message) =>
        {
            var templateItem = CreateTemplateItem(message.Value);

            if (!Items.Contains(templateItem))
                Items.Add(templateItem);
        });

        messenger.Register<SegmentTemplateRemoved>(this, (_, message) =>
        {
            if (_templateItems.Remove(message.Value, out var item))
                Items.Remove(item);
        });

        messenger.Register<SegmentTemplateChanged>(this, (_, message) =>
        {
            if (_templateItems.TryGetValue(message.Value, out var item) && item.EditableTextBlock is not null)
                item.EditableTextBlock.Text = message.Value.Name;
        });

        messenger.Register<SegmentTemplatesReset>(this, (_, _) =>
        {
            Items.Clear();
            _templateItems.Clear();
        });

        foreach (var template in _segment.Templates)
            Items.Add(CreateTemplateItem(template));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        
        Items.Clear();
        
        _templateItems.Clear();
    }

    private SegmentTreeViewItem CreateTemplateItem(SegmentTemplate template)
    {
        if (_templateItems.TryGetValue(template, out var existing))
            return existing;

        var templateItem = new SegmentTreeViewItem(template, Brushes.SteelBlue, true)
        {
            Tag = template,
        };

        templateItem.ContextMenu = new ContextMenu();
        templateItem.ContextMenu.AddItem("Rename", "Rename.png", (s, e) => templateItem.Rename());
        templateItem.ContextMenu.AddItem("Duplicate", "Copy.png", (s, e) => template.Copy(_segment));
        templateItem.ContextMenu.AddItem("Delete", "Delete.png", (s, e) =>
        {
            _segment.Templates.Remove(template);

            if (templateItem.Parent is ItemsControl parent)
                parent.Items.Remove(templateItem);
        });

        _templateItems.Add(template, templateItem);
        return templateItem;
    }
}
