using System.Linq;
using CommonServiceLocator;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.MVP
{
	public class VisibilityOptions : ObservableRecipient
	{
		private bool _openDoors;
		
		/// <summary>
		/// Gets or sets a value indicating whether to doors are open.
		/// </summary>
		public bool OpenDoors
		{
			get => _openDoors;
			set
			{
				SetProperty(ref _openDoors, value);

				 /* TODO: Maybe use weak reference messenger with like 'VisibilityUpdate'? */
				var applicationPresenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
				var graphicsService = ServiceLocator.Current.GetInstance<IGraphicsService>();

				if (applicationPresenter != null && applicationPresenter.Segment != null)
				{
					applicationPresenter.Segment.UpdateTiles();

					if (graphicsService != null)
					{
						foreach (var presenter in graphicsService.PresentationTargets.OfType<PresentationTarget>())
							presenter.InvalidateRender();
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VisibilityOptions"/> class.
		/// </summary>
		public VisibilityOptions()
		{
			OpenDoors = false;
		}
	}
}