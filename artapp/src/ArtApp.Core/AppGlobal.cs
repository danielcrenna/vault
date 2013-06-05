using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.IO;

namespace ArtApp
{
	public class AppGlobal
	{
		public static bool RotationPending { get;set; }
		public static TabBarController TabBarController { get; set; }		
		public static bool CollectionsViewInListMode { get; set; }
		public static IList<PieceViewController> PiecesInView { get; set; }

		public static string BundleVersion
		{
			get
			{
				return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
			}
		}

		// App was originally rejected because we were storing processed images in the app's bundle directory!
		private static string _cachePath;
		public static string LibraryCachesPath
		{
			get
			{
				if(_cachePath == null)
				{
				    _cachePath = Path.GetFullPath(Path.Combine(NSBundle.MainBundle.BundleUrl.Path, "..", "Library", "Caches"));
					if(!Directory.Exists(_cachePath))
					{
						Directory.CreateDirectory(_cachePath);
					}
				}
				return _cachePath;
			}
		}
		
		public static void DisableRotations()
		{
			_disableRotations = true;
			_lastOrientation = UIApplication.SharedApplication.StatusBarOrientation;
		}
		
		public static void EnableRotations()
		{
			_disableRotations = false;
		}
		
		private static bool _disableRotations;
		private static UIInterfaceOrientation _lastOrientation;
		public static bool MayRotate(UIInterfaceOrientation requested)
		{
			if(RotationPending)
			{
				return false;	
			}
			if(_disableRotations)
			{
				return requested == _lastOrientation;
			}
			return true;
		}
	}
}

