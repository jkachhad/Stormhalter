using System;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge
{
	public class ArrowTool : Tool
	{
		private static ClipboardManager _clipboard = new ClipboardManager();
		
		private bool _isDragging;
		private Vector2F _dragStart;
		private Vector2F _dragDelta;

		private bool _isSelecting;
		
		private Vector2F _selectionStart;
		private Vector2F _selectionEnd;
		
		private bool _isMoving;
		private Vector2F _movingStart;
		private Vector2F _movingEnd;
		
		public ArrowTool() : base("Arrow", @"Editor-Icon-Arrow")
		{
		}
		
		public override void OnHandleInput(PresentationTarget target, IInputService inputService)
		{
			base.OnHandleInput(target, inputService);
			
			if (inputService.IsMouseOrTouchHandled)
				return;
			
			var currentPosition = inputService.MousePosition;
			
			var services = ServiceLocator.Current;
			var graphicsScreen = target.WorldScreen;
			var region = target.Region;
			
			var presenter = services.GetInstance<ApplicationPresenter>();
			var selection = presenter.Selection;
			
			if (!inputService.IsKeyboardHandled)
			{
				if (inputService.IsDown(Keys.LeftControl) || inputService.IsDown(Keys.RightControl))
				{
					if (inputService.IsReleased(Keys.C))
					{
						CopySelection();
						inputService.IsKeyboardHandled = true;
					}
					else if (inputService.IsReleased(Keys.V))
					{
						PasteSelection();
						graphicsScreen.InvalidateRender();
						inputService.IsKeyboardHandled = true;
					}
				}
			}

			var modify = (inputService.IsDown(Keys.RightShift) || inputService.IsDown(Keys.LeftShift));
			var (cx, cy) = graphicsScreen.ToWorldCoordinates((int)currentPosition.X, (int)currentPosition.Y);
			
			if (!modify)
			{
				_cursor = _selectNormal;
				
				if (selection.IsSelected(cx, cy, region))
					_cursor = _selectMove;
			}
			else
			{
				_cursor = _selectAdd;
			}

			if (inputService.IsReleased(MouseButtons.Right))
			{
				if (!_isDragging)
				{
					if (selection.Count > 0 && !selection.IsSelected(cx, cy, region))
					{
						selection.Clear();
						graphicsScreen.InvalidateRender();

						inputService.IsMouseOrTouchHandled = true;
					}
				}
				else
				{
					inputService.IsMouseOrTouchHandled = true;
				}
			}
			else if (inputService.IsDown(MouseButtons.Right))
			{
				if (_dragStart != Vector2F.Zero)
				{
					_dragDelta = graphicsScreen.CameraDrag = currentPosition - _dragStart;

					if (_dragDelta.Magnitude > 2.0f)
						_isDragging = true;

					inputService.IsMouseOrTouchHandled = true;
				}
				else
				{
					_dragStart = currentPosition;
				}
			}
			else if (inputService.IsDown(MouseButtons.Left))
			{
				if (!_isSelecting && !modify && selection.IsSelected(cx, cy, region) || _isMoving)
				{
					if (_isMoving)
						_movingEnd = currentPosition;
					else
						_movingStart = _movingEnd = currentPosition;
					
					_isMoving = true;
				}
				else
				{
					if (_isSelecting)
						_selectionEnd = currentPosition;
					else
						_selectionStart = _selectionEnd = currentPosition;

					_isSelecting = true;
				}

				inputService.IsMouseOrTouchHandled = true;
			}
			else
			{
				if (_isDragging)
				{
					var deltaX = (_dragDelta.X / (presenter.UnitSize * graphicsScreen.ZoomFactor));
					var deltaY = (_dragDelta.Y / (presenter.UnitSize * graphicsScreen.ZoomFactor));

					graphicsScreen.CameraLocation -= new Vector2F(deltaX, deltaY);
					graphicsScreen.CameraDrag = Vector2F.Zero;

					_isDragging = false;
					
					inputService.IsMouseOrTouchHandled = true;
				}
				
				_dragStart = Vector2F.Zero;
				_dragDelta = Vector2F.Zero;

				if (_isSelecting)
				{
					var (left, top) = graphicsScreen.ToWorldCoordinates((int)_selectionStart.X, (int)_selectionStart.Y);
					var (right, bottom) = graphicsScreen.ToWorldCoordinates((int)_selectionEnd.X, (int)_selectionEnd.Y);

					var width = Math.Max(1, Math.Abs(right - left) + 1);
					var height = Math.Max(1, Math.Abs(bottom - top) + 1);
					
					selection.Select(new Rectangle(
						(left < right ? left : right), 
						(top < bottom ? top : bottom), 
						width, height), 
						region, modify);
					
					_isSelecting = false;
					
					graphicsScreen.InvalidateRender();
					
					inputService.IsMouseOrTouchHandled = true;
				} 
				else if (_isMoving)
				{
					var (left, top) = graphicsScreen.ToWorldCoordinates((int)_movingStart.X, (int)_movingStart.Y);
					var (right, bottom) = graphicsScreen.ToWorldCoordinates((int)_movingEnd.X, (int)_movingEnd.Y);
					
					selection.Shift(right - left, bottom - top);
					
					_isMoving = false;
					
					graphicsScreen.InvalidateRender();
					
					inputService.IsMouseOrTouchHandled = true;
				}
				
				_selectionStart = Vector2F.Zero;
				_selectionEnd = Vector2F.Zero;
			}
			
			if (inputService.IsDoubleClick(MouseButtons.Left))
			{
				if (graphicsScreen.UI != null)
				{
					var (mx, my) = graphicsScreen.ToWorldCoordinates((int)_position.X, (int)_position.Y);
					var tile = region.GetTile(mx, my);

					if (tile != null)
					{
						var componentWindow = new ComponentsWindow(region, tile, graphicsScreen);
						
						selection.Clear();

						componentWindow.Show(graphicsScreen.UI);
						componentWindow.Center();
					}
					else
					{
						new RegionWindow(region).Show(graphicsScreen.UI);
					}
				}

				inputService.IsMouseOrTouchHandled = true;
			}
		}
		
		private static Color _selectionPreview = Color.FromNonPremultiplied(255, 255, 0, 100);
		private static Color _movingFill = Color.FromNonPremultiplied(100, 100, 255, 75);
		private static Color _movingBorder = Color.FromNonPremultiplied(100, 100, 255, 100);
		
		public override void OnRender(RenderContext context)
		{
			base.OnRender(context);
			
			var services = ServiceLocator.Current;
			var presenter = services.GetInstance<ApplicationPresenter>();
			
			var graphicsService = context.GraphicsService;
			var spriteBatch = graphicsService.GetSpriteBatch();
			
			var presentationTarget = context.GetPresentationTarget();
			
			var worldScreen = presentationTarget.WorldScreen;
			var viewRectangle = worldScreen.GetViewRectangle();
			
			if (_isSelecting)
			{
				var preview = new Rectangle((int)_selectionStart.X, (int)_selectionStart.Y, 
					(int)(_selectionEnd.X - _selectionStart.X), 
					(int)(_selectionEnd.Y - _selectionStart.Y));
					
				spriteBatch.DrawRectangle(preview, _selectionPreview);
			}
			else if (_isMoving)
			{
				var rectangles = presenter.Selection;

				foreach (var rectangle in rectangles)
				{
					if (!viewRectangle.Intersects(rectangle))
						continue;

					var ox = rectangle.Left - viewRectangle.Left;
					var oy = rectangle.Top - viewRectangle.Top;

					var x = (int)(ox * presenter.UnitSize);
					var y = (int)(oy * presenter.UnitSize);

					var width = (int)(rectangle.Width * presenter.UnitSize);
					var height = (int)(rectangle.Height * presenter.UnitSize);

					var sx = _movingEnd.X - _movingStart.X;
					var sy = _movingEnd.Y - _movingStart.Y;

					var bounds = new Rectangle((int)(x + sx), (int)(y + sy), width, height);

					spriteBatch.FillRectangle(bounds, _movingFill);
					spriteBatch.DrawRectangle(bounds, _movingBorder);
				}
			}
		}

		private Cursor _selectNormal;
		private Cursor _selectAdd;
		private Cursor _selectMove;
		
		public override void OnActivate()
		{
			base.OnActivate();
			var selectNormal = Application.GetResourceStream(
				new Uri(@"/Kesmai.WorldForge;component/Resources/Select-Normal.cur", UriKind.Relative));

			if (selectNormal != null)
				_selectNormal = new Cursor(selectNormal.Stream);
			else
				_selectNormal = Cursors.Arrow;
			
			var selectAdd = Application.GetResourceStream(
				new Uri(@"/Kesmai.WorldForge;component/Resources/Select-Add.cur", UriKind.Relative));

			if (selectAdd != null)
				_selectAdd = new Cursor(selectAdd.Stream);
			else
				_selectAdd = Cursors.Arrow;
			
			var selectMove = Application.GetResourceStream(
				new Uri(@"/Kesmai.WorldForge;component/Resources/Select-Move.cur", UriKind.Relative));

			if (selectMove != null)
				_selectMove = new Cursor(selectMove.Stream);
			else
				_selectMove = Cursors.Arrow;

			_cursor = _selectNormal;
		}

		public override void OnDeactivate()
		{
			_cursor = Cursors.Arrow;
			base.OnDeactivate();
		}

		public void CopySelection()
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var selection = presenter.Selection;

			if (selection != null && selection.Count > 0)
				_clipboard.Copy(selection);
		}
		
		public void PasteSelection()
		{
			var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
			var selection = presenter.Selection;

			if (selection != null && selection.Count > 0)
				_clipboard.Paste(selection);
		}
	}
}