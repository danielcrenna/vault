using System;
using MonoTouch.UIKit;
using MonoTouch.CoreAnimation;
using System.Drawing;
using MonoTouch.MapKit;

namespace ArtApp
{
	// http://cocoawithlove.com/2009/08/adding-shadow-effects-to-uitableview.html
	public class CustomTableView : UITableView
	{
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_dropShadowLayer != null)
				{
					_dropShadowLayer.Contents.Dispose();
					_dropShadowLayer.Contents = null;
					_dropShadowLayer.Dispose ();
					_dropShadowLayer = null;
				}
			}
			base.Dispose (disposing);
		}
		private CALayer _dropShadowLayer;
		
		public CustomTableView(RectangleF bounds, UITableViewStyle style) : base(bounds, style)
		{
			ApplyDropShadow();
			
			// Necessary because we are using a transparent navigation bar setting
			// http://stackoverflow.com/questions/2339620/uitableview-add-content-offset-at-top
			ContentInset = new UIEdgeInsets(44, 0, 0, 0);
		}
						
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			
			this.ContentInset = new UIEdgeInsets(Util.IsLandscape() ? 32 : 44, 0, 0, 0);
						
			// Keep drop shadow layer fixed when child views are scrolled
			CATransaction.Begin();
			CATransaction.DisableActions = true;
			
			SetDropShadowFrame();
			RectangleF originFrame = _dropShadowLayer.Frame;
			originFrame.Size.Width = this.Frame.Size.Width;
			originFrame.Y = this.ContentOffset.Y;
			_dropShadowLayer.Frame = originFrame;
			
			CATransaction.Commit();
		}
		
		private void SetDropShadowFrame()
		{
			RectangleF frame;
			if(Util.IsPad())
			{
				if(Util.IsPortrait())
				{
					frame = new RectangleF(0, 44, 768, 44);
				}
				else
				{
					frame = new RectangleF(0, 44, 1024, 44);	
				}				
			}
			else
			{
				// iPhone Portrait
				if(Util.IsPortrait())
				{
					frame = new RectangleF(0, 44, 320, 44);
				}
				else
				{
					frame = new RectangleF(0, 32, 480, 32);
				}				
			}			
			
			_dropShadowLayer.Frame = frame;
		}

		private void ApplyDropShadow()
		{
			var barImage = UIImage.FromBundle("Images/opaqueBar.png");
			
			// http://stackoverflow.com/questions/1304017/drawing-woes-with-calayer
			if(_dropShadowLayer != null)
			{
				_dropShadowLayer.Contents.Dispose();
				_dropShadowLayer.Contents = null;
				_dropShadowLayer.Dispose ();
				_dropShadowLayer = null;
			}
			_dropShadowLayer = new CALayer();
			_dropShadowLayer.MasksToBounds = false;
			_dropShadowLayer.ShadowColor = UIColor.DarkGray.CGColor;
			_dropShadowLayer.ShadowOpacity = 1.0f;
			_dropShadowLayer.ShadowRadius = 4.0f;
			_dropShadowLayer.ShadowOffset = new SizeF(0f, 1f);
			
			var cgImage = barImage.CGImage;
			_dropShadowLayer.Contents = barImage.CGImage;
						
			SetDropShadowFrame();
			
			ClipsToBounds = false;			
			Layer.AddSublayer(_dropShadowLayer);

			barImage.Dispose();
			cgImage.Dispose();
		}
	}
}

