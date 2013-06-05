using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace ArtApp
{
	public partial class CustomNavigationBar : UINavigationBar
	{
		public const int MAX_BACK_BUTTON_WIDTH = 160;
		private int backButtonCapWidth;
		
		private UIImageView _overlayImageView;
		private UIImageView _underlayImageView;
		private bool _isTransparent;
		
		public bool IsTransparent
		{
			get
			{
				return _isTransparent;
			}
		}

		public CustomNavigationBar (IntPtr ptr) : base(ptr)
		{
			// Assuming translucent is necessary to avoid inset glitches when we flip
			Translucent = true;
			Opaque = false;
			TintColor = UIColor.Clear;
			BackgroundColor = UIColor.Clear;
			
			var opaqueImage = UIImage.FromBundle("Images/opaqueBar.png");
			_overlayImageView = new UIImageView(this.Frame);
			_overlayImageView.Image = opaqueImage;
			_overlayImageView.Alpha = 1.0f;
			opaqueImage.Dispose();
			
			var transparentImage = UIImage.FromBundle("Images/transparentBar.png");
			_underlayImageView = new UIImageView (this.Frame);
			_underlayImageView.Image = transparentImage;
			_underlayImageView.Alpha = 1.0f;
			transparentImage.Dispose();
			
			SetNeedsDisplay(); 
		}
		
		public override void Draw (RectangleF rect)
		{
			if (_overlayImageView != null)
			{
				//Util.Log ("Drawing blended image with overlay alpha " + _overlayImageView.Alpha);
				UIImage bottomImage = _underlayImageView.Image;  
				UIImage topImage = _overlayImageView.Image;
				UIImageView imageView = new UIImageView(bottomImage);
				UIImageView subView = new UIImageView(topImage);
				bottomImage.Dispose();
				topImage.Dispose();
				
				subView.Alpha = _overlayImageView.Alpha;
				imageView.AddSubview(subView);
				
				UIGraphics.BeginImageContext(imageView.Frame.Size);
				imageView.Layer.RenderInContext(UIGraphics.GetCurrentContext());
				UIImage blendedImage = UIGraphics.GetImageFromCurrentImageContext();
				UIGraphics.EndImageContext();
				
				subView.Dispose();
				imageView.Dispose();
				
				blendedImage.Draw(rect);
				blendedImage.Dispose();				
			}
			else
			{
				// Draw nothing		
			}
		}
		
		public void SetOpaque()
		{
			FadeIn();
			_isTransparent = false;
		}
		
		public void SetTransparent()
		{
			FadeOut();
			_isTransparent = true;
		}
		
		private NSTimer _inTimer;
		private void FadeIn()
		{
			_inTimer = NSTimer.CreateRepeatingScheduledTimer (0.01, () => { FadeInTimerEvent (); });
		}

		private NSTimer _outTimer;
		private void FadeOut()
		{
			_outTimer = NSTimer.CreateRepeatingScheduledTimer (0.01, () => { FadeOutTimerEvent (); });
		}
		
		private static readonly float FADE_IN_RATE = 2.0f / 10.0f;
		private static readonly float FADE_OUT_RATE = 2.0f / 50.0f;
		
		private void FadeOutTimerEvent ()
		{
			if (_outTimer == null)
			{
				return;	
			}
			if (_overlayImageView.Alpha <= 0.0f)
			{
				_outTimer.Invalidate();
				_outTimer = null;
				_overlayImageView.Alpha = 0.0f;
			}
			else
			{ 
				_overlayImageView.Alpha -= FADE_OUT_RATE;
			}
			SetNeedsDisplay ();
		}
		
		private void FadeInTimerEvent()
		{
			if (_inTimer == null)
			{
				return;	
			}
			if (_overlayImageView.Alpha >= 1.0f && _inTimer != null)
			{
				_inTimer.Invalidate();
				_inTimer = null;
				_overlayImageView.Alpha = 1.0f;
			}
			else
			{ 
				_overlayImageView.Alpha += FADE_IN_RATE;
			}
			SetNeedsDisplay();
		}		
		
		public void SetBackButtonOn(UIViewController vc)
		{
			UIButton button = CreateBackButton(14);
			vc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(button);	
		}
		
		public UIButton CreateBackButton (int leftCapWidth)
		{
			backButtonCapWidth = leftCapWidth;		
			UIImage buttonImage = UIImage.FromBundle("Images/gallery/backButton.png").StretchableImage(backButtonCapWidth, 0);
			UIImage buttonHighlightImage = UIImage.FromBundle("Images/gallery/backButtonHighlighted.png").StretchableImage(backButtonCapWidth, 0); 
			
			UIButton button = UIButton.FromType(UIButtonType.Custom); 
			button.TitleLabel.Font = UIFont.BoldSystemFontOfSize (UIFont.SmallSystemFontSize);
			button.TitleLabel.TextColor = UIColor.White;
			button.TitleLabel.ShadowOffset = new SizeF (0, -1);
			button.TitleLabel.ShadowColor = UIColor.DarkGray;
			button.TitleLabel.LineBreakMode = UILineBreakMode.TailTruncation;
			button.TitleEdgeInsets = new UIEdgeInsets (0, 6.0f, 0, 3.0f);
			button.Frame = new RectangleF (0, 0, 0, buttonImage.Size.Height);
			
			SetBackButtonText(button);
			
			// Set the stretchable images as the background for the button
			button.SetBackgroundImage(buttonImage, UIControlState.Normal);
			button.SetBackgroundImage(buttonHighlightImage, UIControlState.Highlighted);
			button.SetBackgroundImage(buttonHighlightImage, UIControlState.Selected);
			button.AddTarget (delegate { _navigationController.PopViewControllerAnimated (true); }, UIControlEvent.TouchUpInside);
			
			buttonImage.Dispose();
			buttonHighlightImage.Dispose();

			return button;
		}

		private void SetBackButtonText (UIButton button)
		{
			NSString text;
			if (TopItem != null)
			{
				text = new NSString (!string.IsNullOrWhiteSpace(TopItem.Title) ? TopItem.Title : "Back");
			}
			else
			{
				text = new NSString("Home");
			}
			SetText(text, button);
		}
		
		public void SetText (NSString text, UIButton backButton)
		{
			var textSize = text.StringSize (backButton.TitleLabel.Font);
			var delta = (textSize.Width + (backButtonCapWidth * 1.5)) > MAX_BACK_BUTTON_WIDTH ? MAX_BACK_BUTTON_WIDTH : (textSize.Width + (backButtonCapWidth * 1.5));
			backButton.Frame = new RectangleF (backButton.Frame.X, backButton.Frame.Y, (float)delta, backButton.Frame.Size.Height);
			backButton.SetTitle (text, UIControlState.Normal);
		}
	}
}

