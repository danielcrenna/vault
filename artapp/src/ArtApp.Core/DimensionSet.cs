using System;
using MonoTouch.UIKit;
using System.Diagnostics;
using System.Drawing;

namespace ArtApp
{
	public class DimensionSet
	{
		private static UIUserInterfaceIdiom _idiom;
		public static UIUserInterfaceIdiom Idiom
		{
			get
			{
				return _idiom;	
			}				
		}
				
		static DimensionSet()
		{
			_idiom = UIDevice.CurrentDevice.UserInterfaceIdiom;
		}
		
		public static bool IsRetinaDisplay
		{
			get
			{
				if(!UIDevice.CurrentDevice.CheckSystemVersion(4, 0)) // Protect iOS 3
				{
					return false;
				}				
				return Idiom == UIUserInterfaceIdiom.Phone && UIScreen.MainScreen.Scale == 2.0;	
			}
		}
		
		public static float ListThumbnailSquare
		{
			get
			{
				return UIScreen.MainScreen.Bounds.Width / 10;	
			}
		}
		
		public static float ScreenWidth
		{
			get
			{
				return UIScreen.MainScreen.Bounds.Width;	
			}
		}
		
		public static float ScreenHeight
		{
			get
			{
				return UIScreen.MainScreen.Bounds.Height;
			}
		}
		
		public static float GalleryScreenWidth
		{
			get
			{
				return UIScreen.MainScreen.Bounds.Width;	
			}
		}
				
		public static float StatusBarHeight
		{
			get
			{
				return 20;	
			}
		}
		
		public static float NavigationBarHeight
		{
			get
			{
				return 44;	
			}
		}
		
		public static float TabBarHeight
		{
			get	
			{
				return 49;	
			}
		}

		public static int BioImageSquareSize
		{
			get
			{
				switch(_idiom)
				{
					case UIUserInterfaceIdiom.Pad:
						return 146;
					default:
						return 73;
				}					
			}
		}
	}
}

