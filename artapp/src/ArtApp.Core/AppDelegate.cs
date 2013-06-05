using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using MonoTouch.CoreGraphics;
using MonoTouch.ObjCRuntime;

namespace ArtApp
{
	[Preserve]
	public partial class AppDelegate : UIApplicationDelegate
	{
		private static UINavigationController _navigationController;
		public static UINavigationController NavigationController
		{
			get
			{
				return _navigationController;
			}
		}
		
		public static CustomNavigationBar NavigationBar
		{
			get
			{
				return NavigationController.NavigationBar as CustomNavigationBar;	
			}
		}
		
		private static UIWindow _window;
		public static UIWindow ApplicationWindow
		{
			get
			{
				return _window;	
			}
		}
		
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			CheckBundleVersion();
			
			// Required to set background color behind flip transition
			// http://stackoverflow.com/questions/4252350/change-background-color-of-uimodaltransitionstylefliphorizontal
			_window = window;
			_window.BackgroundColor = UIColor.Black;
			_navigationController = navigationController;
			_navigationController.View.BackgroundColor = UIColor.Black;
			
			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackOpaque;
			
			window.RootViewController = navigationController;
			window.MakeKeyAndVisible();			

			return true;
		}
		
		private static void CheckBundleVersion()
		{
			// Drop a version nugget and clear the disk cache if one didn't previously exist, or it's not the same
			var version = AppGlobal.BundleVersion;

			string appdir = NSBundle.MainBundle.BundleUrl.Path;
    		string cache = Path.GetFullPath (Path.Combine (appdir, "..", "Library", "Caches"));
			
			var versionPath = Path.Combine(cache, "v.txt");
			var dumpDiskCache = false;
			
			if(!File.Exists(versionPath))
			{
				dumpDiskCache = true;
				File.WriteAllText(versionPath, version);
			}
			else
			{
				var versionOnDisk = File.ReadAllText(versionPath);
				if(!version.Equals(versionOnDisk))
				{
					dumpDiskCache = true;
					File.WriteAllText(versionPath, version);
				}
			}
			
			if(dumpDiskCache)
			{
				ImageCache.Clear();
				ImageCache.DumpDiskCache();	
			}
		}
		
		// http://stackoverflow.com/questions/9368498/monotouch-items-saved-to-library-caches-never-staying-around
		private void DisplayCachesContent()
		{
			string appdir = NSBundle.MainBundle.BundleUrl.Path;
		    string cache = Path.GetFullPath (Path.Combine (appdir, "..", "Library", "Caches"));
		    Console.WriteLine ("Files:");
		    int i = 0;
		    foreach (string file in Directory.GetFiles(cache, "*.*", SearchOption.AllDirectories))
			{
				Console.WriteLine ("{0}. {1}", i++, file);
			}
		}

		public override void OnActivated(UIApplication application)
        {
			// Required for iOS 3.0
        }
		
		public override void ReceiveMemoryWarning (UIApplication application)
		{
			ImageCache.Clear();
		}
	}
}