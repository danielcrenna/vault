using System;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.ObjCRuntime;

namespace ArtApp
{
	[Register("TabBarController")]
	public class TabBarController : UITabBarController
	{
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		public TabBarController(IntPtr handle) : base(handle)
		{
			// Don't put anything in this!
		}
		
		public TabBarController() : base()
		{
			CreateTabViewForBackground();
			this.TabBar.InsertSubview(_tabView, 0);
		}
		
		// Create a view controller and setup it's tab bar item with a title and image
		public UIViewController ViewControllerWithTabTitle(string title, UIImage image)
		{
			UIViewController viewController = new UIViewController();
			viewController.TabBarItem = new UITabBarItem(title, image, 0);
			return viewController;
		}		
		
		private UIImageView _tabBarArrow;
				
		private UIView _tabView;
		public UIColor BackgroundColor {
			get { return _tabView.BackgroundColor; }
			set { _tabView.BackgroundColor = value; }
		}
		
		private UIViewController _rootViewController;
		public UIViewController RootViewController
		{
			get { return _rootViewController; }
			set { _rootViewController = value; }
		}
				
		private void CreateTabViewForBackground()
		{
			_tabView = _tabView ?? new UIView();
			
			switch(InterfaceOrientation)
			{
				case UIInterfaceOrientation.LandscapeLeft:
				case UIInterfaceOrientation.LandscapeRight:
					_tabView.Frame = new RectangleF (0.0f, 0.0f, this.View.Bounds.Size.Height, 46);
					break;
				case UIInterfaceOrientation.Portrait:
				case UIInterfaceOrientation.PortraitUpsideDown:
					_tabView.Frame = new RectangleF (0.0f, 0.0f, this.View.Bounds.Size.Width, 46);
					break;
			}
			
			_tabView.Alpha = 0.5f;
		}
		
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			
			this.AddTabBarArrow(this.SelectedIndex);			
		}
		
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();			
		}
		
		private bool _initialized = false;
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			
			if(!_initialized)
			{
				this.ViewControllerSelected += HandleViewControllerSelected;
				this.AddTabBarArrow(0);			
				SelectedIndex = 0;
				_initialized = true;
			}
			else
			{
				this.AddTabBarArrow(this.SelectedIndex);
			}
		}

		private void HandleViewControllerSelected(object sender, UITabBarSelectionEventArgs e)
		{
			MoveArrowToSelectedTab();  
		}

		public void MoveArrowToSelectedTab()
		{
			UIView.BeginAnimations(null);
			UIView.SetAnimationDuration(0.2f);			
			
			var x = this.HorizontalLocationFor(this.SelectedIndex);
			var frame = new RectangleF(
				x, 
				_tabBarArrow.Frame.Y, 
				_tabBarArrow.Frame.Width, 
				_tabBarArrow.Frame.Height
			);
			
			_tabBarArrow.Frame = frame;			
			UIView.CommitAnimations();
		}
		
		private void AddTabBarArrow(int index)
		{
			if(this._tabBarArrow != null)
			{
				_tabBarArrow.RemoveFromSuperview();
				_tabBarArrow.Dispose();
			}
			
			UIImage tabBarArrowImage = UIImage.FromBundle("Images/tabArrow.png");
		    this._tabBarArrow = new UIImageView(tabBarArrowImage);
					  
			// To get the vertical location we start at the bottom of the window, go up by height of the tab bar, 
			// go up again by the height of arrow and then come back down 2 pixels so the arrow is slightly on top 
			// of the tab bar.

		    var verticalLocation = 
				(Util.IsLandscape() ? AppDelegate.ApplicationWindow.Frame.Size.Width : AppDelegate.ApplicationWindow.Frame.Size.Height) - 
				this.TabBar.Frame.Size.Height - 
				tabBarArrowImage.Size.Height + 2;
		    
			_tabBarArrow.Frame = new RectangleF(
				this.HorizontalLocationFor(index), 
				verticalLocation, 
				tabBarArrowImage.Size.Width, 
				tabBarArrowImage.Size.Height
				);
		
			this.View.AddSubview(_tabBarArrow);
			tabBarArrowImage.Dispose();
		}
				
		// http://stackoverflow.com/questions/7228570/changing-width-of-uitabbaritems-and-margins-between-them-in-ipad-app
		public int HorizontalLocationFor(int tabIndex)
		{
			if(UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
			{
				switch(tabIndex)
				{
					case 0:
						return 159;
					case 1:
						return 269;
					case 2:
						return 379;
					case 3:
						return 489;
					case 4:
						return 599;
					default:
						throw new ArgumentException("no such tab");
				}
			}
			
			var tabItemWidth = (this.TabBar.Frame.Size.Width / this.TabBar.Items.Length);
			
			// A half width is tabItemWidth divided by 2 minus half the width of the arrow
  			var halfTabItemWidth = (int)(tabItemWidth / 2) - (_tabBarArrow.Frame.Size.Width / 2);
  
			// The horizontal location is the index times the width plus a half width
			return (int)((tabIndex * tabItemWidth) + halfTabItemWidth);
		}
	}
}
