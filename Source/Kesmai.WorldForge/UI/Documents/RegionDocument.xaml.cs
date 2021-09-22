using System.Windows.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Documents
{
	public partial class RegionDocument : UserControl
	{
		public RegionDocument()
		{
			InitializeComponent();

			WeakReferenceMessenger.Default.Register<RegionDocument, JumpSegmentRegionLocation>(this, (r, m) => { MoveCamera(m); });
		}
		private void MoveCamera ( JumpSegmentRegionLocation target)
        {
			if (_presenter.Region.ID == target.Region)
            {
				if (_presenter.WorldScreen is not null)
					_presenter.WorldScreen.CenterCameraOn(target.X, target.Y);
            }
        }
	}
}