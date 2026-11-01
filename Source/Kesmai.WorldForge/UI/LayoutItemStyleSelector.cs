using System.Windows;
using System.Windows.Controls;
using AvalonDock.Controls;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// Gives documents their style and leaves panels (Project, Properties, Problems) alone, so the
/// titles declared on the panels in XAML aren't replaced by the name of their content.
/// </summary>
public class LayoutItemStyleSelector : StyleSelector
{
	public Style DocumentStyle { get; set; }

	public override Style SelectStyle(object item, DependencyObject container)
	{
		if (container is LayoutDocumentItem)
			return DocumentStyle;

		return base.SelectStyle(item, container);
	}
}
