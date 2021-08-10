using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge
{
	public class Selection : List<Rectangle>
	{
		public SegmentRegion Region { get; set; }
		
		public int SurfaceArea => this.Sum((rectangle) => (rectangle.Width * rectangle.Height));
		
		public Selection()
		{
		}
		
		public bool IsSelected(int x, int y, SegmentRegion region)
		{
			if (region != Region)
				return false;
			
			foreach (var rectangle in this)
				if (rectangle.Contains(x, y))
					return true;

			return false;
		}
		
		public void Select(Rectangle selection, SegmentRegion region, bool modifySelection = false)
		{
			if (!modifySelection || region != Region)
				Clear();

			Add(selection);
			
			if (modifySelection && Count > 1)
				Clean();
			
			Region = region;
		}

		public void Shift(int dx, int dy)
		{
			var updated = new List<Rectangle>();

			foreach (var rectangle in this)
				updated.Add(new Rectangle(rectangle.X + dx, rectangle.Y + dy, rectangle.Width, rectangle.Height));

			Clear();
			
			AddRange(updated);
		}

		private void Clean()
		{
			var oldRectangles = this.ToList();

			Clear();

			foreach (var oldRectangle in oldRectangles)
			{
				var overlaps = false;

				foreach (var overlapRectangle in oldRectangles)
				{
					if (oldRectangle != overlapRectangle && overlapRectangle.Contains(oldRectangle))
						overlaps = true;
				}

				if (overlaps)
					continue;

				Add(oldRectangle);
			}
		}
	}
}