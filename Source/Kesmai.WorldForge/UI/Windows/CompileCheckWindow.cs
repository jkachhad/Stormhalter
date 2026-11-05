using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Roslyn;

namespace Kesmai.WorldForge.UI.Windows;

/// <summary>Test-compiles every script in the segment and lists the errors; double click one to open its owner.</summary>
public class CompileCheckWindow : Window
{
	private readonly Segment _segment;
	private readonly DataGrid _results;
	private readonly TextBlock _status;
	private readonly Button _again;
	private System.Collections.Generic.List<Microsoft.CodeAnalysis.MetadataReference> _references;

	public CompileCheckWindow(Segment segment)
	{
		_segment = segment;

		Title = "Compile check";
		Width = 1000;
		Height = 560;
		WindowStartupLocation = WindowStartupLocation.CenterOwner;

		var layout = new DockPanel { Margin = new Thickness(8) };

		var top = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
		_again = new Button { Content = "Check again", Padding = new Thickness(10, 1, 10, 1) };
		_again.Click += async (_, _) => await RunAsync();
		DockPanel.SetDock(_again, Dock.Right);
		top.Children.Add(_again);

		_status = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
		top.Children.Add(_status);

		DockPanel.SetDock(top, Dock.Top);
		layout.Children.Add(top);

		_results = new DataGrid
		{
			AutoGenerateColumns = false,
			IsReadOnly = true,
			CanUserAddRows = false,
			SelectionMode = DataGridSelectionMode.Single,
			HeadersVisibility = DataGridHeadersVisibility.Column,
		};
		_results.Columns.Add(new DataGridTextColumn { Header = "In", Binding = new Binding(nameof(CompileCheck.Problem.Owner)) });
		_results.Columns.Add(new DataGridTextColumn { Header = "Script", Binding = new Binding(nameof(CompileCheck.Problem.Script)) });
		_results.Columns.Add(new DataGridTextColumn { Header = "Line", Binding = new Binding(nameof(CompileCheck.Problem.Line)) });
		_results.Columns.Add(new DataGridTextColumn { Header = "Error", Binding = new Binding(nameof(CompileCheck.Problem.Code)) });
		_results.Columns.Add(new DataGridTextColumn { Header = "Message", Binding = new Binding(nameof(CompileCheck.Problem.Message)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
		_results.MouseDoubleClick += (_, _) =>
		{
			if (_results.SelectedItem is CompileCheck.Problem { Target: { } target })
				target.Present(ServiceLocator.Current.GetInstance<ApplicationPresenter>());
		};
		layout.Children.Add(_results);

		Content = layout;

		Loaded += async (_, _) => await RunAsync();
	}

	private async Task RunAsync()
	{
		_again.IsEnabled = false;
		_status.Text = "Compiling every script against the server's reference package...";

		try
		{
			// scripts typed into an open editor go to the model first, as saving does
			WeakReferenceMessenger.Default.Send(new SegmentSerialize(_segment));

			var references = _references ??= await CompileCheck.ReferencesAsync();
			var problems = await Task.Run(() => CompileCheck.Run(_segment, references));

			_results.ItemsSource = problems;

			var scripts = problems.Where(p => p.Target is not null).Select(p => (p.Owner, p.Script)).Distinct().Count();

			_status.Text = problems.Count == 0
				? "No errors: every script compiles against the server's reference package."
				: $"{problems.Count} error{(problems.Count == 1 ? "" : "s")}" + (scripts > 0 ? $" in {scripts} script{(scripts == 1 ? "" : "s")}" : "")
				  + ". Double click one to open it. Lines count from the first line of the script.";
		}
		catch (Exception exception)
		{
			_status.Text = $"The check couldn't run: {exception.Message}";
		}
		finally
		{
			_again.IsEnabled = true;
		}
	}
}
