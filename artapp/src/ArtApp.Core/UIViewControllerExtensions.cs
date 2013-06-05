using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace ArtApp
{
	public static class UIViewControllerExtensions
	{
		// http://blog.touch4apps.com/home/iphone-monotouch-development/monotouch-performselector-helper	
		private delegate void Selector();
		private static void PerformSelector(Selector selector, float delay)
		{
			NSTimer.CreateScheduledTimer(TimeSpan.FromSeconds(delay), delegate { selector (); });
		}
		
		public static void PresentSemiModalViewController(this UIViewController instance, SemiModalViewController vc)
		{	
			AppGlobal.DisableRotations();
			
			UIView modalView = vc.View;
			UIView coverView = vc.CoverView;
			
			var middleCenter = instance.View.Center;
			var size = UIScreen.MainScreen.Bounds.Size;
			var center = PointF.Empty;
			
			if(Util.IsLandscape())
			{
				center = new PointF(size.Height / 2.0f, size.Width * 1.2f);
				middleCenter = new PointF(middleCenter.Y, middleCenter.X);
				modalView.Bounds = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width - DimensionSet.StatusBarHeight);
				coverView.Frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width);
			}
			else
			{
				center = new PointF(size.Width / 2.0f, size.Height * 1.2f);
				modalView.Bounds = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - DimensionSet.StatusBarHeight);
				coverView.Frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
			}
						
			// we start off-screen
			modalView.Center = center;
			coverView.Alpha = 0.0f;
			
			instance.View.AddSubview(coverView);
			instance.View.AddSubview(modalView);
			
			// Show it with a transition effect
			UIView.BeginAnimations("semi_modal_present");
			UIView.SetAnimationDuration(0.6);			
			modalView.Center = middleCenter;
			coverView.Alpha = 0.5f;			
			UIView.CommitAnimations();			
		}
		
		public static void DismissSemiModalViewController(this UIViewController instance, SemiModalViewController vc)
		{
			AppGlobal.EnableRotations();		
						
			var animationDelay = 0.7f;
			UIView modalView = vc.View;
			UIView coverView = vc.CoverView;
			
			var offSize = UIScreen.MainScreen.Bounds.Size;
			var offScreenCenter = PointF.Empty;
			
			UIDeviceOrientation orientation = UIDevice.CurrentDevice.Orientation;
			if(orientation == UIDeviceOrientation.LandscapeLeft || orientation == UIDeviceOrientation.LandscapeRight)
			{
				offScreenCenter = new PointF(offSize.Height / 2.0f, offSize.Width * 1.5f);
			}
			else
			{
				offScreenCenter = new PointF(offSize.Width / 2.0f, offSize.Height * 1.5f);
			}
			
			UIView.Animate(animationDelay,
			delegate
			{
                modalView.Center = offScreenCenter;
				coverView.Alpha = 0.0f;
            },
			delegate
			{
                if(modalView != null && modalView.Superview != null)
				{ 
					modalView.RemoveFromSuperview();
				}
				if(coverView != null && coverView.Superview != null)
				{ 
					coverView.RemoveFromSuperview();
				}
            });
		}
	}
}