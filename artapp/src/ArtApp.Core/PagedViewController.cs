using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MonoTouch.CoreGraphics;
 
namespace ArtApp
{
	// Original: https://github.com/escoz/monotouch-controls/blob/master/UICatalog/PagedViewController.cs
			
	public class PagedViewController : UIViewController
	{
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				Clear();
				_scrollView.Dispose();
				_scrollView = null;
			}
			base.Dispose(disposing);
		}
		
		// The size of the scrollable area; should cover the bottom as the tab bar is not present
		private static readonly float GalleryScreenHeight = UIScreen.MainScreen.Bounds.Height - DimensionSet.StatusBarHeight - DimensionSet.NavigationBarHeight + DimensionSet.TabBarHeight;
		
		public SeriesDataSource PagedViewDataSource { get; set; }
 
		public event EventHandler<PagedViewEventArgs> PageChanged;
		public void OnPageChanged()
		{
			if(PageChanged != null)
			{
				PageChanged(this, new PagedViewEventArgs(Page));
			}
		}
		
		private PagedScrollView _scrollView;
		
		private readonly IList<PieceViewController> _pages = new List<PieceViewController>();
		public IList<PieceViewController> Pages
		{
			get
			{
				return _pages;
			}
		}
		
		private readonly UIPageControl _pageControl = new UIPageControl
		{
			Pages = 0,
			Frame = new RectangleF(0, 2000, 0, 0) // Should be invisible on all devices / rotations
        };
           
		public PagedViewController()
		{
			_scrollView = new PagedScrollView();
			_scrollView.DecelerationEnded += HandleScrollViewDecelerationEnded;
			_pageControl.ValueChanged += HandlePageControlValueChanged;
		}
           
		private int _page;
		public int Page
		{
			get { return _page; }
			set
			{
				var distance =  Math.Abs(value - _page);
				var unit = 1.75f / _pages.Count;
				var speed = unit * distance;
				
				_pageControl.CurrentPage = value;
				_page = value;
				
				UIView.Animate(speed, 0.75f /* same as flip speed */, UIViewAnimationOptions.CurveEaseInOut,
				()=>
				{
					CalculateScrollLocationByPage(value);
				},
				()=>
				{
					if(_pages.Count > 0)
					{
						_pages[value].ViewDidAppear(true);
					}			
				});
			}
		}
		
		private void CalculateScrollLocationByPage(int page)
		{
			PointF point;
			SizeF size;
			
			if(Util.IsPortrait())
			{
				point = new PointF((page * DimensionSet.ScreenWidth), 0);
				size = new SizeF(DimensionSet.GalleryScreenWidth, GalleryScreenHeight);
			}
			else
			{
				point = new PointF((page * UIScreen.MainScreen.Bounds.Height), 0);
				size = new SizeF(UIScreen.MainScreen.Bounds.Height, DimensionSet.GalleryScreenWidth);
			}	
								
			_scrollView.ScrollRectToVisible(new RectangleF(point, size), false);
		}
 
		public void HandleScrollViewDecelerationEnded(object sender, EventArgs e)
		{
			int page = (int)Math.Floor((_scrollView.ContentOffset.X - _scrollView.Frame.Width / 2) / _scrollView.Frame.Width) + 1;
			_page = page;
			_pageControl.CurrentPage = page;
			_pages[page].ViewDidAppear (true);
			OnPageChanged();
		}
           
		void HandlePageControlValueChanged (object sender, EventArgs e)
		{
			Page = _pageControl.CurrentPage;
		}
           
		private int _count;
		public void ReloadPages(int startAt)
		{
			Clear();
			
			_count = BuildPageViewControllers();
			            
			SetContentSizeCoveringPages(); 
			
			_pageControl.Pages = _count;
			_pageControl.CurrentPage = startAt;                 
			_pages[startAt].ViewDidAppear(true);
			_page = startAt;
			
			if(startAt > 0)
			{
				CalculateScrollLocationByPage(startAt);
			}
		}

		public void SetContentSizeCoveringPages()
		{
			SizeF contentSize;
			if(Util.IsPortrait())
			{
				contentSize = new SizeF(DimensionSet.ScreenWidth * (_count == 0 ? 1 : _count), GalleryScreenHeight);	
			}
			else
			{
				contentSize = new SizeF(UIScreen.MainScreen.Bounds.Height * (_count == 0 ? 1 : _count), DimensionSet.ScreenWidth);
			}			
			_scrollView.ContentSize = contentSize;
		}

		public int BuildPageViewControllers()
		{
			int i;
			var numberOfPages = PagedViewDataSource.Pages;
			for (i = 0; i< numberOfPages; i++)
			{
				var pvc = PagedViewDataSource.GetPage(i);
				SetPageFrame(pvc, i);
				_scrollView.AddSubview(pvc.View);
				_pages.Add(pvc);
			}
						
			return i;
		}

		private void SetPageFrame(PieceViewController pvc, int i)
		{
			RectangleF frame;
			if(Util.IsPortrait())
			{
				frame = new RectangleF(
					DimensionSet.ScreenWidth * i, 0, 
					DimensionSet.ScreenWidth, GalleryScreenHeight
				);
			}
			else
			{
				frame = new RectangleF(
					UIScreen.MainScreen.Bounds.Height * i, 0, 
					UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width
					);
			}				
			pvc.View.Frame = frame;
		}
		
		public override void ViewDidLoad()
		{
			SetFrame();
						
			View.BackgroundColor = UIColor.Black;
			
			SetBackgroundView();
			
			View.AutosizesSubviews = false;
			View.AddSubview(_backgroundView);		
			View.AddSubview(_pageControl);
			View.AddSubview(_scrollView);			
			View.ClipsToBounds = false;
		}

		public void SetFrame()
		{
			float w;
			float h;
			
			if(Util.IsPortrait())
			{
				w = DimensionSet.ScreenWidth;
				h = GalleryScreenHeight;
			}
			else
			{
				h = UIScreen.MainScreen.Bounds.Width - DimensionSet.StatusBarHeight;
				w = UIScreen.MainScreen.Bounds.Height;
			}
			
			View.Frame = new RectangleF(0, 0, w, h);
		}
		
		private static UIImageView _backgroundView;
		private void SetBackgroundView()
		{
			var image = UIImageEx.FromIdiomBundleForBackground("Images/backgrounds/leather.jpg");
			_backgroundView = _backgroundView ?? new UIImageView();
			_backgroundView.Image = image;
			_backgroundView.Frame = View.Frame;
			_backgroundView.UserInteractionEnabled = true;
			image.Dispose();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			AppGlobal.PiecesInView = _pages;
		}
		
		public void Clear()
		{
			foreach (var p in _pages)
			{
				// http://stackoverflow.com/questions/8754124/uiimageview-uiimage-memory-tag-70-release-timing-when-scrolling
				p.View.RemoveFromSuperview();
				p.Dispose();
			}
			_pages.Clear();	
		}
 
		private sealed class PagedScrollView : UIScrollView
		{
			public PagedScrollView()
			{				
				ClipsToBounds = false;
				ShowsHorizontalScrollIndicator = false;
				ShowsVerticalScrollIndicator = false;
				Bounces = true;
				PagingEnabled = true;				
				SetScrollArea();
			}

			public void SetScrollArea()
			{
				SizeF contentSize;
				RectangleF frame;
				
				if(Util.IsPortrait())
				{
					contentSize = new SizeF(DimensionSet.ScreenWidth, GalleryScreenHeight);
					frame = new RectangleF(0, 0, DimensionSet.ScreenWidth, GalleryScreenHeight);
				}
				else
				{
					contentSize = new SizeF(UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width);
					frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width);
				}
				
				ContentSize = contentSize;
				Frame = frame;
			}
		}
	}
}