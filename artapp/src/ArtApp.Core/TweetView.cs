using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ArtApp
{
	public class TweetView : UIView
	{
		public float Height { get; private set; }

		// Tapped events
		public delegate void TappedEvent (TweetTap value);
		public TappedEvent Tapped;
		public TappedEvent TapAndHold;
		
		// Rest of the text is UIFont.ItalicSystemFontOfSize(15);
		private UIFont regular = UIFont.ItalicSystemFontOfSize (fontHeight);
		private UIFont bold = UIFont.BoldSystemFontOfSize (fontHeight);
		
		public string Text;
		private RectangleF lastRect;
		private List<Block> blocks;
		private Block highlighted = null;
		
		private int top = 5;
		private int left = 5;

		const int fontHeight = 15;
		
		public TweetView (RectangleF frame, string text, TappedEvent tapped, TappedEvent tapAndHold) : base (frame)
		{
			this.BackgroundColor = UIColor.Clear;
			blocks = new List<Block> ();
			lastRect = RectangleF.Empty;

			this.Text = text;
			Height = Layout ();
			Tapped = tapped;
			TapAndHold = tapAndHold;
			
			// Update our Frame size
			var f = Frame;
			f.Height = Height;
			Frame = f;
		}
		
		class Block
		{
			public string Value;
			public RectangleF Bounds;
			public UIFont Font;
		}
		
		private const int lineHeight = fontHeight + 4;
		public float Layout()
		{
			float max = Bounds.Width, segmentLength, lastx = 0, x = 0, y = 0;
			int p = 0;
			UIFont font = regular, lastFont = null;
			string line = "";
			
			blocks.Clear ();
			while (p < Text.Length)
			{
				int sidx = Text.IndexOf (' ', p);
				if (sidx == -1)
					sidx = Text.Length-1;
						
				var segment = Text.Substring (p, sidx-p+1);
				if (segment.Length == 0)
					break;
				
				// if the word contains @ like ".@foo" or "foo@bar", split there as well
				int aidx = segment.IndexOf ('@');
				if (aidx > 0){
					segment = segment.Substring (0, aidx);
					sidx = p + segment.Length-aidx;
				}
				
				var start = segment [0];
				if (start == '@' || start == '#' || IndexOfUrlStarter (segment, 0) != -1)
					font = bold;
				else
					font = regular;
				
				segmentLength = StringSize (segment, font).Width;
			
				// If we would overflow the line.
				if (x + segmentLength >= max){
					// Push the text we have so far, go to next line
					if (line != ""){
						blocks.Add (new Block () {
							// Respect the margins
							Bounds = new RectangleF (lastx + left, y + top, x-lastx, lineHeight),
							Value = line,
							Font = lastFont ?? font,
						});
						lastFont = font;
						y += lineHeight;
						lastx = 0;
					}
					
					// Too long to fit even on its own line, stick it on its own line.
					if (segmentLength >= max){
						var dim = StringSize (segment, font, new SizeF (max, float.MaxValue), UILineBreakMode.WordWrap);
						blocks.Add (new Block () {
							// TODO? Respect the margins?
							Bounds = new RectangleF (new PointF (left, y + top), dim),
							Value = segment,
							Font = lastFont ?? font
						});
						y += dim.Height;
						x = 0;
						line = "";
					} else {
						x = segmentLength;
						line = segment;
					}
					p = sidx + 1;
					lastFont = font;
				} else {
					// append the segment if the font changed, or if the font
					// is bold (so we can make a tappable element on its own).
					if (x != 0 && (font != lastFont || font == bold)){
						blocks.Add (new Block () {
							// Respect the margins
							Bounds = new RectangleF (lastx + left, y + top, x-lastx, lineHeight),
							Value = line,
							Font = lastFont
						});
						lastx = x;
						line = segment;
						lastFont = font;
					} else {
						lastFont = font;
						line = line + segment;
					}
					x += segmentLength;
					p = sidx+1;
				}
				// remove duplicate spaces
				while (p < Text.Length && Text [p] == ' ')
					p++;
				//Console.WriteLine ("p={0}", p);
			}
			if (line == "")
				return y;
			
			blocks.Add (new Block () {
				// Respect the margins
				Bounds = new RectangleF (lastx + left, y + top, x-lastx, lineHeight),
				Value = line,
				Font = font
			});
			
			return y + lineHeight + top + top;
		}
		
		public override void Draw(RectangleF rect)
		{
			if (rect != lastRect)
			{
				Layout();
				lastRect = rect;
			}
			
			var context = UIGraphics.GetCurrentContext();
			UIFont last = null;

			foreach (var block in blocks)
			{
				var font = block.Font;
				if (font != last)
				{
					if (font == bold)
					{
						UIColor.FromRGB (0x32, 0x4f, 0x85).SetFill();
					}
					else
					{
						UIColor.DarkGray.SetFill();
					}
					last = font;
				}

				// selected?
				if (block == highlighted && block.Font == bold)
				{
					context.FillRect (block.Bounds);
					context.SetFillColor (1, 1, 1, 1);
					last = null;
				}
				
				// We need to use the full overload because the short overload does not
				// render Unicode character beyond a simple range.   Amazing, but true
				DrawString(block.Value, block.Bounds, block.Font, UILineBreakMode.WordWrap, UITextAlignment.Left);
			}
		}
				
		public enum TweetType
		{
			Url,
			Mention,
			Hashtag
		}
	
		public class TweetTap
		{
			public string Value;
			public TweetType Type;
		}
		
		private static TweetTap PrepareTappedText(string source)
		{
			var tap = new TweetTap();
			
			if (source.StartsWith ("http://") || source.StartsWith ("https://"))
			{
				tap.Value = source.Trim();
				return tap;
			}
			if (source.StartsWith ("bit.ly/"))
			{
				tap.Value = "http://" + source.Trim();
				return tap;
			}
			if (source [0] == '@')
			{
				source = source.Trim ();
				if (source.EndsWith(":") || source.EndsWith ("."))
				{
					tap.Value = source.Substring (0, source.Length-1);
					tap.Type = TweetType.Mention;
					return tap;
				}
				tap.Value = source;
				tap.Type = TweetType.Mention;
				return tap;
			}			
			if (source[0] == '#')
			{
				tap.Value = source.Trim();
				tap.Type = TweetType.Hashtag;
			}
			
			return tap;
		}
		
		private bool blockingTouchEvents;
		private NSTimer holdTimer;
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			blockingTouchEvents = false;
			Track ((touches.AnyObject as UITouch).LocationInView (this));
			
			// Start tracking tap and hold
			if (highlighted != null && highlighted.Font == bold){
				holdTimer = NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (1), delegate {
					blockingTouchEvents = true;
					
					if (TapAndHold != null)
						TapAndHold (PrepareTappedText (highlighted.Value));
				});
			}
		}

		void CancelHoldTimer ()
		{
			if (holdTimer == null)
				return;
			holdTimer.Invalidate ();
			holdTimer = null;
		}
		
		void Track (PointF pos)
		{
			foreach (var block in blocks)
			{
				if (!block.Bounds.Contains (pos))
				{
					continue;
				}
				highlighted = block;
				SetNeedsDisplay ();
			}
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			CancelHoldTimer ();
			if (blockingTouchEvents)
				return;
			if (highlighted != null && highlighted.Font == bold){
				if (Tapped != null)
					Tapped (PrepareTappedText (highlighted.Value));
			}
			
			highlighted = null;
			SetNeedsDisplay ();
		}
		
		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			CancelHoldTimer ();
			highlighted = null;
			SetNeedsDisplay ();
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			CancelHoldTimer ();
			Track ((touches.AnyObject as UITouch).LocationInView (this));
		}		
		
		static string [] urlStarters = {
			"http://", "https://", "bit.ly/", "t.co"
		};
		
		public static int IndexOfUrlStarter (string text, int startIndex)
		{
			int min = -1;
				
			foreach (var urlStart in urlStarters){
				int n = text.IndexOf (urlStart, startIndex, StringComparison.InvariantCultureIgnoreCase);
				if (n >= 0){
					if (min == -1)
						min = n;
					else if (n < min)
						min = n;
				}
			}
			return min;
		}
	}
}