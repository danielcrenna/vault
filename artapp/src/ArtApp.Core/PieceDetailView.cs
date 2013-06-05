using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using MonoTouch.MapKit;
using MonoTouch.Dialog;

namespace ArtApp
{
	public class PieceDetailView : UIView
	{
	    protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_buy != null)
				{
					_buy.Dispose();	
				}
				if(_pieceView != null)
				{
					_pieceView.Image.Dispose();
					_pieceView.Image = null;
					_pieceView.Dispose ();
					_pieceView = null;
				}
				if(_mapView != null)
				{
					_mapView.Dispose();
					_mapView = null;
				}
				if(_backgroundView != null)
				{
					_backgroundView.Image.Dispose();
					_backgroundView.Image = null;
					_backgroundView.Dispose();
					_backgroundView = null;
				}
				if(_detailsImageView != null)
				{
					_detailsImageView.Image.Dispose();
					_detailsImageView.Image = null;
					_detailsImageView.Dispose();
					_detailsImageView = null;
				}
			}
			base.Dispose (disposing);
		}
		
		// Dimensions that change based on retina display (for drawing text on the card)
		private int _titleSize = 19;
		private int _mediaSize = 13;
		private int _left = Util.IsPad() ? 95 + 73 : 95;
		
		private static readonly int _size = Util.IsPad() ? 146 : 73;
		private const int _physicalSize = 14;
		private const int urlSize = 14;
		private int _y = 176 + 44;
		
		private UIImageView _pieceView;
		private UIImageView _backgroundView;
		private Piece _piece;
		private ConfirmButton _buy;
		
		private MKMapView _mapView;
		private RectangleF _transparentFrame;
		
		public ConfirmButton BuyButton
		{
			get
			{
				return _buy;		
			}
		}
		
		public SemiModalViewController Parent { get;set; }
		
		public PieceDetailView(IntPtr handle) : base(handle)
		{
			
		}
		
		public PieceDetailView(Piece piece, RectangleF frame) : base(frame)
		{
			_piece = piece;
			BackgroundColor = UIColor.Clear;
			
			var gutter = Util.IsPortrait() ? 158 : 0;
			var start = 176 + gutter;
			_y = _y + gutter;
						
			_transparentFrame = Util.IsPortrait() ? new RectangleF(0, 0, this.Frame.Width, start + 44):
				                                    new RectangleF(0, 0, this.Frame.Height, start + 44);
								
			var transparent = new UIView(_transparentFrame);
			
			AddSubview(transparent);
			AddBackground();
			AddDetailThumbnail();
			AddBuyButton();

			FitCardInCenter();
			SetNeedsDisplay();
		}

		private void AddBackground()
		{
			var backgroundImage = UIImageEx.FromIdiomBundleForBackground("Images/backgrounds/card.jpg");
			var backgroundFrame = new RectangleF(0, _y, backgroundImage.Size.Width, backgroundImage.Size.Height);
			
			if(_backgroundView != null)
			{
				_backgroundView.Image.Dispose();
				_backgroundView.Image = null;
				_backgroundView.Dispose();
				_backgroundView = null;
			}
			
			_backgroundView = new UIImageView(backgroundImage);
			_backgroundView.Frame = backgroundFrame;
			_backgroundView.Opaque = false;
			_backgroundView.Layer.MasksToBounds = false;
			_backgroundView.Layer.ShadowColor = UIColor.Black.CGColor;
			_backgroundView.Layer.ShadowOpacity = 1.0f;
			_backgroundView.Layer.ShadowRadius = 10.0f;
			_backgroundView.Layer.ShadowOffset = new SizeF(0, 1);
			
			backgroundImage.Dispose();
			AddSubview(_backgroundView);
		}
		
		private float _offset;
		public void FitCardInCenter()
		{
			var boundsSize = new SizeF(
				Util.IsLandscape() ? this.Bounds.Size.Height : this.Bounds.Size.Width, 
				Util.IsLandscape() ? this.Bounds.Size.Width : this.Bounds.Size.Height
				);
			
			var original = _backgroundView.Frame;
			var centered = CenterFrame (boundsSize, _backgroundView.Frame);
			
			_backgroundView.Frame = centered;
			_offset = centered.X - original.X;			
			_pieceView.Frame = new RectangleF(_pieceView.Frame.X + _offset, _pieceView.Frame.Y, _pieceView.Frame.Width, _pieceView.Frame.Height);
			
			if(_buy != null)
			{
				_buy.Frame = new RectangleF(_buy.Frame.X + _offset, _buy.Frame.Y, _buy.Frame.Width, _buy.Frame.Height); 	
			}	
		}

		RectangleF CenterFrame (SizeF boundsSize, RectangleF frameToCenter)
		{
			// Center horizontally
			if (frameToCenter.Size.Width < boundsSize.Width)
			    frameToCenter.X = (boundsSize.Width - frameToCenter.Size.Width) / 2;
			
			else
			    frameToCenter.X = 0;
			
			return frameToCenter;
		}
		
		
		public void AddDetailThumbnail()
		{
			var frame = new RectangleF (10, 10 + _y, _size, _size);
			if(_pieceView != null)
			{
				_pieceView.Image.Dispose();
				_pieceView.Image = null;
				_pieceView.Dispose();
				_pieceView = null;
			}			
			_pieceView = new UIImageView(frame);
			var img = ImageFactory.GeneratePieceThumbnail(_piece.Source, _size, _size, true, false);
			_pieceView.Image = img;
			_pieceView.BackgroundColor = UIColor.Clear;
			img.Dispose();
			AddSubview(_pieceView);
		}

		public void AddBuyButton()
		{
			var anchor = Util.IsPad() ? new PointF(758, 10 + _y): new PointF(310, 8 + _y);
			if(!_piece.Sold.HasValue)
			{
				return;
			}
			else
			{
				_buy = _piece.Sold.Value ? new ConfirmButton("SOLD") : new ConfirmButton("BUY", "EMAIL");
			}
			
			_buy.SetAnchor(anchor);			
			AddSubview(_buy);
		}

		public event NSAction PictureTapped;
		public event NSAction Tapped;
		public event NSAction OffsideTapped;
		
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			var touch = touches.AnyObject as UITouch;
			var location = touch.LocationInView (this);			
			if (_pieceView.Frame.Contains(location))
			{
				if (PictureTapped != null)
				{
					PictureTapped();
				}
			}
			else
			{
				if(_transparentFrame.Contains(location))
				{
					if(OffsideTapped != null)
					{
						OffsideTapped();
					}
				}
				else
				{
					if (Tapped != null)
					{
						Tapped();
					}	
				}
			}
			base.TouchesBegan(touches, evt);
		}

		private UIImageView _detailsImageView;
		public override void Draw(RectangleF rect)
		{
			if(_detailsImageView != null)
			{
				_detailsImageView.Image.Dispose();
				_detailsImageView.Image = null;
				_detailsImageView.Dispose();
				_detailsImageView = null;
			}
						
			var highResolution = Util.IsRetina();
			var size = highResolution ? new SizeF(rect.Width * 2, rect.Height * 2) : new SizeF(rect.Width, rect.Height);
			
			UIGraphics.BeginImageContext(size);
			var context = UIGraphics.GetCurrentContext();	
			DrawInContext (rect, context, highResolution);
			var details = UIGraphics.GetImageFromCurrentImageContext();
			context.Dispose();
			UIGraphics.EndImageContext();
			
			_detailsImageView = new UIImageView(details);
			details.Dispose();
			_detailsImageView.Frame = rect;
			this.AddSubview(_detailsImageView);
		}

		private void DrawInContext(RectangleF rect, CGContext context, bool isRetina)
		{
			context.SaveState();
			context.SetFillColor(0, 0, 0, 1);
			context.SetShadowWithColor(new SizeF (1, -1), 1.0f, UIColor.White.CGColor);
		
			DrawTitleInContext(rect, isRetina);
			DrawMediaInContext(rect, isRetina);
			DrawPhysicalDetailsInContext(rect, isRetina);
			DrawLimitedDetailsInContext(rect, isRetina);
			
			// Border defaults
			var left = 9;
			var top = 9 + _y;
			var width = 0.5f;
			var size = _size;
			
			if(isRetina)
			{
				left *= 2;
				top *= 2;
				width /= 2;
				size *= 2;
			}
			
			var borderPath = ImageHelper.MakeRoundedPath(size + 2, 4);
			
			context.RestoreState();
			context.TranslateCTM(left + (Util.IsRetina () ? _offset * 2 : _offset), top);
			context.AddPath(borderPath);
			context.SetStrokeColor(0.5f, 0.5f, 0.5f, 1);
			context.SetLineWidth(width);
			context.StrokePath();
		}

		private void DrawMediaInContext (RectangleF rect, bool isRetina)
		{
			// Set defaults
			var size = _mediaSize;
			var left = _left;
			var mediaY = 34 + _y - 4;
			var mediaSizeWidthMax = 145;
			var width = rect.Width - left;
			
			if(isRetina)
			{
				size *= 2;
				left *= 2;
				mediaY *= 2;
				mediaSizeWidthMax *= 2;
				_mediaSize *= 2;
				width *= 2;
			}
			
			var mediaFont = UIFont.SystemFontOfSize(size);
			var mediaSize = new NSString(_piece.Media).StringSize(mediaFont);
			while(mediaSize.Width > mediaSizeWidthMax)
			{
				mediaFont = UIFont.SystemFontOfSize(size--);
				mediaSize = new NSString(_piece.Media).StringSize(mediaFont);
				mediaY += (size % 3 == 0) ? 1 : 0;
			}
			
			var mediaArea = new RectangleF(left + (Util.IsRetina() ? _offset * 2 : _offset), mediaY, width, _mediaSize);
			DrawString(_piece.Media, mediaArea, mediaFont);
		}

		private void DrawPhysicalDetailsInContext (RectangleF rect, bool isRetina)
		{
			// Set defaults
			var left = _left;
			var top = 47 + _y;
			var size = _physicalSize;
			var width = rect.Width - left;
			
			// Modify defaults if displaying @2x
			if(isRetina)
			{
				left *= 2;	
				top *= 2;
				size *= 2;
				width *= 2;
			}
			
			var physical = string.Concat(_piece.Height, "\" x ", _piece.Width, "\" (", _piece.Year, ")");	
			var font = UIFont.SystemFontOfSize(size);
			DrawString(physical, new RectangleF(left + (Util.IsRetina() ? _offset * 2 : _offset), top, width, size), font, UILineBreakMode.TailTruncation);
		}
		
		private void DrawLimitedDetailsInContext(RectangleF rect, bool isRetina)
		{
			if(!_piece.LimitedPrints.HasValue)
			{
				return;
			}
			
			UIColor.FromRGB(146, 27, 39).SetColor();
			
			var left = _left;
			var top = 47 + 18 + _y;
			var size = _physicalSize - 2;
			var width = rect.Width - left;
			
			if(isRetina)
			{
				left *= 2;	
				top *= 2;
				size *= 2;
				width *= 2;
			}
			
			var limited = string.Format("Limited edition, {0} in series", _piece.LimitedPrints.Value);
			var font = UIFont.SystemFontOfSize(size);
			DrawString(limited, new RectangleF(left + (Util.IsRetina() ? _offset * 2 : _offset), top, width, size), font, UILineBreakMode.TailTruncation);
		}

		private void DrawTitleInContext (RectangleF rect, bool isRetina)
		{
			// Set defaults
			var titleMaxWidth = 145;
			var titleTop = 12 + _y - 4;
			var left = _left;
			var width = rect.Width - left;
			
			if(isRetina)
			{
				_titleSize *= 2;
				titleMaxWidth *= 2;
				titleTop *= 2;
				left *= 2;
				width *= 2;
			}
			
			var size = _titleSize;
			var titleFont = UIFont.BoldSystemFontOfSize(size);
			var titleSize = new NSString(_piece.Title).StringSize(titleFont);
			while(titleSize.Width > titleMaxWidth)
			{
				titleFont = UIFont.BoldSystemFontOfSize(size--);
				titleSize = new NSString(_piece.Title).StringSize(titleFont);
			}
			var titleArea = new RectangleF(left + (Util.IsRetina() ? _offset * 2 : _offset), titleTop, titleSize.Width/* width */, _titleSize);
			
			DrawString(_piece.Title, titleArea, titleFont, UILineBreakMode.TailTruncation);
		}
	}
}