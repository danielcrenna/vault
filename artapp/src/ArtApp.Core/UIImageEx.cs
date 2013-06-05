using System;
using MonoTouch.UIKit;
using System.IO;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace ArtApp
{
	public static class UIImageEx
	{ 
		public static UIImage FromFile(string path)
		{
			return UIImage.FromFileUncached(path);	
		}
		
		public static UIImage FromIdiomBundleForBackground(string path)
		{
			return FromIdiomBundleForBackground(path, Util.IsLandscape());
		}
		
		public static UIImage FromIdiomBundleForBackground(string path, bool isLandscape)
		{
			string filename = Path.GetFileNameWithoutExtension(path);
			string file = filename;			
			switch(UIDevice.CurrentDevice.UserInterfaceIdiom)
			{
				case(UIUserInterfaceIdiom.Pad):	
					switch(UIApplication.SharedApplication.StatusBarOrientation)
					{
						case UIInterfaceOrientation.Portrait:	
						case UIInterfaceOrientation.PortraitUpsideDown:
							file += "-Portrait";
						    break;
						case UIInterfaceOrientation.LandscapeLeft:
						case UIInterfaceOrientation.LandscapeRight:
							file += "-Landscape";
							break;
					}				
					break;					
				default:
					break;
			}		
			
			path = path.Replace(filename, file);
			
			var image = UIImage.FromBundle(path);
			
			// If we're an iPhone and in landscape mode, just flip it on its ear
			if(Util.IsPhone() && isLandscape)
			{
				var cgImage = image.CGImage;
				var rotated = new UIImage(cgImage, image.CurrentScale, UIImageOrientation.Right);
				cgImage.Dispose();
				image.Dispose();
				return rotated;
			}
			
			return image;
		}
		
		public static UIImage FromIdiomBundle(string path)
		{
			string filename = Path.GetFileNameWithoutExtension(path);
			string file = filename;			
			switch(UIDevice.CurrentDevice.UserInterfaceIdiom)
			{
				case(UIUserInterfaceIdiom.Pad):	
					file += "~ipad";				
					switch(UIDevice.CurrentDevice.Orientation)
					{
						case UIDeviceOrientation.Portrait:	
						case UIDeviceOrientation.PortraitUpsideDown:
						    break;
						case UIDeviceOrientation.LandscapeLeft:
						case UIDeviceOrientation.LandscapeRight:
							break;
					}				
					break;
				default:
					break;
			}					
			path = path.Replace(filename, file);			
			return UIImage.FromBundle(path);
		}
	}
}