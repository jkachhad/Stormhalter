using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Editor.Assets;

namespace Kesmai.WorldForge.UI.Windows;

/// <summary>
/// Every entity in the segment in one table, with the values its OnSpawn sets. Click a header to sort (numbers sort as
/// numbers), type in a cell to change it, right click a row to open it.
/// </summary>
public class CreatureTableWindow : Window
{
	private readonly Segment _segment;
	private readonly DataGrid _grid;
	private readonly TextBox _filter;
	private readonly TextBlock _status;
	private readonly ComboBox _bulkColumn;
	private readonly ComboBox _bulkMode;
	private readonly TextBox _bulkAmount;
	private ListCollectionView _view;

	public CreatureTableWindow(Segment segment)
	{
		_segment = segment;

		Title = "Creature table";
		Width = 1150;
		Height = 650;
		WindowStartupLocation = WindowStartupLocation.CenterOwner;

		var layout = new DockPanel { Margin = new Thickness(8) };

		var top = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
		top.Children.Add(new TextBlock { Text = "Filter:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });

		var refresh = new Button { Content = "Reload", Padding = new Thickness(8, 1, 8, 1), Margin = new Thickness(6, 0, 0, 0), ToolTip = "Reads every entity's OnSpawn again" };
		refresh.Click += async (_, _) => await LoadAsync();
		DockPanel.SetDock(refresh, Dock.Right);
		top.Children.Add(refresh);

		_filter = new TextBox { ToolTip = "Shows the rows with this text in any column" };
		_filter.TextChanged += (_, _) => _view?.Refresh();
		top.Children.Add(_filter);

		DockPanel.SetDock(top, Dock.Top);
		layout.Children.Add(top);

		_status = new TextBlock { Margin = new Thickness(0, 6, 0, 0), TextWrapping = TextWrapping.Wrap };
		DockPanel.SetDock(_status, Dock.Bottom);
		layout.Children.Add(_status);

		// bulk edit: one change to a value of every selected row
		var bulk = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
		bulk.Children.Add(new TextBlock { Text = "Selected rows:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) });

		_bulkColumn = new ComboBox { Width = 130, Margin = new Thickness(0, 0, 6, 0) };
		_bulkColumn.Items.Add(new ComboBoxItem { Content = "Group", Tag = nameof(CreatureRow.Group) });

		foreach (var (property, header) in CreatureRow.Columns)
			_bulkColumn.Items.Add(new ComboBoxItem { Content = header, Tag = property });

		_bulkColumn.SelectedIndex = 3;
		bulk.Children.Add(_bulkColumn);

		_bulkMode = new ComboBox { Width = 110, Margin = new Thickness(0, 0, 6, 0), ItemsSource = CreatureRow.BulkModes, SelectedIndex = 0 };
		bulk.Children.Add(_bulkMode);

		_bulkAmount = new TextBox { Width = 120, Margin = new Thickness(0, 0, 6, 0), VerticalContentAlignment = VerticalAlignment.Center };
		bulk.Children.Add(_bulkAmount);

		var apply = new Button { Content = "Apply to selected", Padding = new Thickness(8, 1, 8, 1), ToolTip = "Ctrl or Shift click rows to pick several; Ctrl+A picks every row shown" };
		apply.Click += (_, _) => ApplyBulk();
		bulk.Children.Add(apply);

		DockPanel.SetDock(bulk, Dock.Bottom);
		layout.Children.Add(bulk);

		_grid = new DataGrid
		{
			AutoGenerateColumns = false,
			CanUserAddRows = false,
			CanUserDeleteRows = false,
			SelectionMode = DataGridSelectionMode.Extended,
			SelectionUnit = DataGridSelectionUnit.FullRow,
			HeadersVisibility = DataGridHeadersVisibility.All,
			FrozenColumnCount = 1,
		};

		_grid.Columns.Add(Column("Entity", nameof(CreatureRow.EntityName), false));
		_grid.Columns.Add(Column("Group", nameof(CreatureRow.Group), false));
		_grid.Columns.Add(Column("Class", nameof(CreatureRow.CreatureType), true));

		foreach (var (property, header) in CreatureRow.Columns)
			_grid.Columns.Add(Column(header, $"[{property}]", false, property));

		_grid.Columns.Add(Column("Note", nameof(CreatureRow.Problem), true));

		_grid.Sorting += OnSorting;
		_grid.BeginningEdit += (_, e) =>
		{
			// OnSpawn values of a row the builder can't write stay as they are
			if (e.Row.Item is CreatureRow { CanEdit: false } && e.Column.SortMemberPath.StartsWith("["))
				e.Cancel = true;
		};

		var open = new MenuItem { Header = "Open in the Entities editor" };
		open.Click += (_, _) =>
		{
			if (_grid.SelectedItem is CreatureRow row)
				Open(row);
		};
		_grid.ContextMenu = new ContextMenu { Items = { open } };

		layout.Children.Add(_grid);

		Content = layout;

		Loaded += async (_, _) => await LoadAsync();
	}

	private static DataGridTextColumn Column(string header, string path, bool readOnly, string sortPath = null)
		=> new()
		{
			Header = header,
			IsReadOnly = readOnly,
			SortMemberPath = sortPath is null ? path : $"[{sortPath}]",
			Binding = new Binding(path) { Mode = readOnly ? BindingMode.OneWay : BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
		};

	private async System.Threading.Tasks.Task LoadAsync()
	{
		_status.Text = "Reading every creature's OnSpawn...";

		var catalog = await AssetCatalog.LoadAsync();
		var rows = CreatureRow.For(_segment, catalog);

		_view = new ListCollectionView(rows) { Filter = Matches };
		_grid.ItemsSource = _view;

		var unreadable = rows.Count(r => !r.CanEdit);

		_status.Text = $"{rows.Count} entities. Type in a cell to change it (Enter or leave the cell to write it to OnSpawn). "
			+ (unreadable > 0 ? $"{unreadable} can't have their OnSpawn values changed here; the Note column says why. " : "")
			+ "Click a header to sort; right click a row to open it.";
	}

	private void ApplyBulk()
	{
		var rows = _grid.SelectedItems.OfType<CreatureRow>().ToList();
		var property = (_bulkColumn.SelectedItem as ComboBoxItem)?.Tag as string;
		var header = (_bulkColumn.SelectedItem as ComboBoxItem)?.Content;
		var mode = _bulkMode.SelectedItem as string;
		var amount = _bulkAmount.Text.Trim();

		if (rows.Count == 0 || property is null || mode is null)
		{
			_status.Text = "Pick rows first (Ctrl or Shift click, or Ctrl+A for every row shown).";
			return;
		}

		if (amount.Length == 0 && mode != CreatureRow.BulkModes[0])
		{
			_status.Text = $"Type the amount to {mode.ToLowerInvariant()}.";
			return;
		}

		if (MessageBox.Show(this, $"{mode} {amount} for {header} on {rows.Count} entit{(rows.Count == 1 ? "y" : "ies")}?", "Bulk edit",
			    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
			return;

		// finish a cell being typed in first, so it doesn't write over the bulk change
		_grid.CommitEdit(DataGridEditingUnit.Row, true);

		var changed = 0;
		var skipped = 0;

		foreach (var row in rows)
		{
			var isGroup = property == nameof(CreatureRow.Group);

			if (!isGroup && !row.CanEdit)
			{
				skipped++;
				continue;
			}

			var current = isGroup ? row.Group : row[property];
			var value = CreatureRow.Bulk(current, mode, amount);

			if (value is null)
			{
				skipped++;
				continue;
			}

			if (value == current)
				continue;

			if (isGroup)
				row.Group = value;
			else
				row[property] = value;

			changed++;
		}

		_view.Refresh();
		_status.Text = $"Changed {header} on {changed} entit{(changed == 1 ? "y" : "ies")}."
			+ (skipped > 0 ? $" {skipped} skipped (no number there to change, or an OnSpawn this table can't write)." : "");
	}

	private bool Matches(object item)
	{
		var text = _filter.Text.Trim();

		if (text.Length == 0 || item is not CreatureRow row)
			return true;

		IEnumerable<string> values = [row.EntityName, row.Group, row.CreatureType, row.Problem, .. CreatureRow.Columns.Select(c => row[c.Property])];

		return values.Any(v => v?.Contains(text, StringComparison.OrdinalIgnoreCase) == true);
	}

	/// <summary>Sorts numbers as numbers, so 100 comes after 20.</summary>
	private void OnSorting(object sender, DataGridSortingEventArgs e)
	{
		if (_view is null)
			return;

		e.Handled = true;

		var direction = e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

		foreach (var column in _grid.Columns)
			column.SortDirection = null;

		e.Column.SortDirection = direction;

		var path = e.Column.SortMemberPath;
		Func<CreatureRow, string> value = path.StartsWith("[")
			? row => row[path.Trim('[', ']')]
			: row => typeof(CreatureRow).GetProperty(path)?.GetValue(row) as string ?? "";

		_view.CustomSort = new RowComparer(value, direction == ListSortDirection.Descending);
	}

	private sealed class RowComparer(Func<CreatureRow, string> value, bool descending) : IComparer
	{
		public int Compare(object x, object y)
		{
			// C# number separators (90_228_281) read as the number
			var a = value((CreatureRow)x).Replace("_", "");
			var b = value((CreatureRow)y).Replace("_", "");

			var bothNumbers = double.TryParse(a, NumberStyles.Float, CultureInfo.InvariantCulture, out var na)
				& double.TryParse(b, NumberStyles.Float, CultureInfo.InvariantCulture, out var nb);

			var order = bothNumbers ? na.CompareTo(nb) : string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

			// empty cells go last either way
			if (string.IsNullOrEmpty(a) != string.IsNullOrEmpty(b))
				return string.IsNullOrEmpty(a) ? 1 : -1;

			return descending ? -order : order;
		}
	}

	private static void Open(CreatureRow row)
	{
		var presenter = CommonServiceLocator.ServiceLocator.Current.GetInstance<ApplicationPresenter>();

		row.Entity.Present(presenter);
	}
}
