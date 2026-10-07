using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI;

public partial class ProblemsPanel : UserControl
{
	public ProblemsPanel()
	{
		InitializeComponent();
	}

	private void OnProblemDoubleClick(object sender, MouseButtonEventArgs args)
	{
		// only rows carry a problem; double-clicking a header or empty space does nothing.
		var dataContext = args.OriginalSource switch
		{
			FrameworkElement element => element.DataContext,
			FrameworkContentElement element => element.DataContext,
			_ => null,
		};

		if (dataContext is not SegmentProblem problem)
			return;

		if (DataContext is ApplicationPresenter presenter && presenter.GoToProblemCommand.CanExecute(problem))
			presenter.GoToProblemCommand.Execute(problem);
	}
}
