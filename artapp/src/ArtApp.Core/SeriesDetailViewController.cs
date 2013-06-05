using System;
using System.Net;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;

namespace ArtApp
{
	public class SeriesDetailViewController : CustomDialogViewController
	{
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_detailView != null)
				{
					_detailView.Dispose();
					_detailView = null;
				}
				if(_triangleView != null)
				{
					_triangleView.Dispose();
					_triangleView = null;	
				}
				if(_containerView != null)
				{
					_containerView.Dispose();
					_containerView = null;
				}
			}
			base.Dispose (disposing);
		}
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		public const int PadX = 4;
		
		private Series _series;
		private SeriesViewController _parent;
		private UIView _containerView;
		private SeriesDetailView _detailView;
		private TriangleView _triangleView;
		
		public SeriesDetailViewController(Series series, SeriesViewController parent) : base("Images/backgrounds/wall.jpg", null, true)
		{
			_series = series;
			_parent = parent;
			
			Root = GetRoot();
		}
		
		public SeriesDetailViewController(IntPtr handle) : base(handle)
		{
			
		}
		
		private RootElement GetRoot()
		{
			var rect = new RectangleF(PadX, 0, View.Bounds.Width - 30 - PadX * 2, 100);
			_detailView = new SeriesDetailView(_series, rect);
			
			_triangleView = new TriangleView (UIColor.FromRGB(247, 247, 247), UIColor.FromRGB (171, 171, 171)) {
				Frame = new RectangleF (43, _detailView.Bounds.Height - 7, 16, 8)
			};
			
			_containerView = new UIView(rect);
			_containerView.Add(_detailView);
			_containerView.Add(_triangleView);
			
			var text = new StyledMultilineElement(_series.Description);
			text.TextColor = UIColor.DarkGray;
			text.LineBreakMode = UILineBreakMode.WordWrap;
			text.Font = UIFont.ItalicSystemFontOfSize(14);
			text.DetailColor = text.TextColor;
						
			var main = new Section(_containerView)
			{
				text
			};
			
			var root = new RootElement("")
			{
				main
		 	};
			
			return root;
		}
		
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			this.TableView.ContentInset = new UIEdgeInsets(0, 0, 0, 0); // Required to show content at the Y offset		
			AppDelegate.NavigationBar.SetBackButtonOn(this);
		}
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			_parent.PendingRotate = fromInterfaceOrientation;	
		}
		
		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			var customNavigationBar = AppDelegate.NavigationBar;
			customNavigationBar.SetTransparent();
		}		
	}
}

