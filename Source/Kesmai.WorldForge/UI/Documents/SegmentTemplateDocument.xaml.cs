using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Windows;

namespace Kesmai.WorldForge.UI.Documents;

public partial class SegmentTemplateDocument : UserControl
{
    private SegmentTemplateViewModel? _viewModel;

    public SegmentTemplateDocument()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = e.NewValue as SegmentTemplateViewModel;

        if (_viewModel != null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdateTemplate();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SegmentTemplateViewModel.Template))
            UpdateTemplate();
    }

    private void UpdateTemplate()
    {
        if (_viewModel is null)
            return;
        
        _presenter.Template = _viewModel.Template;

        if (_viewModel.Template is null)
            return;

        _presenter.Focus();
        _presenter.Invalidate();
    }

    private void OnAddComponentClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null || _viewModel.Template is null)
            return;

        var picker = new ComponentsWindow
        {
            Owner = Window.GetWindow(this)
        };

        var result = picker.ShowDialog();

        if (!result.HasValue || result != true || picker.SelectedComponent is null)
            return;

        _viewModel.Template.Providers.Add(picker.SelectedComponent);

        _providersList.SelectedItem = picker.SelectedComponent;
        _providersList.ScrollIntoView(picker.SelectedComponent);
        
        _presenter.Invalidate();
    }

    private void OnRemoveComponentClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null || _viewModel.Template is null)
            return;

        if (_providersList.SelectedItem is not IComponentProvider provider)
            return;

        _viewModel.Template.Providers.Remove(provider);
        
        _presenter.Invalidate();
    }
}

public class SegmentTemplateViewModel : ObservableRecipient
{
    private SegmentTemplate? _template;

    public string Name => "(Template)";

    public SegmentTemplate? Template
    {
        get => _template;
        set => SetProperty(ref _template, value);
    }
}
