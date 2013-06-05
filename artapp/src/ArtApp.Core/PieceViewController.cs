using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace ArtApp
{
	public class PieceViewController : UIViewController
	{
		// http://stackoverflow.com/questions/8754124/uiimageview-uiimage-memory-tag-70-release-timing-when-scrolling
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_galleryImageView != null)
				{
					UIImage image = _galleryImageView.Image;
					_galleryImageView.Image = null;
					image.Dispose();
					_galleryImageView.RemoveFromSuperview();
					_galleryImageView.Dispose();
					_galleryImageView = null;
				}
				
				if(_pdvc != null)
				{
					if(_pdvc.View != null && _pdvc.View.Superview != null)
					{
						_pdvc.View.RemoveFromSuperview();
					}						
					_pdvc.Dispose();
					_pdvc = null;
				}
			}
			base.Dispose(disposing);
		}
		
		private Piece _piece;
		private RectangleF _frame;
		private SeriesViewController _parent;
		private GalleryImageView _galleryImageView;
		
		public Piece Piece
		{
			get
			{
				return _piece;		
			}
		}
		
		public PieceViewController(Piece piece, RectangleF frame, SeriesViewController parent, GalleryImageView galleryImageView)
		{
			_piece = piece;
			_frame = frame;
			_parent = parent;
			_galleryImageView = galleryImageView;
		}
		
		private PieceDetailViewController _pdvc;
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			var touch = touches.AnyObject as UITouch;
			var location = touch.LocationInView (this.View);
			if (_frame.Contains(location))
			{
				// Piece tapped			
				var old = _pdvc;
				_pdvc = new PieceDetailViewController(_piece, _parent);
				_parent.NavigationController.PresentSemiModalViewController(_pdvc);
				if(old != null)
				{
					old.Dispose();
				}
			}
			base.TouchesBegan(touches, evt);
		}
		
		public bool LightsOn
		{
			get
			{
				return _galleryImageView.Lights.On;	
			}
		}
		
		public bool ToggleLights()
		{
			if(_galleryImageView.Lights.On)
			{
				_galleryImageView.Lights.TurnOff();
			}
			else
			{
				_galleryImageView.Lights.TurnOn();	
			}
			return _galleryImageView.Lights.On;
		}
	}
}

