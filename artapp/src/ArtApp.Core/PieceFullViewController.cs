using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;
using MonoTouch.CoreAnimation;

namespace ArtApp
{
	public class PieceFullViewController : UIViewController
	{
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_view != null)
				{
					_view.Dispose();
					_view = null;
				}
				if(_backgroundView != null)
				{
					_backgroundView.Image.Dispose();
					_backgroundView.Image = null;
					_backgroundView.Dispose();
					_backgroundView = null;
				}
			}
			base.Dispose(disposing);
		}
		
		private PieceFullView _view;
		
		private void SetMinimumZoomForCurrentFrame()
		{
			UIImageView imageView = (UIImageView)this._view.ChildView;
			
			var navBarHeight = Util.IsPortrait() ? 44 : 48;
			var imageSize = imageView.Image.Size;
			var scrollSize = Util.IsPortrait() ? new SizeF(320, 480 - 20) : new SizeF(480, 320 - 20);
			
			var widthRatio = (scrollSize.Width) / imageSize.Width;
			var heightRatio = (scrollSize.Height) / (imageSize.Height + navBarHeight + 20);
			
			var ratio = (widthRatio > heightRatio) ? heightRatio : widthRatio;
			var minimumZoom = Math.Min(1.0f, ratio);
			
			_view.MinimumZoomScale = minimumZoom;
		}
		
		private Piece _piece;
		private SeriesViewController _parent;
		
		private UIInterfaceOrientation _orientation;
		public PieceFullViewController(Piece piece, SeriesViewController parent)
		{
			_piece = piece;	
			_parent = parent;
			_orientation = UIApplication.SharedApplication.StatusBarOrientation;
		}
		
		public PieceFullViewController(IntPtr handle) : base(handle)
		{
			
		}
		
		private void SetMinimumZoomForCurrentFrameAndAnimateIfNecessary()
		{
			var wasAtMinimumZoom = false;
		
			if(_view.ZoomScale == _view.MinimumZoomScale)
			{
				wasAtMinimumZoom = true;
			}
			
			SetMinimumZoomForCurrentFrame();
			
			if(wasAtMinimumZoom || _view.ZoomScale < _view.MinimumZoomScale)
			{
				_view.SetZoomScale(_view.MinimumZoomScale, true);
			}	
		}
		
		private UIImageView _backgroundView;		
		private void SetBackgroundView(bool isLandscape)
		{
			if(_backgroundView != null)
			{
				_backgroundView.Image.Dispose();
				_backgroundView.Image = null;
			}
			
			var bg = isLandscape ? UIImageEx.FromIdiomBundleForBackground("Images/backgrounds/leather.jpg", true) :
								   UIImageEx.FromIdiomBundleForBackground("Images/backgrounds/leather.jpg", false);
			
			_backgroundView = _backgroundView ?? new UIImageView(bg);
			_backgroundView.Image = bg;
			_backgroundView.Frame = new RectangleF(0, 0, bg.Size.Width, bg.Size.Height);
			_backgroundView.UserInteractionEnabled = true;
			if(_backgroundView.Superview == null)
			{
				View.Add(_backgroundView);	
			}			
			bg.Dispose();
		}
				
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			AppDelegate.NavigationBar.SetBackButtonOn(this);
			
			SetBackgroundView(Util.IsLandscape());
			
			if(_view != null)
			{
				_view.Dispose();
				_view = null;
			}
			
			// Set up our custom ScrollView
			_view = new PieceFullView(View.Bounds);
			_view.BackgroundColor = UIColor.Clear;
			_view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_view.ShowsVerticalScrollIndicator = false;
			_view.ShowsHorizontalScrollIndicator = false;
			_view.BouncesZoom = true;
			_view.Delegate = new ScrollViewDelegate();
			
			// Added by me (snap back does not occur on device)
			_view.UserInteractionEnabled = true;
			_view.MultipleTouchEnabled = true;
			_view.ScrollsToTop = false;
			_view.PagingEnabled = false;
			
			View.AddSubview(_view);
			
			// Set up the ImageView that's going inside our scroll view
			UIImage image = UIImageEx.FromFile(_piece.Source);
			UIImageView iv = new UIImageView(image);
			image.Dispose();
			
			iv.Layer.ShadowPath = UIBezierPath.FromRect(iv.Bounds).CGPath;
			iv.Layer.ShouldRasterize = true;
			iv.Layer.MasksToBounds = false;
			iv.Layer.ShadowColor = UIColor.Black.CGColor;
			iv.Layer.ShadowOpacity = 1.0f;
			iv.Layer.ShadowRadius = 10.0f;
			iv.Layer.ShadowOffset = new SizeF(0f, 1f);
			
			// Finish the ScrollView setup
			_view.ContentSize = iv.Frame.Size;
			_view.SetChildView(iv);
			
			_view.MaximumZoomScale = 2.0f;
			SetMinimumZoomForCurrentFrame();			
			_view.SetZoomScale(_view.MinimumZoomScale, false);
		}
		
		public class ScrollViewDelegate : UIScrollViewDelegate
		{
			public override UIView ViewForZoomingInScrollView (UIScrollView scrollView)
			{
				return ((PieceFullView)scrollView).ChildView;
			}
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		private bool _rotatedBack = false;
		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate (toInterfaceOrientation, duration);
			
			if(_orientation == toInterfaceOrientation)
			{
				_rotatedBack = true;	
			}
			
			switch(toInterfaceOrientation)
			{
				case UIInterfaceOrientation.LandscapeLeft:	
				case UIInterfaceOrientation.LandscapeRight:
					SetBackgroundView(true);
					break;
				case UIInterfaceOrientation.Portrait:
				case UIInterfaceOrientation.PortraitUpsideDown:
					SetBackgroundView(false);
					break;
			}			
		}
		
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			SetMinimumZoomForCurrentFrameAndAnimateIfNecessary();

			if(_rotatedBack)
			{
				_parent.PendingRotate = 0;
				_rotatedBack = false;
			}
			else
			{
				_parent.PendingRotate = fromInterfaceOrientation;			
			}
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear(animated);
		}
				
		public override void ViewDidUnload()
		{
			base.ViewDidUnload();
			if(_view != null)
			{
				_view.Dispose();
				_view = null;			
			}
		}
	}
}

