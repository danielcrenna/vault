using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;

namespace ArtApp
{
	public class ImageFactory
	{	
		// In this case since we're manually building the background, we need actual pixels for retina displays
		private static readonly float GalleryScreenHeight = (UIScreen.MainScreen.Bounds.Height * (Util.IsRetina() ? 2 : 1));		
		private static readonly float GalleryScreenWidth = (UIScreen.MainScreen.Bounds.Width) * (Util.IsRetina() ? 2 : 1);
		
		public static string GetImageKey(Piece piece, float width, float height)
		{
			return string.Concat(piece.Title.ToLower(), "_", piece.Year, "_", width, "x", height, DimensionSet.IsRetinaDisplay ? "@2x" : "", Util.IsPad() ? "_ipad" : "_iphone");	
		}
		
		public static string GetImageKey(Artist artist, float width, float height)
		{
			return string.Concat(artist.FirstName.ToLowerInvariant(), ".", artist.LastName.ToLowerInvariant());	
		}
		
		public static UIImage LoadMapThumbnail(Piece piece)
		{
			return ImageCache.Get(GetImageKey(piece, 27, 28), () => GeneratePieceThumbnail(piece, 27, 28));
		}
		
		public static UIImage LoadRoundedThumbnail(Artist artist)
		{
			var s = DimensionSet.ListThumbnailSquare;
			
			return ImageCache.Get(GetImageKey(artist, s, s), () => GeneratePieceThumbnail(artist, s, s));
		}
		
		public static UIImage LoadRoundedThumbnail(Piece piece)
		{
			var s = DimensionSet.ListThumbnailSquare;
			
			return ImageCache.Get(GetImageKey(piece, s, s), () => GeneratePieceThumbnail(piece, s, s));
		}
		
		// Static because we want to cache the stretchy part
		private static UIImage _shadowMask;
		private static UIImage _shadowLeft;
		private static UIImage _shadowRight;
		private static UIImage _shadowTop;		
		static ImageFactory()
		{
			_shadowMask = UIImage.FromBundle("Images/gallery/shadowMask.png").StretchableImage(5, 0);
			_shadowLeft = UIImage.FromBundle("Images/gallery/left_shadow.png").StretchableImage(0, 7);
			_shadowRight = UIImage.FromBundle("Images/gallery/right_shadow.png").StretchableImage(0, 7);
			_shadowTop = UIImage.FromBundle("Images/gallery/top_shadow.png").StretchableImage(2, 0);
		}		
		
		public static GalleryImageView BuildGalleryPage(Piece piece, int index, int total, out RectangleF canvasFrame)
		{
			float width;
			float height;
			float gutter;				
			GetGalleryArea(out width, out height, out gutter);

			// Background width is 1280	
			var backgroundImage = BuildGalleryBackground(Util.IsLandscape());
			UIImage background = GetSeamlessBackgroundSlice(index, backgroundImage);
			var imageView = new GalleryImageView(background); // UIImageView
			background.Dispose();
			backgroundImage.Dispose();
			
			// Add canvas
			UIImage canvas = ImageCache.Get(GetImageKey(piece, width * 2, height * 2), ()=> GenerateGalleryPiece(piece, width * 2, height * 2));
			var canvasView = new UIImageView(canvas);
			canvasView.Center = new PointF(width / 2, (height / 2) - gutter);
			canvasView.Layer.MasksToBounds = false;
			var canvasViewFrame = canvasView.Frame;
			canvasFrame = canvasViewFrame;
							
			// Add reflection
			float reflectionY = GetReflectionY();			
			var reflection = GetOrGenerateReflection(piece, canvasViewFrame, canvas, reflectionY);
			canvas.Dispose();				
		
			var reflectionFrame = new RectangleF(canvasViewFrame.X, reflectionY, reflection.Size.Width, reflection.Size.Height);
			var reflectionView = new UIImageView(reflection);
			reflectionView.Frame = reflectionFrame;
			reflectionView.Layer.Opacity = 0.30f;
			reflection.Dispose();
			
			// Add shadow
			var shadowY = canvasViewFrame.Y + canvasViewFrame.Height - 10;
			var shadowView = new UIImageView(_shadowMask);
			shadowView.Frame = new RectangleF(canvasViewFrame.X, shadowY, canvasViewFrame.Width, _shadowMask.Size.Height);	
			var shadowLeftView = new UIImageView(_shadowLeft);
			shadowLeftView.Frame = new RectangleF(canvasViewFrame.X - 3, canvasViewFrame.Y, 3, canvasViewFrame.Height);			
			var shadowRightView = new UIImageView(_shadowRight);
			shadowRightView.Frame = new RectangleF(canvasViewFrame.X + canvasViewFrame.Width, canvasViewFrame.Y, 3, canvasViewFrame.Height);
			var shadowTopView = new UIImageView(_shadowTop);
			shadowTopView.Frame = new RectangleF(canvasViewFrame.X, canvasViewFrame.Y, canvasViewFrame.Width, _shadowTop.Size.Height);
			
			// Add image layers together
			imageView.AddSubview(shadowView);
			imageView.AddSubview(shadowLeftView);
			imageView.AddSubview(shadowRightView);
			imageView.AddSubview(canvasView);
			imageView.AddSubview(shadowTopView);
			imageView.AddSubview(reflectionView);
			
			var lighting = BuildLighting();
			var lightingView = new LightImageView(lighting);
			lighting.Dispose();
			imageView.AddSubview(lightingView);
			imageView.Lights = lightingView;
			return imageView;
		}

		private static UIImage BuildLighting()
		{
			var landscape = Util.IsLandscape();			
			UIImage lighting;			
			if(Util.IsPhone() && landscape)
			{
			    lighting = UIImage.FromBundle("Images/gallery/lighting-iphone-Landscape.png");
			}
			else
			{
				lighting = UIImageEx.FromIdiomBundleForBackground("Images/gallery/lighting.png");	
			}
			
		    return ImageHelper.CopyAndDispose(lighting);
		}

		private static float GetReflectionY()
		{
			float reflectionY;
			if(Util.IsPad())
			{
				if(Util.IsPortrait())
				{
					reflectionY = 907.5f;	
				}
				else
				{
					reflectionY = 667.5f;
				}				
			}
			else				
			{
				if(Util.IsRetina())
				{					
					if(Util.IsPortrait())
					{
						reflectionY = 413.5f;
					}
					else
					{
						reflectionY = 269.5f;
					}
				}
				else
				{
					if(Util.IsPortrait())
					{
						reflectionY = 410;
					}
					else
					{
						reflectionY = 266;
					}
				}
			}
			return reflectionY;
		}
		
		private static UIImage GetSeamlessBackgroundSlice(int index, UIImage background)
		{
			UIImage final;
						
			if(Util.IsPhone())
			{
				if(Util.IsPortrait())
				{
					var even = index % 2 == 0;
					final = even ? BuildCroppedGalleryBackground(background.Size.Width / 2, index, false) : 
						           BuildCroppedGalleryBackground(background.Size.Width / 2, background.Size.Width / 2, index, false);
				}
				else
				{
					if(!Util.IsRetina())
					{
						// Width is 480; split the background into slabs based on index										
						if(index == 0)
						{
							final = CutSlice(background, 1280 / 2, 0, 960 / 2);			
						}
						else if(index == 1)
						{
							final = CutSlice (background, 1280 / 2, 960 / 2, 960 / 2);
						}
						else if (index == 2)
						{
							final = CutSlice (background, 1280 / 2, 640 / 2, 960 / 2);
						}
						else if (index == 3)
						{
							final = CutSlice (background, 1280 / 2, 320 / 2, 960 / 2);
						}
						else if (index == 4)
						{
							final = CutSlice (background, 1280 / 2, 0, 960 / 2);
						}
						else if (index == 5) // Repeats
						{
							final = CutSlice (background, 1280 / 2, 960 / 2, 960 / 2);
						}
						else if (index == 6)
						{
							final = CutSlice (background, 1280 / 2, 640 / 2, 960 / 2);
						}
						else if (index == 7)
						{
							final = CutSlice (background, 1280 / 2, 320 / 2, 960 / 2);
						}
						else if (index == 8)
						{
							final = CutSlice (background, 1280 / 2, 0, 960 / 2);
						}
						else if (index == 9)
						{
							final = CutSlice (background, 1280 / 2, 960 / 2, 960 / 2);
						}
						else if (index == 10)
						{
							final = CutSlice (background, 1280 / 2, 640 / 2, 960 / 2);
						}
						else if (index == 11)
						{
							final = CutSlice (background, 1280 / 2, 320 / 2, 960 / 2);
						}
						else if (index == 12)
						{
							final = CutSlice (background, 1280 / 2, 0, 960 / 2);
						}
						else
						{
							final = CutSlice(background, 1280 / 2, 0, 960 / 2);
						}
					}
					else
					{
						// Width is 960; split the background into slabs based on index										
						if(index == 0)
						{
							final = CutSlice(background, 1280, 0, 960);			
						}
						else if(index == 1)
						{
							final = CutSlice (background, 1280, 960, 960);
						}
						else if (index == 2)
						{
							final = CutSlice (background, 1280, 640, 960);
						}
						else if (index == 3)
						{
							final = CutSlice (background, 1280, 320, 960);
						}
						else if (index == 4)
						{
							final = CutSlice (background, 1280, 0, 960);
						}
						else if (index == 5) // Repeats
						{
							final = CutSlice (background, 1280, 960, 960);
						}
						else if (index == 6)
						{
							final = CutSlice (background, 1280, 640, 960);
						}
						else if (index == 7)
						{
							final = CutSlice (background, 1280, 320, 960);
						}
						else if (index == 8)
						{
							final = CutSlice (background, 1280, 0, 960);
						}
						else if (index == 9)
						{
							final = CutSlice (background, 1280, 960, 960);
						}
						else if (index == 10)
						{
							final = CutSlice (background, 1280, 640, 960);
						}
						else if (index == 11)
						{
							final = CutSlice (background, 1280, 320, 960);
						}
						else if (index == 12)
						{
							final = CutSlice (background, 1280, 0, 960);
						}
						else
						{
							final = CutSlice(background, 1280, 0, 960);;	
						}
					}
				}
			}
			else
			{
				if(Util.IsLandscape())
				{
					// Width is 1024; split the background into slabs based on index
					var left = ImageHelper.Crop(background, new RectangleF(0, 0, 1024, background.Size.Height));
					
					if(index == 0)
					{
						final = left;	
					}
					else if(index == 1)
					{
						final = CutSlice(background, 1280, 1024, 1024);
					}
					else if(index == 2)
					{
						final = CutSlice(background, 1280, 768, 1024);
					}
					else if(index == 3)
					{
						final = CutSlice (background, 1280, 512, 1024);
					}
					else if(index == 4)
					{
						final = CutSlice(background, 1280, 512, 1024);
					}
					else if(index == 5)
					{
						final = CutSlice (background, 1280, 512, 1024);
					}
					else if(index == 6)
					{
						final = CutSlice (background, 1280, 512, 1024);
					}
					else if(index == 7)
					{
						final = CutSlice (background, 1280, 512, 1024);
					}
					else if(index == 8)
					{
						final = CutSlice (background, 1280, 512, 1024);
					}
					else if(index == 9)
					{
						final = CutSlice (background, 1280, 512, 1024);
					}
					else
					{
						return left;	
					}
					
					left.Dispose();
				}
				else
				{
					// Width is 768; split the background into slabs based on index
					var left = ImageHelper.Crop(background, new RectangleF(0, 0, 768, background.Size.Height));
					
					if(index == 0)
					{
						final = left;	
					}
					else if(index == 1)
					{
						final = CutSlice (background, 1280, 768, 768);
					}
					else if(index == 2)
					{
						final = CutSlice (background, 1280, 256, 768);
					}
					else if(index == 3)
					{
						final = CutSlice (background, 1280, 1024, 768);
					}
					else if(index == 4)
					{
						final = CutSlice (background, 1280, 512, 768);
					}
					else if(index == 5)
					{
						return left;
					}
					else if(index == 6)
					{
						final = CutSlice (background, 1280, 768, 768);
					}
					else if(index == 7)
					{
						final = CutSlice (background, 1280, 256, 768);
					}
					else if(index == 8)
					{
						final = CutSlice (background, 1280, 1024, 768);
					}
					else if(index == 9)
					{
						final = CutSlice (background, 1280, 512, 768);
					}
					else
					{
						return left;	
					}
					
					left.Dispose();
				}
			}
			
			return final;
		}
		
		public static UIImage CutSlice(UIImage background, int totalWidth, int leftStart, int sliceWidth)
		{
			var leftSpan = totalWidth - leftStart;
			if(leftSpan >= sliceWidth)
			{
				leftSpan = sliceWidth;	
			}
			var cl = ImageHelper.Crop(background, new RectangleF(leftStart, 0, leftSpan, background.Size.Height));
			
			if(leftSpan == sliceWidth)
			{
				return cl;
			}
			
			var rightSpan = sliceWidth - leftSpan;
			var cr = ImageHelper.Crop(background, new RectangleF(0, 0, rightSpan, background.Size.Height));
			
			var stitched = ImageHelper.Stitch(cl, cr);	// Disposes both images internally
			cl.Dispose();
			cr.Dispose();
			return stitched;
		}

		public static UIImage BuildGalleryBackground(bool landscape)
		{
			var key = "gallery_background_" + (landscape ? "landscape" : "portrait");
						
			return ImageCache.Get(key, ()=>
			{
				UIImage background0;
				if(Util.IsRetina() || Util.IsPad())
				{
					background0 = UIImageEx.FromFile("Images/gallery/background_seamless_high.png");
				}
				else
				{
					background0 = UIImageEx.FromFile("Images/gallery/background_seamless_low.png");
				}
				
				// Snip the background height so we don't shrink the viewable area
				var delta = background0.Size.Height - (Util.IsPortrait() ? GalleryScreenHeight : GalleryScreenWidth);// - 20;
				var frame = new RectangleF(0, delta, background0.Size.Width, background0.Size.Height - delta);
				var background1 = ImageHelper.Crop(background0, frame);
				background0.Dispose();
				
				return background1;
			});
		}
		
		public static UIImage BuildCroppedGalleryBackground()
		{
			var width = Util.PixelScreenWidth;
			return BuildCroppedGalleryBackground(width, -1, Util.IsLandscape());
		}
		
		public static UIImage BuildCroppedGalleryBackground(bool landscape)
		{
			var width = Util.IsPad() ? landscape ? 1024 : 768 :
					   landscape ? Util.IsRetina() ? 960 : 480 : 
					   Util.IsRetina() ? 640 : 320;
			
			return BuildCroppedGalleryBackground(width, -1, landscape);
		}
		
		public static UIImage BuildCroppedGalleryBackground(float width, int index, bool landscape)
		{
			return BuildCroppedGalleryBackground(0, width, index, landscape);
		}
		
		public static UIImage BuildCroppedGalleryBackground(float left, float width, int index, bool landscape)
		{
			var key = "gallery_background_" + left + "_" + landscape + "_" + width;// + "_" + index;
			return ImageCache.Get(key,()=>
			{
				var image0 = ImageFactory.BuildGalleryBackground(landscape);
				var image1 = ImageHelper.Crop(image0, new RectangleF(left, 0, width, landscape ? image0.Size.Width : image0.Size.Height));
				image0.Dispose();
				return image1; 
			});
		}
		
		private static UIImage GetOrGenerateReflection(Piece piece, RectangleF canvasFrame, UIImage canvas, float reflectionY)
		{
			// Doesn't work when saved to disk
			return GenerateReflection(canvasFrame, canvas, reflectionY);
		}

		private static UIImage GenerateReflection(RectangleF canvasFrame, UIImage canvas, float reflectionY)
		{
			var reflectionHeight = Util.IsPad() ? 85 : 60;
			reflectionHeight = Util.IsPortrait() ? reflectionHeight : reflectionHeight - 20;
			
			// Build reflection and crop the upper image
			var reflection0 = BuildCoverFlow(canvas, 1.0f);
			var areaWithoutOriginal = new RectangleF(0, canvasFrame.Height, canvasFrame.Width, canvasFrame.Height);
			var reflection1 = ImageHelper.Crop(reflection0, areaWithoutOriginal);
			var reflection2 = ImageHelper.Crop(reflection1, new RectangleF(0, 0, canvasFrame.Width, reflectionHeight));	
			reflection0.Dispose();
			reflection1.Dispose();			
			
			var reflectionFrame = new RectangleF(canvasFrame.X, reflectionY, reflection2.Size.Width, reflection2.Size.Height);
			
			// Create the gradient used for the opacity mask (mask image must have no alpha channel, black is opaque, white is transparent)
			var black = UIColor.Black.CGColor;
			var white = UIColor.White.CGColor;
			CAGradientLayer gradient = new CAGradientLayer();
			gradient.Frame = reflectionFrame;
			gradient.Colors = new CGColor[] { black, white, white };
			
			// Mask the reflection image
			var maskImage = ImageHelper.ImageFromLayer(gradient);
			gradient.Dispose();
			
			var reflection3 = ImageHelper.MaskImage(reflection2, maskImage);
			reflection2.Dispose();
			maskImage.Dispose();

			return reflection3;
		}
		
		public static UIImage BuildCoverFlow(UIImage image, float reflectionFraction)
		{
			int reflectionHeight = (int) (image.Size.Height * reflectionFraction);

			// gradient is always black and white and the mask must be in the gray colorspace
			var colorSpace = CGColorSpace.CreateDeviceGray();

			// Create the bitmap context
			var gradientBitmapContext = new CGBitmapContext (IntPtr.Zero, 1, reflectionHeight, 8, 0, colorSpace, CGImageAlphaInfo.None);

			// define the start and end grayscale values (with the alpha, even though
			// our bitmap context doesn't support alpha the gradien requires it)
			float [] colors = { 0, 1, 1, 1 };

			// Create the CGGradient and then release the gray color space
			var grayScaleGradient = new CGGradient (colorSpace, colors, null);
			colorSpace.Dispose();

			// create the start and end points for the gradient vector (straight down)
			var gradientStartPoint = new PointF (0, reflectionHeight);
			var gradientEndPoint = PointF.Empty;

			// draw the gradient into the gray bitmap context
			gradientBitmapContext.DrawLinearGradient (grayScaleGradient, gradientStartPoint, gradientEndPoint, CGGradientDrawingOptions.DrawsAfterEndLocation);
			grayScaleGradient.Dispose();

			// Add a black fill with 50% opactiy
			gradientBitmapContext.SetFillColor(0, 0.5f);
			gradientBitmapContext.FillRect (new RectangleF (0, 0, 1, reflectionHeight));

            // conver the context into a CGImage and release the context
			var gradientImageMask = gradientBitmapContext.ToImage();
			gradientBitmapContext.Dispose();

			// create an image by masking the bitmap of the mainView content with the gradient view
			// then release the pre-masked content bitmap and the gradient bitmap
			var cgImage = image.CGImage;
			var reflectionImage = cgImage.WithMask(gradientImageMask);
			cgImage.Dispose();
			gradientImageMask.Dispose();

			var size = new SizeF(image.Size.Width, image.Size.Height + reflectionHeight);
			
			UIGraphics.BeginImageContext(size);
			image.Draw(PointF.Empty);
			var context = UIGraphics.GetCurrentContext();
			context.DrawImage(new RectangleF(0, image.Size.Height, image.Size.Width, reflectionHeight), reflectionImage);

			var result = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			reflectionImage.Dispose();

			return result;
		}

		private static void GetGalleryArea(out float width, out float height, out float gutter)
		{
			if(Util.IsPortrait())
			{
				if(Util.IsPad())
				{
					width = 768;
					height = 44 + 911 + 49;
					gutter = 20;
				}
				else
				{
					width = 320;
					height = 460;
					gutter = 15;
				}				
			}
			else
			{
				if(Util.IsPad())
				{
					width = 1024;
					height = 44 + 655 + 49;  
					gutter = 50;
				}
				else
				{
					width = 480;
					height = 32 + 219 + 49;
					gutter = 15;
				}				
			}
		}
		
		public static UIImage GeneratePieceThumbnail(Artist artist, float w, float h)
		{
			return GeneratePieceThumbnail(artist.Image, w, h, true, true);
		}
		
		public static UIImage GeneratePieceThumbnail(Piece piece, float w, float h)
		{
			return GeneratePieceThumbnail(piece.Source, w, h, true, true);
		}
		
		public static UIImage GeneratePieceThumbnail(string source, float w, float h, bool rounded, bool notDetailView)
		{
			// If we're using the retina display, we need to double the resolution,
			// even though when saving the image we still use 32x as the basis
			var sw = w;
			var sh = h;
			if(DimensionSet.IsRetinaDisplay)
			{
				sw *= 2f;
				sh *= 2f;
			}
			
			// On the iPad, the thumbnail is rendered too large as-is
			if(DimensionSet.Idiom == UIUserInterfaceIdiom.Pad && notDetailView)
			{
				sw *= 2f;
				sh *= 2f;
				w /= 2.5f;
				h /= 2.5f;
			}
			
			var image = UIImageEx.FromFile(source); // Don't EVER bundle cache the source file!!
			var small = ImageHelper.CropResize(image, sw, sh, CropPosition.Center);
			image.Dispose();
									
			UIImage thumb;
			if(rounded)
			{
			    var roundThumb = ImageHelper.MakeRoundCornerImage(small, 5, 5);
				thumb = ImageHelper.ImageToFitSize(roundThumb, new SizeF(w, h));
				roundThumb.Dispose();
			}
			else
			{
				thumb = ImageHelper.ImageToFitSize(small, new SizeF(w, h));
			}				
			small.Dispose();
			return thumb;
		}

		public static UIImage GenerateGalleryPiece(Piece piece, float width, float height)
		{
			if(Util.IsPad())
			{
				if(Util.IsLandscape())
				{
					// iPad Landscape
					width = 720;
					height = 515;
				}
				else
				{
					// iPad Portrait
					width = 440;
					height = 605;
				}				
			}
			else
			{
				if(Util.IsLandscape())
				{
					// iPhone Landscape
					width = 300;
					height = 190;
				}
				else
				{
					// iPhone Portrait
					width = 200;
					height = 275;
				}
			}			
			
			var image = UIImageEx.FromFile(piece.Source);
			var final = ImageHelper.ImageToFitSize(image, new SizeF(width, height));
			image.Dispose();
			return final;
		}
	}
}

