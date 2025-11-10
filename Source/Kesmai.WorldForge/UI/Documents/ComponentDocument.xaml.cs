using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.AvalonEdit;

namespace Kesmai.WorldForge.UI.Documents;

public partial class ComponentDocument : UserControl
{
	private ComponentViewModel? _viewModel;
	
	public ComponentDocument()
	{
		InitializeComponent();

		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (_viewModel != null)
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = e.NewValue as ComponentViewModel;

		if (_viewModel != null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		UpdateComponent();
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ComponentViewModel.Component))
			UpdateComponent();
	}

	private void UpdateComponent()
	{
		_error.Visibility = Visibility.Hidden;
		_error.Text = String.Empty;
		
		_editor.Text = String.Empty;
		
		if (_viewModel is null || _viewModel.Component is null)
			return;

		if (_viewModel.Component.Element is not null)
		{
			var editorValue = _viewModel.Component.Element.ToString();

			if (!String.IsNullOrEmpty(editorValue))
				_editor.Text = editorValue;
		}

		_presenter.Provider = _viewModel.Component;
	}

	private void OnEditorChanged(object sender, EventArgs args)
	{
		if (sender is not TextEditor editor)
			return;

		if (_viewModel is null || _viewModel.Component is null)
			return;
		
		try
		{
			_viewModel.Component.UpdateComponent(XElement.Parse(editor.Text));
			
			_error.Visibility = Visibility.Hidden;
			_error.Text = String.Empty;
		}
		catch (Exception exception)
		{
			_error.Visibility = Visibility.Visible;
			_error.Text = exception.Message;
		}
	}
}

public class ComponentViewModel : ObservableRecipient
{
	private SegmentComponent? _component;

	public string Name => "(Component)";

	public SegmentComponent? Component
	{
		get => _component;
		set => SetProperty(ref _component, value);
	}
}
