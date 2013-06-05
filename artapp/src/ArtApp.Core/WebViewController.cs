//
// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//
using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.Dialog;

namespace ArtApp
{
	public class WebViewController : UIViewController
	{
		private static WebViewController Main = new WebViewController();
		
		private UIToolbar topBar;
		private UIToolbar toolbar;
		private UIBarButtonItem backButton, forwardButton, stopButton, refreshButton;
		private UILabel title;
		protected UIWebView WebView;

		protected WebViewController()
		{
			var fixedSpace = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace, null) {
				Width = 26
			};
			var flexibleSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null);

			toolbar = new UIToolbar ();
			toolbar.BarStyle = UIBarStyle.Black;
			
			topBar = new UIToolbar ();
			topBar.BarStyle = UIBarStyle.Black;			
			topBar.SetBackgroundImage(UIImage.FromBundle ("Images/opaqueBar.png"), UIToolbarPosition.Top, UIBarMetrics.Default);
			
			title = new UILabel(new RectangleF(10, 0, 80, 30))
			{
				BackgroundColor = UIColor.Clear,
				AdjustsFontSizeToFitWidth = true,
				Font = UIFont.BoldSystemFontOfSize(22),
				MinimumFontSize = 14,
				TextColor = UIColor.White,
				ShadowColor = UIColor.FromRGB(64, 74, 87),
				ShadowOffset = new SizeF (0, -1)
			};
			
			topBar.Items = new UIBarButtonItem []
			{
				new UIBarButtonItem (title),
				flexibleSpace,
				new UIBarButtonItem ("Close", UIBarButtonItemStyle.Bordered, (o, e) =>
				{ 
					Main.DismissModalViewControllerAnimated (true);
				})
			};
			
			backButton = new UIBarButtonItem (UIImage.FromBundle("Images/39-back.png"), UIBarButtonItemStyle.Plain, (o, e) => { WebView.GoBack (); });
			forwardButton = new UIBarButtonItem (UIImage.FromBundle("Images/40-forward.png"), UIBarButtonItemStyle.Plain, (o, e) => { WebView.GoForward (); });
			refreshButton = new UIBarButtonItem (UIBarButtonSystemItem.Refresh, (o, e) => { WebView.Reload (); });
			stopButton = new UIBarButtonItem (UIBarButtonSystemItem.Stop, (o, e) => { WebView.StopLoading (); });
			
			toolbar.Items = new UIBarButtonItem [] { backButton, fixedSpace, forwardButton, flexibleSpace, stopButton, fixedSpace, refreshButton };

			View.AddSubview (topBar);
			View.AddSubview (toolbar);
		}

		void UpdateNavButtons()
		{
			if (WebView == null)
			{
				return;
			}
			backButton.Enabled = WebView.CanGoBack;
			forwardButton.Enabled = WebView.CanGoForward;
		}
		
		protected virtual string UpdateTitle()
		{
			return WebView.EvaluateJavascript("document.title");			
		}
		
		public void SetupWeb (string initialTitle, bool enableTitle)
		{
			WebView = new UIWebView()
			{
				ScalesPageToFit = true,
				MultipleTouchEnabled = true,
				AutoresizingMask = UIViewAutoresizing.FlexibleHeight|UIViewAutoresizing.FlexibleWidth,
			};
			WebView.LoadStarted += delegate
			{ 
				stopButton.Enabled = true;
				refreshButton.Enabled = false;
				UpdateNavButtons ();				
				Util.PushNetworkActive(); 
			};
			WebView.LoadFinished += delegate 
			{
				stopButton.Enabled = false;
				refreshButton.Enabled = true;
				Util.PopNetworkActive(); 
				UpdateNavButtons();				
				if(enableTitle) title.Text = UpdateTitle();
			};
			
			if(enableTitle)
			{
				title.Text = initialTitle;
			}
			else
			{
				title.Text = "";	
			}
			View.AddSubview (WebView);
			backButton.Enabled = false;
			forwardButton.Enabled = false;
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear(animated);
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear(animated);
			
			if (WebView != null)
			{
				WebView.RemoveFromSuperview();
				WebView.Dispose();
				WebView = null;
			}
		}

		private void LayoutViews()
		{
			var sbounds = View.Bounds;
			int top = (InterfaceOrientation == UIInterfaceOrientation.Portrait) ? 0 : -44;
			
			topBar.Frame = new RectangleF (0, top, sbounds.Width, 44);
			toolbar.Frame =  new RectangleF (0, sbounds.Height - 44, sbounds.Width, 44);
			WebView.Frame = new RectangleF (0, top + 44, sbounds.Width, sbounds.Height - 88 -top);			
			title.Frame = new RectangleF (10, 0, sbounds.Width - 80, 38);
		}
		
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			LayoutViews();
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			LayoutViews();
		}
		
		public static void OpenUrl(DialogViewController parent, string url)
		{
			OpenUrl (parent, url, true);
		}
		
		public static void OpenUrl(DialogViewController parent, string url, bool enableTitle)
		{
			UIView.BeginAnimations("OpenUrl");
			Main.HidesBottomBarWhenPushed = true;
			Main.SetupWeb(url, enableTitle);			
			if (url.StartsWith("http://"))
			{
				string host;
				int last = url.IndexOf ('/', 7);
				if (last == -1)
				{
					host = url.Substring (7);
				}
				else 
				{
					host = url.Substring (7, last - 7);
				}
				url = "http://" + host + (last == -1 ? "" : url.Substring (last));
			}
			Main.WebView.LoadRequest(new NSUrlRequest(new NSUrl(url)));
			parent.PresentModalViewController(Main, true);
			UIView.CommitAnimations();
		}
	}
}

