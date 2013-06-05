using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace ArtApp
{
	public enum LabelVerticalTextAlignment
	{
		Top,
		Middle,
		Bottom
	}
	
	public class TextLabel : UILabel
	{
		private LabelVerticalTextAlignment _verticalTextAlignment;
		public LabelVerticalTextAlignment VerticalTextAlignment
		{
			get
			{
				return _verticalTextAlignment;
			}
			set
			{
				_verticalTextAlignment = value;
				this.SetNeedsLayout();		
			}
		}
		
		private UIEdgeInsets _textEdgeInsets;
		public UIEdgeInsets TextEdgeInsets
		{
			get
			{
				return _textEdgeInsets;
			}
			set
			{
				_textEdgeInsets = value;
				this.SetNeedsLayout();
			}
		}
		
		public TextLabel(RectangleF frame) : base(frame)
		{
			this.VerticalTextAlignment = LabelVerticalTextAlignment.Middle;
			this.TextEdgeInsets = UIEdgeInsets.Zero;
		}
		
		// UIEdgeInsetsInsetRect is missing from MonoTouch
		public RectangleF UIEdgeInsetsInsetRect(RectangleF rect, UIEdgeInsets insets)
		{
			return new RectangleF(
				rect.X + insets.Left, 						// 0 + 20 left 
				rect.Y + insets.Top,  						// 0 + 10 top
				rect.Width - (insets.Left + insets.Right),	// 100 - (20 left + 40 right)
				rect.Height	- (insets.Top + insets.Bottom)  // 200 - (10 top + 30 bottom)			  
			);
		}
		
		public override void DrawText(RectangleF rect)
		{
			rect = UIEdgeInsetsInsetRect(rect, _textEdgeInsets);
							
			if (this.VerticalTextAlignment == LabelVerticalTextAlignment.Top)
			{
				var sizeThatFits = this.SizeThatFits(rect.Size);
				rect = new RectangleF(rect.X, rect.Y, rect.Size.Width, sizeThatFits.Height);
			}
			else if (this.VerticalTextAlignment == LabelVerticalTextAlignment.Bottom)
			{
				var sizeThatFits = this.SizeThatFits(rect.Size);
				rect = new RectangleF(rect.X, rect.Y + (rect.Size.Height - sizeThatFits.Height), rect.Size.Width, sizeThatFits.Height);
			}
			
			base.DrawText(rect);
		}
		
	}
}

