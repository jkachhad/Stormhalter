using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using ICSharpCode.AvalonEdit.Document;
using Kesmai.WorldForge.Editor;
using Syncfusion.Windows.PropertyGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using static Kesmai.WorldForge.UI.Documents.SpawnsDocument;
using static Kesmai.WorldForge.UI.Documents.SpawnsViewModel;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SubregionDocument : UserControl
{
	public SubregionDocument()
	{
		InitializeComponent();

        _presenter.Initialize();

        WeakReferenceMessenger.Default
            .Register<SubregionDocument, SubregionViewModel.SelectedSubregionChangedMessage>(
                this, OnSubregionChanged);

		WeakReferenceMessenger.Default.Register<SubregionDocument, UnregisterEvents>(this,
			(r, m) => { WeakReferenceMessenger.Default.UnregisterAll(this); });

    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is not SubregionViewModel viewModel)
            return;

        if (e.NewValue is SegmentSubregion region)
            viewModel.SelectedSubregion = region;
    }
    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollViewer scroller = sender as ScrollViewer;
        if (scroller != null)
        {
            scroller.ScrollToVerticalOffset(scroller.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
    private void PropertyGrid_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if ((e.Property.Name != "Name" && e.Property.Name != "Region") || Equals(e.OldValue, e.NewValue))
            return;
        if (!(DataContext is SubregionViewModel vm))
            return;

        var pg = (PropertyGrid)sender;
        var subregion = pg.SelectedObject as SegmentSubregion;
        if (subregion == null) return;
        var name = subregion.Name;
        var region = subregion.Region;
        string regionNewName = "", regionOldName = "";
        if (e.Property.Name == "Region")
        {
            regionNewName = vm.GetSegment()?.GetRegion((int)e.NewValue) is { Name: var newName }
                ? $"{subregion.Region}: {newName}"
                : $"Region {subregion.Region}";
            regionOldName = vm.GetSegment()?.GetRegion((int)e.OldValue) is { Name: var oldName }
                ? $"{subregion.Region}: {oldName}"
                : $"Region {subregion.Region}";
        }

        var expanded = vm.Groups.Groups
                            .Where(g => g.IsExpanded)
                            .Select(g => g.Name)
                            .ToHashSet();

        vm.Groups.Import(vm.Subregions);

        foreach (var grp in vm.Groups.Groups)
        {
            foreach (var ssr in grp.SegmentSubregions)
            {
                grp.IsSelected = false;
                if (ssr.Name == name)
                    ssr.IsSelected = true;
                grp.Debug = $"  ";
            }
            if (e.Property.Name == "Region")
            {
                if (grp.Name == regionNewName)
                    grp.IsExpanded = true;
                else if (grp.Name == regionOldName)
                    grp.IsExpanded = false;
                else
                    grp.IsExpanded = expanded.Contains(grp.Name);
            }
            else
                grp.IsExpanded = expanded.Contains(grp.Name);

        }
        CollectionViewSource.GetDefaultView(vm.Groups.Groups)
                            .Refresh();
    }

    private void OnSubregionChanged(SubregionDocument recipient, SubregionViewModel.SelectedSubregionChangedMessage message)
    {
        var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
        var segment = segmentRequest.Response;

        var subregion = message.Value;
        if (subregion != null)
        {
            _presenter.Region = segment.GetRegion(subregion.Region);
            _presenter.SetSubregion(subregion);
        }

        if (DataContext is not SubregionViewModel viewModel)
            return;
        if (message?.Value == null)
            viewModel.RefreshGroups();
    }


}

public class SubregionViewModel : ObservableRecipient
{
	public class SelectedSubregionChangedMessage : ValueChangedMessage<SegmentSubregion>
	{
		public SelectedSubregionChangedMessage(SegmentSubregion value) : base(value)
		{
		}
    }
    public class SegmentSubregionGroup : ObservableObject
    {
        public string Name { get; set; }
        public string SegmentSubregionName { get; set; }
        public ObservableCollection<SegmentSubregion> SegmentSubregions { get; set; } = new();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        private string _debug;
        public string Debug
        {
            get => _debug;
            set => SetProperty(ref _debug, value);
        }
    }
    public SegmentSubregionGroups Groups { get; private set; }
    public class SegmentSubregionGroups : ObservableObject
    {
        private readonly Segment _segment;

        public SegmentSubregionGroups(Segment segment)
        {
            _segment = segment;
        }

        public ObservableCollection<SegmentSubregionGroup> Groups { get; set; } = new();

        public void Import(IEnumerable<SegmentSubregion> subregions)
        {
            Groups.Clear();
            foreach (var subregion in subregions.OrderBy(s => s.Region))
            {
                string groupName = _segment.GetRegion(subregion.Region) is { Name: var name }
                    ? $"{subregion.Region}: {name}"
                    : $"Region {subregion.Region}";
                var group = Groups.FirstOrDefault(g => g.Name == groupName);
                if (group == null)
                {
                    group = new SegmentSubregionGroup { Name = groupName };
                    Groups.Add(group);
                }
                group.SegmentSubregions.Add(subregion);
            }
        }
    }

    private int _newSubregionCount = 1;
		
	public string Name => "(Subregions)";
		
	private Segment _segment;
    public Segment GetSegment() => _segment;

    private SegmentSubregion _selectedSubregion;
		
	public SegmentSubregion SelectedSubregion
	{
		get => _selectedSubregion;
		set
		{
			SetProperty(ref _selectedSubregion, value, true);
					
			if (value != null)
				WeakReferenceMessenger.Default.Send(new SelectedSubregionChangedMessage(value));
		}
	}
		
	public SegmentSubregions Subregions => _segment.Subregions;
    
    public RelayCommand AddSubregionCommand { get; private set; }
	public RelayCommand<SegmentSubregion> RemoveSubregionCommand { get; private set; }

	public RelayCommand ImportSubregionCommand { get; private set; }
	public RelayCommand<SegmentSubregion> ExportSubregionCommand { get; private set; }

	public SubregionViewModel(Segment segment)
	{
		_segment = segment ?? throw new ArgumentNullException(nameof(segment));

        Groups = new SegmentSubregionGroups(_segment);

        Groups.Import(Subregions);
        RefreshGroups();
        AddSubregionCommand = new RelayCommand(AddSubregion);
			
		RemoveSubregionCommand = new RelayCommand<SegmentSubregion>(RemoveSubregion, 
			(location) => SelectedSubregion != null);
		RemoveSubregionCommand.DependsOn(() => SelectedSubregion);

		ImportSubregionCommand = new RelayCommand(ImportSubregion);

		ExportSubregionCommand = new RelayCommand<SegmentSubregion>(ExportSubregion,
			(location) => SelectedSubregion != null);
		ExportSubregionCommand.DependsOn(() => SelectedSubregion);
    }
    public void RefreshGroups()
    {
        var expandedGroupNames = Groups.Groups
            .Where(g => g.IsExpanded)
            .Select(g => g.Name)
            .ToHashSet();

        CollectionViewSource.GetDefaultView(Groups.Groups).Refresh();

        foreach (var group in Groups.Groups)
        {
            group.IsExpanded = expandedGroupNames.Contains(group.Name);
        }
    }

    private void AddSubregion()
	{
		var newSubregion = new SegmentSubregion()
		{
			Name = $"Subregion {_newSubregionCount++}"
		};
			
		Subregions.Add(newSubregion);
		SelectedSubregion = newSubregion;

        Groups.Import(Subregions); //  Refresh group view
        RefreshGroups();
    }

    private void RemoveSubregion(SegmentSubregion subregion)
	{
		var result = MessageBox.Show($"Are you sure with to delete subregion '{subregion.Name}'?",
			"Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.No)
        {
            Subregions.Remove(subregion);
            Groups.Import(Subregions); //  Refresh group view
            RefreshGroups();
        }
    }

    private void ImportSubregion()
	{
		XDocument clipboard = null;
		try
		{
			clipboard = XDocument.Parse(Clipboard.GetText());
		}
		catch { }
		if (clipboard is null || clipboard.Root.Name.ToString() != "subregion")
			return;

		var newSubregion = new SegmentSubregion(clipboard.Root);
		Subregions.Add(newSubregion);
        Groups.Import(Subregions); //  Refresh group view
        RefreshGroups();
    }

	private void ExportSubregion(SegmentSubregion subregion)
	{
		Clipboard.SetText(subregion.GetXElement().ToString());
	}
}