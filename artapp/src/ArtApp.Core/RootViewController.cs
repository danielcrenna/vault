using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreGraphics;

namespace ArtApp
{
	[Register("RootViewController")]
	public class RootViewController : TabBarController
	{
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}

		public RootViewController (IntPtr handle) : base(handle)
		{
			
		}
		
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			
			InitializeViewControllersOnce();
		}
		
		private void InitializeViewControllersOnce()
		{
			var controllers = new List<UIViewController>();
			
			var _cvc = new CollectionsViewController();
			_cvc.Title = "Collections";
			_cvc.TabBarItem = new UITabBarItem("Collections", UIImage.FromBundle("Images/41-picture-frame.png"), 2);
			controllers.Add(_cvc);				

			var bvc = new BioViewController();
			bvc.Title = "Bio";
			bvc.TabBarItem = new UITabBarItem("Bio", UIImage.FromBundle("Images/111-user.png"), 4);
			controllers.Add(bvc);
					
			SetViewControllers(controllers.ToArray(), false);
			controllers.Clear();
		}
	}		
}

