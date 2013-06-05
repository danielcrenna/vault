using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ArtApp
{
	public class SemiModalViewController : UIViewController 
	{
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(CoverView != null)
				{
					CoverView.Dispose();
					CoverView = null;
				}
			}
			base.Dispose (disposing);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		public UIView CoverView { get; private set; }
		
		public SemiModalViewController()
		{
			CoverView = new UIView(UIScreen.MainScreen.ApplicationFrame);
			CoverView.BackgroundColor = UIColor.Black;
		}
		
		public SemiModalViewController(IntPtr handle) : base(handle)
		{
			
		}
		
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			
			View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			CoverView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
		}
		
		public override void ViewDidUnload()
		{
			base.ViewDidUnload();
			CoverView = null;
		}
	}	
}