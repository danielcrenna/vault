using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ArtApp
{
	public class PieceFullView : UIScrollView
	{
		private bool _suspendOffset = false;
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				_suspendOffset = true;
				if(_childView != null)
				{
					_childView.Image.Dispose();
					_childView.Image = null;
					_childView.Dispose();
					_childView = null;
				}
			}
			base.Dispose (disposing);
		}
		
		private UIImageView _childView;
		
		public UIImageView ChildView
		{
			get	
			{
				return _childView;	
			}
		}
		
		public PieceFullView(RectangleF rect) : base(rect)
		{
			
		}
		
		public PieceFullView(IntPtr handle) : base(handle)
		{
			
		}
		
		public PieceFullView(UIImageView aChildView)
		{
			SetChildView(aChildView);		
		}
		
		public PieceFullView(RectangleF rect, UIImageView aChildView) : base(rect)
		{
			SetChildView(aChildView);		
		}
		
		public void SetChildView(UIImageView aChildView)
		{
			if (_childView != aChildView)
			{
				if(_childView != null)
				{
					_childView.RemoveFromSuperview();
		        	_childView.Dispose();
		        }
				_childView = aChildView;

				base.AddSubview(_childView);

				SetContentOffset(PointF.Empty, false);
		    }
		}
					
		public override PointF ContentOffset
		{
			get
			{
				return base.ContentOffset;
			}
			set
			{
				if(_suspendOffset)
				{
					return;	
				}
				var anOffset = value;
				
				if(_childView != null)
				{
					var zoomViewSize = _childView.Frame.Size;
					var scrollViewSize = this.Bounds.Size;
							
					if(zoomViewSize.Width < scrollViewSize.Width)
					{
						anOffset.X = -(scrollViewSize.Width - zoomViewSize.Width) / 2.0f;
					}
								
					var navBarOffset = Util.IsPortrait() ? 22 : 16;
					
					if(zoomViewSize.Height < scrollViewSize.Height)
					{
						anOffset.Y = (-(scrollViewSize.Height - zoomViewSize.Height) / 2.0f) - navBarOffset;
					}
				}
				
				base.ContentOffset = anOffset;
			}
		}
	}
}

