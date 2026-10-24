#nullable enable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// Property grid editor for a list of times of day kept as "08:00, 20:30": one box per time with a − button to remove
/// it, and a + button to add one.
/// </summary>
public class TimeListEditor : ITypeEditor
{
	public FrameworkElement ResolveEditor(PropertyItem propertyItem)
	{
		var panel = new StackPanel { Margin = new Thickness(0, 1, 0, 1) };

		void Write(System.Collections.Generic.IEnumerable<TimeSpan> times)
		{
			propertyItem.Value = SpawnTimeWindow.FormatTimes(times);
			Build();
		}

		void Build()
		{
			panel.Children.Clear();

			var times = SpawnTimeWindow.ParseTimes(propertyItem.Value as string);

			foreach (var time in times)
			{
				var row = new DockPanel { Margin = new Thickness(0, 0, 0, 2) };
				var remove = new Button { Content = "−", Width = 22, Margin = new Thickness(2, 0, 0, 0), ToolTip = "Remove this time" };
				var box = new TextBox { Text = time.ToString(@"hh\:mm"), VerticalContentAlignment = VerticalAlignment.Center };

				remove.Click += (_, _) => Write(times.Where(t => t != time));

				void Commit()
				{
					if (SpawnTimeWindow.ParseTime(box.Text) is TimeSpan typed)
					{
						if (typed != time)
							Write(times.Where(t => t != time).Append(typed));
					}
					else
					{
						// not a time: put back what it was
						box.Text = time.ToString(@"hh\:mm");
					}
				}

				box.LostKeyboardFocus += (_, _) => Commit();
				box.KeyDown += (_, e) =>
				{
					if (e.Key == Key.Enter)
						Commit();
				};

				DockPanel.SetDock(remove, Dock.Right);
				row.Children.Add(remove);
				row.Children.Add(box);
				panel.Children.Add(row);
			}

			var add = new Button { Content = "+", Width = 22, HorizontalAlignment = HorizontalAlignment.Left, ToolTip = "Add a time" };

			// a new time starts an hour after the last one
			add.Click += (_, _) =>
			{
				var next = times.Count == 0 ? new TimeSpan(12, 0, 0) : TimeSpan.FromMinutes((times[^1].TotalMinutes + 60) % (24 * 60));

				while (times.Contains(next))
					next = TimeSpan.FromMinutes((next.TotalMinutes + 15) % (24 * 60));

				Write(times.Append(next));
			};

			panel.Children.Add(add);

			if (times.Count == 0)
				panel.Children.Add(new TextBlock { Text = "No times yet: + adds one.", Foreground = Brushes.DimGray, FontSize = 11 });
		}

		Build();

		return panel;
	}
}
