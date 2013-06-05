using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

// Original source: https://github.com/samsoffes/sstoolkit/blob/master/SSToolkit

namespace ArtApp
{
	public class LoadingView : UIView, IDisposable
	{
		public const float INDICATOR_SIZE = 40.0f;
		
		private CustomWindow _hudWindow;
		private bool _loading;
		private bool _textLabelHidden;
		private UILabel _textLabel;
		private SizeF _hudSize;
		private UIActivityIndicatorView _activityIndicator;
		private bool _successful;
				
		public LoadingView(string title) : this(title, true) { }
		
		public LoadingView(string title, bool isLoading) : base(RectangleF.Empty)
		{	
			this.BackgroundColor = UIColor.Clear;
			_hudSize = new SizeF(172.0f, 172.0f);
	
			// Indicator
			_activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			_activityIndicator.Alpha = 0.0f;
			_activityIndicator.StartAnimating();
			this.AddSubview(_activityIndicator);
	
			// Text Label
			_textLabel = new UILabel(RectangleF.Empty);
			_textLabel.Font = UIFont.BoldSystemFontOfSize(14.0f);
			_textLabel.BackgroundColor = UIColor.Clear;
			_textLabel.TextColor = UIColor.White;
			_textLabel.ShadowColor = UIColor.FromWhiteAlpha(0.0f, 0.7f);
			_textLabel.ShadowOffset = new SizeF(0.0f, 1.0f);
			_textLabel.TextAlignment = UITextAlignment.Center;
			_textLabel.LineBreakMode = UILineBreakMode.TailTruncation;
			_textLabel.Text = title ?? @"Loading";
			this.AddSubview(_textLabel);
	
			// Loading
			this.SetLoading(isLoading);
	        
			// This is unnecessary for our locked views, and causing transform disposal race conditions
	        // Orientation
			//this.SetTransformForCurrentOrientation(false);
			//_observer = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, DeviceOrientationChanged);
		}
		
		private NSObject _observer;
			
		public string Text
		{
			get
			{
				return _textLabel.Text;
			}
			set
			{
				_textLabel.Text = value;	
			}
		}
		
		public bool HidesVignette
		{
			get
			{
				return _hudWindow.HidesVignette;
			}
			set
			{
				SetHidesVignette(value);	
			}
		}
		
		public void SetTextLabelHidden(bool hidden)
		{
			_textLabelHidden = hidden;
			_textLabel.Hidden = hidden;
			this.SetNeedsLayout();
		}
		
		public void SetLoading(bool isLoading)
		{
			_loading = isLoading;
			_activityIndicator.Alpha = _loading ? 1.0f : 0.0f;
			this.SetNeedsDisplay();
		}

		public void SetHidesVignette(bool hide)
		{
			_hudWindow.HidesVignette = hide;
		}
		
		public override void Draw(RectangleF rect)
		{
			// Draw rounded rectangle
			var context = UIGraphics.GetCurrentContext();
			context.SetFillColor(0.0f, 0.0f, 0.0f, 0.5f);
			var rrect = new RectangleF(0.0f, 0.0f, _hudSize.Width, _hudSize.Height);
			SSDrawRoundedRect(context, rrect, 14.0f);
			context.Dispose();
			
			if (!_loading)
			{
			    UIColor.White.SetColor();				
				UIImage image = _successful ? UIImage.FromBundle ("Images/gallery/loadedSuccess.png") : UIImage.FromBundle ("Images/gallery/loadedSuccess.png");
				var imageSize = image.Size;
				var imageRect = new RectangleF((float)Math.Round((_hudSize.Width - imageSize.Width) / 2.0f),
											   (float)Math.Round((_hudSize.Height - imageSize.Height) / 2.0f),
											   imageSize.Width, imageSize.Height);
				image.Draw(imageRect);
				image.Dispose();
			}
		}
				
		public void SSDrawRoundedRect(CGContext context, RectangleF rect, float cornerRadius)
		{
			var minx = rect.GetMinX();
			var midx = rect.GetMidX();
			var maxx = rect.GetMaxX();
			var miny = rect.GetMinY();
			var midy = rect.GetMidY();
			var maxy = rect.GetMaxY();
			context.MoveTo(minx, midy);
			context.AddArcToPoint(minx, miny, midx, miny, cornerRadius);
			context.AddArcToPoint(maxx, miny, maxx, midy, cornerRadius);
			context.AddArcToPoint(maxx, maxy, midx, maxy, cornerRadius);
			context.AddArcToPoint(minx, maxy, minx, midy, cornerRadius);
			context.ClosePath();
			context.FillPath();
		}
		
		public override void LayoutSubviews()
		{
			_activityIndicator.Frame = new RectangleF((float)Math.Round((_hudSize.Width - INDICATOR_SIZE) / 2.0f),
												  (float)Math.Round((_hudSize.Height - INDICATOR_SIZE) / 2.0f),
												  INDICATOR_SIZE, INDICATOR_SIZE);
		
			if (_textLabelHidden)
			{
				_textLabel.Frame = RectangleF.Empty;
			}
			else
			{
				var textSize =  _textLabel.StringSize(_textLabel.Text, _textLabel.Font, new SizeF(this.Bounds.Size.Width, float.MaxValue), _textLabel.LineBreakMode);
				_textLabel.Frame = new RectangleF(0.0f, (float)Math.Round(_hudSize.Height - textSize.Height - 10.0f), _hudSize.Width, textSize.Height);
			}
		}
		
		public void Show()
		{
			_hudWindow = _hudWindow ?? CustomWindow.DefaultWindow;
			_hudWindow.Alpha = 0.0f;
			this.Alpha = 0.0f;
			_hudWindow.AddSubview(this);
			_hudWindow.MakeKeyAndVisible();
		    
			UIView.BeginAnimations(@"SSHUDViewFadeInWindow");
			_hudWindow.Alpha = 1.0f;
			UIView.CommitAnimations();
			
			var windowSize = _hudWindow.Frame.Size;
			
			var contentFrame = new RectangleF((float)Math.Round((windowSize.Width - _hudSize.Width) / 2.0f), 
											 (float)Math.Round((windowSize.Height - _hudSize.Height) / 2.0f) + 10.0f,
											 _hudSize.Width, _hudSize.Height);
		
		    
		    var offset = 20.0f;
		    if (Util.IsPortrait())
			{
		        this.Frame = new RectangleF(contentFrame.X, contentFrame.Y + offset, contentFrame.Width, contentFrame.Height);
		    }
			else
			{
		        this.Frame = new RectangleF(contentFrame.X + offset, contentFrame.Y, contentFrame.Width, contentFrame.Height);
		    }
		
			UIView.BeginAnimations(@"SSHUDViewFadeInContentAlpha");
			UIView.SetAnimationDelay(0.1f);
			UIView.SetAnimationDuration(0.2f);
			this.Alpha = 1.0f;
			UIView.CommitAnimations();
		
			UIView.BeginAnimations(@"SSHUDViewFadeInContentFrame");
			UIView.SetAnimationDelay(0.1f);
			UIView.SetAnimationDuration(0.3f);
			this.Frame = contentFrame;
			UIView.CommitAnimations();
		}
		
		public void CompleteWithTitle(string title)
		{
			this._successful = true;
			this.SetLoading(false);
			_textLabel.Text = title;
		}
		
		public void CompleteAndDismissWithTitle(string title)
		{
			this.CompleteWithTitle(title);
			this.PerformSelector(Dismiss, 1.0f);
		}	
		
		public void CompleteAndDismissWithTitle(string title, float delay)
		{
			this.CompleteWithTitle(title);
			this.PerformSelector(Dismiss, delay);
		}
		
		public void CompleteAndDismissWithTitle(string title, float delay, Action after)
		{
			this.CompleteWithTitle(title);
			this.PerformSelector(delegate { Dismiss(); after(); }, delay);
		}
		
		public void CompleteQuicklyWithTitle(string title)
		{
			this.CompleteWithTitle(title);
			this.Show();
			this.PerformSelector(Dismiss, 1.05f);
		}		
		
		public void FailWithTitle(string title)
		{
			this._successful = false;
			this.SetLoading(false);
			_textLabel.Text = title;
		}		
		
		public void FailAndDismissWithTitle(string title)
		{
			this.FailWithTitle(title);
			this.PerformSelector(Dismiss, 1.0f);
		}
		
		public void FailQuicklyWithTitle(string title)
		{
			this.FailWithTitle(title);
			this.Show();
			this.PerformSelector(Dismiss, 1.05f);
		}		
		
		public void DismissAfter(float delay)
		{
			this.PerformSelector(Dismiss, delay);	
		}
			                     
		public void Dismiss()
		{
			this.DismissAnimated(true);
		}
		
		public void DismissAnimated(bool animated)
		{
			UIView.BeginAnimations(@"SSHUDViewFadeOutContentFrame");
			UIView.SetAnimationDuration(0.2f);
			var contentFrame = this.Frame;
		    var offset = 20.0f;
			
			if (Util.IsPortrait())
			{
		        this.Frame = new RectangleF(contentFrame.X, contentFrame.Y + offset, contentFrame.Width, contentFrame.Height);
		    }
			else
			{
		        this.Frame = new RectangleF(contentFrame.X + offset, contentFrame.Y, contentFrame.Width, contentFrame.Height);
		    }
			UIView.CommitAnimations();
		
			UIView.BeginAnimations(@"SSHUDViewFadeOutContentAlpha");
			UIView.SetAnimationDelay(0.1f);
			UIView.SetAnimationDuration(0.2f);
			this.Alpha = 0.0f;
			UIView.CommitAnimations();
		
			UIView.BeginAnimations(@"SSHUDViewFadeOutWindow");
			_hudWindow.Alpha = 0.0f;
			UIView.CommitAnimations();		
			if (animated)
			{
				this.PerformSelector(RemoveWindow, 0.3f);
			}
			else
			{
				this.RemoveWindow();
			}
		}
		
		// http://blog.touch4apps.com/home/iphone-monotouch-development/monotouch-performselector-helper	
		private delegate void Selector();
		private void PerformSelector(Selector selector, float delay)
		{
			NSTimer.CreateScheduledTimer(TimeSpan.FromSeconds(delay), delegate { selector (); });
		}
		
		public void SetTransformForCurrentOrientation(bool animated)
		{
			UIInterfaceOrientation orientation = UIApplication.SharedApplication.StatusBarOrientation;
			var degrees = 0f;
    
		    // Landscape left
			if (orientation == UIInterfaceOrientation.LandscapeLeft) {
				degrees = -90f;
			}
		
			// Landscape right
			if (orientation == UIInterfaceOrientation.LandscapeRight) {
				degrees = 90f;
			}
		
			// Portrait upside down
			else if (orientation == UIInterfaceOrientation.PortraitUpsideDown) {
				degrees = 180f;
			}
		    
		    CGAffineTransform rotationTransform = CGAffineTransform.MakeRotation(DegreesToRadians(degrees));
		    
			if (animated)
			{
				UIView.BeginAnimations(@"SSHUDViewRotationTransform");
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
				UIView.SetAnimationDuration(0.3);
			}
		    
			this.Transform = rotationTransform;
		    
		    if (animated)
			{
				UIView.CommitAnimations();
			}
		}
		
		private float DegreesToRadians(float angle)
		{
			return (float)(Math.PI * angle / 180.0f);
		}

		private void DeviceOrientationChanged(NSNotification notification)
		{
		    this.SetTransformForCurrentOrientation(true);
			this.SetNeedsDisplay();
		}

		private void RemoveWindow()
		{	
			_hudWindow.ResignKeyWindow();
			_hudWindow = null;
		
			// Return focus to the first window
			UIApplication.SharedApplication.Windows[0].MakeKeyWindow();
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if(_observer != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_observer);
				_observer.Dispose();
				_observer = null;
			}	
		}
	}
}