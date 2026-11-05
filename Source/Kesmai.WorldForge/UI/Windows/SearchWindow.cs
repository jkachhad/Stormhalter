using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Windows;

/// <summary>Searches the whole segment; clicking a result opens it, or goes to the tile.</summary>
public class SearchWindow : Window
{
	private readonly Segment _segment;
	private readonly TextBox _query;
	private readonly DataGrid _results;
	private readonly TextBlock _status;

	public SearchWindow(Segment segment)
	{
		_segment = segment;

		Title = "Search the segment";
		Width = 900;
		Height = 600;
		WindowStartupLocation = WindowStartupLocation.CenterOwner;

		var layout = new DockPanel { Margin = new Thickness(8) };

		var top = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
		var search = new Button { Content = "Search", Padding = new Thickness(10, 1, 10, 1), Margin = new Thickness(6, 0, 0, 0), IsDefault = true };
		search.Click += (_, _) => Run();
		DockPanel.SetDock(search, Dock.Right);
		top.Children.Add(search);

		_query = new TextBox { ToolTip = "A name, a body or item id, or any text in a script; a number matches as a whole number" };
		top.Children.Add(_query);
		DockPanel.SetDock(top, Dock.Top);
		layout.Children.Add(top);

		_status = new TextBlock { Margin = new Thickness(0, 6, 0, 0), Text = "Type a name, a body id or any text, then Enter. Double click a result to open it." };
		DockPanel.SetDock(_status, Dock.Bottom);
		layout.Children.Add(_status);

		_results = new DataGrid
		{
			AutoGenerateColumns = false,
			IsReadOnly = true,
			CanUserAddRows = false,
			SelectionMode = DataGridSelectionMode.Single,
			HeadersVisibility = DataGridHeadersVisibility.Column,
		};
		_results.Columns.Add(new DataGridTextColumn { Header = "Kind", Binding = new Binding(nameof(SegmentSearch.Hit.Kind)) });
		_results.Columns.Add(new DataGridTextColumn { Header = "Where", Binding = new Binding(nameof(SegmentSearch.Hit.Title)) });
		_results.Columns.Add(new DataGridTextColumn { Header = "Found", Binding = new Binding(nameof(SegmentSearch.Hit.Detail)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
		_results.MouseDoubleClick += (_, _) => Open(_results.SelectedItem as SegmentSearch.Hit);
		_results.KeyDown += (_, e) =>
		{
			if (e.Key == Key.Enter)
			{
				Open(_results.SelectedItem as SegmentSearch.Hit);
				e.Handled = true;
			}
		};
		layout.Children.Add(_results);

		Content = layout;

		Loaded += (_, _) => _query.Focus();
	}

	private void Run()
	{
		var hits = SegmentSearch.Find(_segment, _query.Text);

		_results.ItemsSource = hits;
		_status.Text = hits.Count == 0
			? $"Nothing in the segment matches \"{_query.Text.Trim()}\"."
			: $"{hits.Count} found: {string.Join(", ", hits.GroupBy(h => h.Kind).Select(g => $"{g.Count()} {g.Key.ToLowerInvariant()}{(g.Count() == 1 ? "" : "s")}"))}. Double click one to open it.";
	}

	private static void Open(SegmentSearch.Hit hit)
	{
		if (hit is null)
			return;

		var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

		// a tile: the region's view shows and selects it once it is up
		if (hit.Region is not null)
			presenter.PendingNavigation = new RegionNavigation(hit.Region, hit.X, hit.Y);

		hit.Target?.Present(presenter);
	}
}
