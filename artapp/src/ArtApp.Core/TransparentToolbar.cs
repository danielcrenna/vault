using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ArtApp
{
	[Register("TransparentToolbar")]
	public class TransparentToolbar : UIToolbar
	{
		public TransparentToolbar() : base()
		{
			ApplyTranslucentBackground();
		}
		
		public TransparentToolbar(RectangleF frame) : base(frame)
		{
			ApplyTranslucentBackground();	
		}
		
		public override void Draw (RectangleF rect)
		{
			// Draw nothing
		}

		public void ApplyTranslucentBackground()
		{
			BackgroundColor = UIColor.Clear;
			Opaque = true;
			Translucent = true;
		}
	}
}

