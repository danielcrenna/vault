using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using System.IO;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;

namespace ArtApp
{
	public class CustomDialogViewController : DialogViewController
	{
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_imageView != null)
				{
					_imageView.Image.Dispose();
					_imageView.Image = null;
					_imageView.Dispose();
					_imageView = null;
				}
			}
			base.Dispose (disposing);
		}
		
		private string _background;
		private UIImageView _imageView;
		
		public CustomDialogViewController(IntPtr handle) : base(handle)
		{
			
		}
		
		public CustomDialogViewController(string background) : this(background, null)
		{
			
		}
		
		public CustomDialogViewController(string background, RootElement root) :  base(root)
		{
			_background = background;
		}

		public CustomDialogViewController(string background, RootElement root, bool pushing) : base(root, pushing)
		{
			_background = background;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);			
		}
		
		public override void ViewDidLoad()
		{
			SetBackgroundImage();
		}

		private void SetBackgroundImage()
		{
			UIImageView toDispose = null;
			if(_imageView != null)
			{
				toDispose = _imageView;
			}
			
			var image = UIImageEx.FromIdiomBundleForBackground(_background); // Cached			
			
			_imageView = new UIImageView(image);
			_imageView.Frame = new RectangleF (0, 0, this.View.Frame.Width, this.View.Frame.Height);
			_imageView.UserInteractionEnabled = true;
			this.TableView.BackgroundView = _imageView;
			
			if(toDispose != null)
			{
				toDispose.Image.Dispose();
				toDispose.Image = null;
				toDispose.Dispose();
				toDispose = null;	
			}
		}
		
		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate (toInterfaceOrientation, duration);
		}
		
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			SetBackgroundImage();
			base.DidRotate(fromInterfaceOrientation);
		}
	}
}