using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;

namespace ArtApp
{
	public enum CropPosition
	{
		Start,
		Center,
		End
	}	
	
	public static class ImageHelper
	{
		// http://stackoverflow.com/questions/3869692/iphone-flattening-a-uiimageview-and-subviews-to-image-blank-image
		public static UIImage FlattenToImage(UIView view)
		{		
			UIImage image;
		    UIGraphics.BeginImageContextWithOptions(view.Bounds.Size, view.Opaque, UIScreen.MainScreen.Scale);
			var context = UIGraphics.GetCurrentContext();
		    view.Layer.RenderInContext(context);
		    image = UIGraphics.GetImageFromCurrentImageContext();
			context.Dispose();
		    UIGraphics.EndImageContext();		
		    return image;
		}
		
		public static UIImage Crop(UIImage image, RectangleF rect)
		{
		    if (image.CurrentScale > 1.0f)
			{
		        rect = new RectangleF(rect.X * image.CurrentScale,
			                          rect.Y * image.CurrentScale,
			                          rect.Size.Width * image.CurrentScale,
			                          rect.Size.Height * image.CurrentScale);
		    }
		
			var cgImage = image.CGImage;
			var imageRef = cgImage.WithImageInRect(rect);
		    cgImage.Dispose();
			
			UIImage result = new UIImage(imageRef, image.CurrentScale, image.Orientation);
		    imageRef.Dispose();			
			return result;
		}
		
		public static UIImage CopyAndDispose(UIImage original)
		{
			UIGraphics.BeginImageContextWithOptions(original.Size, false, 0);
			original.Draw(new RectangleF(0, 0, original.Size.Width, original.Size.Height));
			UIImage copy = UIGraphics.GetImageFromCurrentImageContext();
		  	UIGraphics.EndImageContext();	
		    original.Dispose();
			return copy;
		}
		
		public static UIImage Stitch(UIImage left, UIImage right)
		{
			UIGraphics.BeginImageContext(new SizeF(left.Size.Width + right.Size.Width, left.Size.Height));
			left.Draw(new RectangleF(0, 0, left.Size.Width,left.Size.Height));
			right.Draw(new RectangleF(left.Size.Width, 0, right.Size.Width, right.Size.Height), CGBlendMode.Normal, 1.0f);
			UIImage stitched = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();			
			return stitched;
		}
		
		// Original source: http://stackoverflow.com/questions/8467141/ios-how-to-achieve-emboss-effect-for-the-text-on-uilabel
		public static UIImage MaskImage(UIImage image, UIImage maskImage)
		{
			var maskRef = maskImage.CGImage; 
		 
			var mask = CGImage.CreateMask(maskRef.Width,
				maskRef.Height,
				maskRef.BitsPerComponent,
				maskRef.BitsPerPixel,
				maskRef.BytesPerRow,
				maskRef.DataProvider, 
			    null, false);
		 
			// CGImages return new instances too!
			var cgImage = image.CGImage;
			var masked = cgImage.WithMask(mask);
			cgImage.Dispose();
			mask.Dispose();
			
			var img = UIImage.FromImage(masked);
			masked.Dispose();
			maskRef.Dispose();
			
			UIGraphics.BeginImageContext(img.Size);
			img.Draw(new RectangleF(0, 0, img.Size.Width, img.Size.Height));
			UIImage copy = UIGraphics.GetImageFromCurrentImageContext();
		  	UIGraphics.EndImageContext();
		  	
			img.Dispose();
			return copy;
		}
		
		public static UIImage ImageFromLayer(CALayer layer)
		{
			UIGraphics.BeginImageContext(layer.Frame.Size);
			layer.RenderInContext(UIGraphics.GetCurrentContext());
			UIImage outputImage = UIGraphics.GetImageFromCurrentImageContext();
		  	UIGraphics.EndImageContext();
		  	return outputImage;
		}
			
		public static UIImage RoundAndSquare(UIImage image, int radius)
		{
			UIGraphics.BeginImageContext(image.Size);
			var c = UIGraphics.GetCurrentContext();
			
			var size = image.Size.Width;
			c.AddPath(MakeRoundedPath(size, radius));
			c.Clip();

			var cg = image.CGImage;
			float width = cg.Width;
			float height = cg.Height;
			if (width != height)
			{
				float x = 0, y = 0;
				if (width > height)
				{
					x = (width-height)/2;
					width = height;
				}
				else 
				{
					y = (height-width)/2;
					height = width;
				}
				
				c.ScaleCTM (1, -1);
				using (var copy = cg.WithImageInRect(new RectangleF (x, y, width, height)))
				{
					c.DrawImage(new RectangleF (0, 0, size, -size), copy);
					cg.Dispose();
				}
			}
			else 
			{
				image.Draw (new RectangleF (0, 0, size, size));
			}
			
			var converted = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return converted;	
		}
 
		public static UIImage ImageToFitSize(this UIImage image, SizeF fitSize)
		{
			double imageScaleFactor = 1.0;
			imageScaleFactor = image.CurrentScale;
 
			double sourceWidth = image.Size.Width * imageScaleFactor;
			double sourceHeight = image.Size.Height * imageScaleFactor;
			double targetWidth = fitSize.Width;
			double targetHeight = fitSize.Height;
 
			double sourceRatio = sourceWidth / sourceHeight;
			double targetRatio = targetWidth / targetHeight;
 
			bool scaleWidth = (sourceRatio <= targetRatio);
			scaleWidth = !scaleWidth;
 
			double scalingFactor, scaledWidth, scaledHeight;
 
			if (scaleWidth)
			{
				scalingFactor = 1.0 / sourceRatio;
				scaledWidth = targetWidth;
				scaledHeight = Math.Round(targetWidth * scalingFactor);
			}
			else
			{
				scalingFactor = sourceRatio;
				scaledWidth = Math.Round(targetHeight * scalingFactor);
				scaledHeight = targetHeight;
			}
 
			RectangleF destRect = new RectangleF (0, 0, (float)scaledWidth, (float)scaledHeight);
 
			UIGraphics.BeginImageContextWithOptions(destRect.Size, false, 0.0f);
			image.Draw(destRect); 
			UIImage newImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
 			
			return newImage;
		}
		
		public static CGPath MakeRoundedPath(float size, float radius)
		{
			float hsize = size/2;
			
			var path = new CGPath ();
			path.MoveToPoint (size, hsize);
			path.AddArcToPoint (size, size, hsize, size, radius);
			path.AddArcToPoint (0, size, 0, hsize, radius);
			path.AddArcToPoint (0, 0, hsize, 0, radius);
			path.AddArcToPoint (size, 0, size, hsize, radius);
			path.CloseSubpath ();
			
			return path;
		}			

		public static UIImage CropResize(UIImage image, float width, float height, CropPosition position)
		{
			UIImage modifiedImage = image;
			
			SizeF ImgSize = modifiedImage.Size;

			if (ImgSize.Width < width)
			{
				width = ImgSize.Width;
			}

			if (ImgSize.Height < height)
			{
				height = ImgSize.Height;
			}

			float crop_x = 0;
			float crop_y = 0;
			if (ImgSize.Width / width < ImgSize.Height / height)
			{
				var cur_width = modifiedImage.Size.Width;
				if (cur_width > width)
				{
					var ratio = width / cur_width;
					var height2 = modifiedImage.Size.Height * ratio;
					
					var beforeResizeWidth = modifiedImage;
					UIGraphics.BeginImageContext (new SizeF (width, height2));
					modifiedImage.Draw(new RectangleF (0, 0, width, height2));
					modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
					UIGraphics.EndImageContext();
					beforeResizeWidth.Dispose();
				}
				
				ImgSize = modifiedImage.Size;

				if (position == CropPosition.Center)
				{
					crop_y = (ImgSize.Height / 2) - (height / 2);
				}
				if (position == CropPosition.End)
				{
					crop_y = ImgSize.Height - height;
				}
			}
			else
			{
				var cur_height = modifiedImage.Size.Height;
				if (cur_height > height)
				{
					var ratio = height / cur_height;
					var width2 = modifiedImage.Size.Width * ratio;
					
					var beforeResizeHeight = modifiedImage;
					UIGraphics.BeginImageContext (new SizeF (width2, height));
					modifiedImage.Draw(new RectangleF (0, 0, width2, height));
					modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
					UIGraphics.EndImageContext ();
					beforeResizeHeight.Dispose ();
				}				
				
				ImgSize = modifiedImage.Size;

				if (position == CropPosition.Center)
				{
					crop_x = (ImgSize.Width / 2) - (width / 2);
				}
				if (position == CropPosition.End)
				{
					crop_x = ImgSize.Width - width;
				}
			}
			
			var beforeFinal = modifiedImage;
			UIGraphics.BeginImageContext (new SizeF (width, height));
			CGContext context = UIGraphics.GetCurrentContext ();
			RectangleF clippedRect = new RectangleF (0, 0, width, height);
			context.ClipToRect (clippedRect);
			RectangleF drawRect = new RectangleF (-crop_x, -crop_y, ImgSize.Width, ImgSize.Height);
			modifiedImage.Draw (drawRect);
			modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			context.Dispose ();
			beforeFinal.Dispose();
			return modifiedImage;
		}
		
		public static void AddRoundedRectToPath(CGContext context, RectangleF rect, float ovalWidth, float ovalHeight)
		{
			float fw, fh;
			if (ovalWidth == 0 || ovalHeight == 0)
			{
				context.AddRect(rect);
				return;
			}
			context.SaveState();
			context.TranslateCTM(rect.GetMinX(), rect.GetMinY());
			context.ScaleCTM(ovalWidth, ovalHeight);
			fw = rect.Width / ovalWidth;
			fh = rect.Height / ovalHeight;
			context.MoveTo(fw, fh / 2);
			context.AddArcToPoint(fw, fh, fw / 2, fh, 1);
			context.AddArcToPoint(0, fh, 0, fh / 2, 1);
			context.AddArcToPoint(0, 0, fw / 2, 0, 1);
			context.AddArcToPoint(fw, 0, fw, fh / 2, 1);
			context.ClosePath();
			context.RestoreState();
		}
		 
		public static UIImage MakeRoundCornerImage(UIImage img, int cornerWidth, int cornerHeight)
		{
			UIImage newImage = null;
			 
			if (null != img)
			{
				var w = img.Size.Width;
				var h = img.Size.Height;
				 
				CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
				CGContext context = new CGBitmapContext(null, (int)w, (int)h, 8, (int)(4 * w), colorSpace, CGImageAlphaInfo.PremultipliedFirst);
				
				context.BeginPath();
				var rect = new RectangleF(0, 0, img.Size.Width, img.Size.Height);
				AddRoundedRectToPath (context, rect, cornerWidth, cornerHeight);
				context.ClosePath();
				context.Clip();
				
				var cgImage = img.CGImage;
				context.DrawImage (new RectangleF(0, 0, w, h), cgImage);
				cgImage.Dispose();
				
				CGImage imageMasked = ((CGBitmapContext)context).ToImage();
				context.Dispose();
				colorSpace.Dispose();
				newImage = new UIImage(imageMasked);
				imageMasked.Dispose();
			}
			 
			return newImage;
		}
	}
}

