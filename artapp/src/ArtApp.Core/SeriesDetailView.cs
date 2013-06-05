using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.Foundation;
using System.Linq;

namespace ArtApp
{
	public class SeriesDetailView : UIView
	{
		private const int _titleSize = 16;
		private const int followerSize = 13;
		private const int _yearsSize = 14;
		private const int urlSize = 14;
		private const int TextX = 95;
		
		private static UIFont _titleFont = UIFont.BoldSystemFontOfSize(_titleSize);
		private static UIFont followerFont = UIFont.SystemFontOfSize(followerSize);
		private static UIFont _yearsFont = UIFont.SystemFontOfSize(_yearsSize);
		private static CGPath borderPath = ImageHelper.MakeRoundedPath (75, 4);
		
		private UIImageView _artistView;
		private Series _series;
		
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_artistView != null)
				{
					_artistView.Image.Dispose();
					_artistView.Image = null;
					_artistView.Dispose();
					_artistView = null;
				}
			}
			base.Dispose (disposing);
		}
		
		public SeriesDetailView(IntPtr handle) : base(handle)
		{
			
		}
		
		public SeriesDetailView(Series series, RectangleF rect) : base (rect)
		{
			_series = series;
			BackgroundColor = UIColor.Clear;

			// Defer to latest twitter if it exists; otherwise, use embed
			var image = ImageCache.Get("latest_twitter");
			if(image == null)
			{
				var src = AppManifest.Current.Image;
				image = UIImageEx.FromIdiomBundle(src);
				
				var fresh = image;
				image = ImageHelper.RoundAndSquare(fresh, 14);
				fresh.Dispose();
			}						
						
			_artistView = new UIImageView(new RectangleF (10, 10, 73, 73));
			_artistView.Image = image;
			_artistView.BackgroundColor = UIColor.Clear;
			image.Dispose();
			
			AddSubview(_artistView);
		}

		public event NSAction PictureTapped;
		public event NSAction Tapped;
		
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			var touch = touches.AnyObject as UITouch;
			var location = touch.LocationInView(this);
			if (_artistView.Frame.Contains(location))
			{
				if (PictureTapped != null)
				{
					PictureTapped();
				}
			}
			else
			{
				if (Tapped != null)
				{
					Tapped();
				}	
			}
			base.TouchesBegan(touches, evt);
		}

		public override void Draw(RectangleF rect)
		{
			var w = rect.Width-TextX;
			var context = UIGraphics.GetCurrentContext();
			
			context.SaveState();
			context.SetFillColor(0, 0, 0, 1);
			context.SetShadowWithColor(new SizeF(0, -1), 1, UIColor.White.CGColor);
			
			// Set defaults
			var titleMaxWidth = 200;
			
			var size = _titleSize;
			var titleFont = UIFont.BoldSystemFontOfSize(size);
			var titleSize = new NSString(_series.Title).StringSize(titleFont);
			while(titleSize.Width > titleMaxWidth)
			{
				titleFont = UIFont.BoldSystemFontOfSize(size--);
				titleSize = new NSString(_series.Title).StringSize(titleFont);
			}
			var titleArea = new RectangleF(TextX, 12, titleSize.Width, titleSize.Height);
			
			
			DrawString(_series.Title, titleArea, _titleFont, UILineBreakMode.TailTruncation);
			DrawString(_series.Years, new RectangleF(TextX, 50, w, _yearsSize), _yearsFont, UILineBreakMode.TailTruncation);
			
			UIColor.DarkGray.SetColor();
			DrawString(_series.Pieces.Count + " pieces", new RectangleF (TextX, 34, w, followerSize), followerFont);

			context.RestoreState();
			context.TranslateCTM(9, 9);
			context.AddPath(borderPath);
			context.SetStrokeColor(0.5f, 0.5f, 0.5f, 1);
			context.SetLineWidth(0.5f);
			context.StrokePath();
		}
	}
}

