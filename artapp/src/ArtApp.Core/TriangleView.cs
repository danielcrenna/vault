using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ArtApp
{
	public class TriangleView : UIView 
	{
		private UIColor _fill;
		private UIColor _stroke;
		
		public TriangleView (UIColor fill, UIColor stroke) 
		{
			Opaque = false;
			_fill = fill;
			_stroke = stroke;
		}
		
		public override void Draw(RectangleF rect)
		{
			var context = UIGraphics.GetCurrentContext();
			var b = Bounds;
			
			_fill.SetColor();
			context.MoveTo(0, b.Height);
			context.AddLineToPoint(b.Width / 2, 0);
			context.AddLineToPoint(b.Width, b.Height);
			context.ClosePath();
			context.FillPath();
			
			_stroke.SetColor();
			context.MoveTo(0, b.Width/2);
			context.AddLineToPoint(b.Width / 2, 0);
			context.AddLineToPoint(b.Width, (b.Width / 2));
			context.StrokePath();
		}
	}
}

