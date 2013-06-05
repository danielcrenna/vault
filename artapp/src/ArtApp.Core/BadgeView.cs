using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace ArtApp
{
	public class BadgeView : UIView
	{
		public static readonly UIColor DefaultBadgeColor = new UIColor(0.541f, 0.596f, 0.694f, 1.0f);
		
		#region Backing Properties
		private TextLabel _textLabel;
		public TextLabel TextLabel
		{
			get
			{
				return _textLabel;	
			}
			set
			{
				_textLabel = value;	
				SetNeedsDisplay();
			}
		}

		private BadgeViewAlignment _badgeAlignment;
		public BadgeViewAlignment BadgeAlignment
		{
			get 
			{
				return _badgeAlignment;
			}
			set
			{
				_badgeAlignment = value;
				SetNeedsDisplay();
			}
		}

		private bool _highlighted;
		public bool Highlighted
		{
			get
			{
				return _highlighted;	
			}
			set
			{
				_highlighted = value;
				SetNeedsDisplay();
			}
		}
				
		private UIColor _highlightedBadgeColor;
		public UIColor HighlightedBadgeColor
		{
			get
			{
				return _highlightedBadgeColor;	
			}
			set
			{
				_highlightedBadgeColor = value;
				SetNeedsDisplay();
			}
		}
				
		private UIImage _highlightedBadgeImage;
		public UIImage HighlightedBadgeImage
		{
			get
			{
				return _highlightedBadgeImage;	
			}
			set
			{
				_highlightedBadgeImage = value;	
				SetNeedsDisplay();
			}
		}

		private UIImage _badgeImage;
		public UIImage BadgeImage
		{
			get
			{
				return _badgeImage;
			}
			set
			{
				_badgeImage = value;
				SetNeedsDisplay();
			}
		}

		private UIColor _badgeColor;
		public UIColor BadgeColor
		{
			get
			{
				return _badgeColor;	
			}
			set
			{
				_badgeColor = value;
				SetNeedsDisplay();
			}
		}

		private float _cornerRadius;
		public float CornerRadius
		{
			get
			{
				return _cornerRadius;	
			}
			set
			{
				_cornerRadius = value;
				SetNeedsDisplay();
			}	
		}
		#endregion
		
		public BadgeView(RectangleF frame) : base(frame)
		{
			Initialize();
		}
		
		private void Initialize()
		{
			BackgroundColor = UIColor.Blue;
			Opaque = true;
			
			_textLabel = new TextLabel(RectangleF.Empty); 
			_textLabel.Text = "";
			_textLabel.TextColor = UIColor.White;
			_textLabel.HighlightedTextColor = new UIColor(0.125f, 0.369f, 0.871f, 1.0f);
			_textLabel.Font = UIFont.BoldSystemFontOfSize(16.0f);
			_textLabel.TextAlignment = UITextAlignment.Center;
			
			BadgeColor = DefaultBadgeColor;
			HighlightedBadgeColor = UIColor.White;
			CornerRadius = 10.0f;
			BadgeAlignment = BadgeViewAlignment.Center;
			Highlighted = false;
		}

		public override void Draw(RectangleF rect)
		{
			UIColor currentBadgeColor = null;
			UIImage currentBadgeImage = null;
				
			if (_highlighted)
			{
				currentBadgeColor = _highlightedBadgeColor != null ? _highlightedBadgeColor : _badgeColor;
				currentBadgeImage = _highlightedBadgeImage != null ? _highlightedBadgeImage : _badgeImage;
			}
			else
			{
				currentBadgeColor = _badgeColor;
				currentBadgeImage = _badgeImage;
			}
			
			var context = UIGraphics.GetCurrentContext();
			
			// Badge
			var size = Frame.Size;
			var badgeSize = SizeThatFits(size);
			badgeSize.Height = (float)Math.Min(badgeSize.Height, size.Height);
			
			var x = 0.0f;
			if (_badgeAlignment == BadgeViewAlignment.Center)
			{
				x =  (float)Math.Round((size.Width - badgeSize.Width) / 2.0f);
			}
			else if (_badgeAlignment == BadgeViewAlignment.Right)
			{
				x = size.Width - badgeSize.Width;
			}
			
			var badgeRect = new RectangleF(x, (float)Math.Round((size.Height - badgeSize.Height) / 2.0f), badgeSize.Width, badgeSize.Height);
			
			// Draw image
			if (currentBadgeImage != null)
			{
				currentBadgeImage.Draw(badgeRect);
			}
			// Draw rectangle
			else if (currentBadgeColor != null)
			{
				currentBadgeColor.SetColor();
				SSDrawRoundedRect(context, badgeRect, _cornerRadius);
			}
			
			// Text
			_textLabel.DrawText(badgeRect);
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
		
		public override SizeF SizeThatFits (SizeF size)
		{
			var textSize = _textLabel.SizeThatFits(this.Bounds.Size);
			return new SizeF((float)Math.Max(textSize.Width + 12.0f, 30.0f), textSize.Height + 8.0f);
		}
		
		public override void WillMoveToSuperview(UIView newSuperview)
		{
			base.WillMoveToSuperview(newSuperview);
			
			var keyPath = new NSString(@"text");
			if (newSuperview != null)
			{
				_textLabel.AddObserver(this, new NSString(@"text"), NSKeyValueObservingOptions.New, IntPtr.Zero);
				this.Hidden = _textLabel.Text != null && _textLabel.Text.Length == 0;
			}
			else
			{
				_textLabel.RemoveObserver(this, keyPath);
			}
		}
		
		public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			if (ofObject == _textLabel && keyPath.Equals(@"text"))
			{
				NSString text = (NSString)change.ObjectForKey((NSObject)NSObject.ChangeNewKey);
				Hidden = text.Length == 0;
				
				if (!Hidden)
				{
					SetNeedsDisplay();
				}
				return;
			}
			
			base.ObserveValue(keyPath, ofObject, change, context);
		}
	}
}

