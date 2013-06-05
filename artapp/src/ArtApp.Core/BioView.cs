using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace ArtApp
{
	public class BioView : UIView
	{
		private UIActivityIndicatorView _spinner;
		
		private const int userSize = 19;
		private const int followerSize = 13;
		private const int locationSize = 14;
		private const int _urlSize = 14;
		
		private static readonly int _left = Util.IsPad() ? 95 + 73 : 95;
		
		private static UIFont _userFont = UIFont.BoldSystemFontOfSize (userSize);
		private static UIFont _followerFont = UIFont.SystemFontOfSize (followerSize);
		private static UIFont _locationFont = UIFont.SystemFontOfSize (locationSize);
		private static CGPath _borderPath = ImageHelper.MakeRoundedPath(DimensionSet.BioImageSquareSize + 2, 4);
		
		private UIImageView _profile;
		private UIButton url;
		private Bio _bio;
		
		public BioView(Bio bio, RectangleF frame, bool discloseButton) : base (frame)
		{
			_bio = bio;
			BackgroundColor = UIColor.Clear;
			
			AddDefaultImage(_bio);
			AddLoadingSpinner();
			AddOrUpdateBio(_bio);			
			AddSiteUrl(frame);
		}

		private void AddLoadingSpinner()
		{
			var box = new RectangleF(10, 10, 73, 73);
			_spinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			_spinner.Frame = box;
			_spinner.HidesWhenStopped = true;
			AddSubview(_spinner);
		}

		public void AddSiteUrl(RectangleF frame)
		{
			url = UIButton.FromType(UIButtonType.Custom);
			url.LineBreakMode = UILineBreakMode.TailTruncation;
			url.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
			url.TitleShadowOffset = new SizeF(0, 1);
			url.SetTitleColor(UIColor.FromRGB (0x32, 0x4f, 0x85), UIControlState.Normal);
			url.SetTitleColor(UIColor.Red, UIControlState.Highlighted);
			url.SetTitleShadowColor(UIColor.White, UIControlState.Normal);
			url.AddTarget(delegate { if (UrlTapped != null) UrlTapped (); }, UIControlEvent.TouchUpInside);
			
			// Autosize the bio URL to fit available space
			var size = _urlSize;
			var urlFont = UIFont.BoldSystemFontOfSize(size);
			var urlSize = new NSString(_bio.Url).StringSize(urlFont);
			var available = Util.IsPad() ? 400 : 185; // Util.IsRetina() ? 185 : 250;		
			while(urlSize.Width > available)
			{
				urlFont = UIFont.BoldSystemFontOfSize(size--);
				urlSize = new NSString(_bio.Url).StringSize(urlFont);
			}
			
			url.Font = urlFont;			
			url.Frame = new RectangleF ((float)_left, (float)70, (float)(frame.Width - _left), (float)size);
			url.SetTitle(_bio.Url, UIControlState.Normal);
			url.SetTitle(_bio.Url, UIControlState.Highlighted);			
			AddSubview(url);
		}
		
		public void AddDefaultImage(Bio bio)
		{
			var frame = new RectangleF(10, 10, DimensionSet.BioImageSquareSize, DimensionSet.BioImageSquareSize);
			var defaultProfile = new UIImageView();
			defaultProfile.Frame = frame;
			defaultProfile.Image = GetDefaultBioImage();
			defaultProfile.BackgroundColor = UIColor.Clear;
			AddSubview(defaultProfile);
		}
		
		public UIImage GetDefaultBioImage()
		{
			var src = AppManifest.Current.Image;
			var image = UIImageEx.FromIdiomBundle(src);
			image = ImageHelper.RoundAndSquare(image, 8);	
			return image;
		}
		
		public void AddOrUpdateBio(Bio bio)
		{
			var frame = new RectangleF(10, 10, DimensionSet.BioImageSquareSize, DimensionSet.BioImageSquareSize);
			_profile = _profile ?? new UIImageView(frame);
			_profile.Image = bio.Image;
			_profile.BackgroundColor = UIColor.Clear;
			if(_profile.Superview == null)
			{
				AddSubview(_profile);
			}
			this.SetNeedsDisplay();
		}
		
		public void ShowActivity()
		{
			_spinner.StartAnimating();	
		}
		
		public void HideActivity()
		{
			_spinner.StopAnimating();	
		}
		
		public event NSAction PictureTapped;
		public event NSAction UrlTapped;
		public event NSAction Tapped;
		
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			var touch = touches.AnyObject as UITouch;
			var location = touch.LocationInView (this);
			if (_profile.Frame.Contains (location))
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
			base.TouchesBegan (touches, evt);
		}

		public override void Draw(RectangleF rect)
		{
			var w = rect.Width - _left;
			var context = UIGraphics.GetCurrentContext();
			
			// Draw text
			context.SaveState();
			context.SetFillColor(0, 0, 0, 1);
			context.SetShadowWithColor(new SizeF (0, -1), 1, UIColor.White.CGColor);
			DrawString (_bio.Name, new RectangleF(_left, 12, w, userSize), _userFont, UILineBreakMode.TailTruncation);
			DrawString (_bio.Location, new RectangleF(_left, 50, w, locationSize), _locationFont, UILineBreakMode.TailTruncation);
			UIColor.DarkGray.SetColor();
			DrawString (_bio.Title, new RectangleF(_left, 34, w, followerSize), _followerFont);

			// Draw border
			context.RestoreState();
			context.TranslateCTM(9, 9);
			context.AddPath(_borderPath);
			context.SetStrokeColor(0.5f, 0.5f, 0.5f, 1);
			context.SetLineWidth(0.5f);
			context.StrokePath();
		}
	}
}

