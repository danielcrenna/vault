using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace ArtApp
{
	[Register("SeriesViewController")]
	public class SeriesViewController : UIViewController
	{
		private SizeF _listViewImageSize = new SizeF(29, 30);
		
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_galleryView != null)
				{
					_galleryView.Dispose();
					_galleryView = null;
				}
				if(_listView != null)
				{
					_listView.Dispose();
					_listView = null;
				}
				if(_buttonGalleryThumb != null)
				{
					_buttonGalleryThumb.Image.Dispose();
					_buttonGalleryThumb.Image = null;
					_buttonGalleryThumb.Dispose();
					_buttonGalleryThumb = null;
				}
				if(_buttonListThumb != null)
				{
					_buttonListThumb.Image.Dispose();
					_buttonListThumb.Image = null;
					_buttonListThumb.Dispose();
					_buttonListThumb = null;
				}
				if(_sdvc != null)
				{
					_sdvc.Dispose();
					_sdvc = null;
				}
			}
			
			base.Dispose(disposing);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		public const double FlipDuration = 0.75f;
		
		private UIBarButtonItem _listButton;
		private UIBarButtonItem _infoButton;
		private UIButton _buttonInner;
		private UIImageView _buttonGalleryThumb;
		private UIImageView _buttonListThumb;
		private RectangleF _buttonFrame;
		private SeriesGalleryViewController _galleryView;
		private SeriesListViewController _listView;
		private Series _series;
		private bool _flipping;
		
		public void GoToPiece(Series series, Piece piece)
		{
			int page = 0;
			for(var i = 0; i < series.Pieces.Count; i++)
			{
				if(series.Pieces[i] == piece)
				{
					page = i;	
				}
			}
			_listView.GoToPage(page, false /* flip */);	
		}
		
		public bool Flipping
		{
			get
			{
				return _flipping;		
			}
		}
		
		public Series Series
		{
			get
			{
				return _series;
			}
		}
		
		private int _page;
		public int Page
		{
			get
			{
				return _page;	
			}
			set
			{
				var previous = _page;
				_page = value;
				
				var isLandscape = Util.IsLandscape();
				if(_galleryView.IsLandscape && !isLandscape || !_galleryView.IsLandscape && isLandscape)
				{
					_galleryView.QueuedPage = _page;
				}
				else
				{
					_galleryView.SetPage(_page);	
				}
				
				if(previous != _page)
				{
					SetFlipPieceThumbnail();			
				}
			}
		}

		private void SetFlipPieceThumbnail()
		{
			var z = ImageCache.Get("flip_thumbnail_" + _series.Title + "_" + _page, ()=>
			{
				var thumb0 = ImageFactory.LoadRoundedThumbnail(_series.Pieces[_page]);
				var thumb1 = ImageHelper.ImageToFitSize(thumb0, _listViewImageSize);
				thumb0.Dispose();
				return thumb1;
			});		
			_buttonGalleryThumb.Image.Dispose();
			_buttonGalleryThumb.Image = null;
			_buttonGalleryThumb.Image = z;
		}
		
		public SeriesViewController(Series series)
		{
			_series = series;
		}
		
		public SeriesViewController(IntPtr handle) : base(handle)
		{
			
		}
		
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);
			
			if(!AppGlobal.CollectionsViewInListMode)
			{
				ShowListViewButton();
				ShowGalleryView();
			}
			else
			{
				ShowPieceButton();
				ShowListView();
			}
			
			if(!AppGlobal.CollectionsViewInListMode)
			{
				AppDelegate.NavigationBar.SetTransparent();
			}
			else
			{
				AppDelegate.NavigationBar.SetOpaque();
			}
		}
		
		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			
			AppDelegate.NavigationBar.SetOpaque();
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear(animated);
			
			// Hopefully hinting that we want to dispose this right away...
			this.View.RemoveFromSuperview();
		}
				
		public override void ViewDidLoad()
        {
			base.ViewDidLoad();
			
			AppDelegate.NavigationBar.SetBackButtonOn(this);
			
			// Initialize the alternate list selector
			_listView = new SeriesListViewController(this);
			_listView.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
			_listView.View.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);
						
			// Initialize the art gallery (already in background)
			_galleryView = new SeriesGalleryViewController(this, _listView);
			_galleryView.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
			_galleryView.View.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);		
						
			// http://stackoverflow.com/questions/1718495/why-does-viewdidappear-not-get-triggered
			this.View.AddSubview(AppGlobal.CollectionsViewInListMode ? _listView.View : _galleryView.View);
									
			// http://www.grokkingcocoa.com/a-simple-way-to-animate-a-u.html
			// http://ykyuen.wordpress.com/2010/06/11/iphone-adding-image-to-uibarbuttonitem/
			var listViewImage = UIImage.FromBundle("Images/gallery/listView.png");
			var listViewImageHighlighted = UIImage.FromBundle("Images/gallery/listViewHighlighted.png");
						
			_buttonFrame = new RectangleF(0, 0, 29, 30);			
			_buttonListThumb = new UIImageView(listViewImage);
			listViewImage.Dispose();
			_buttonListThumb.Frame = _buttonFrame;
			_buttonListThumb.Hidden = AppGlobal.CollectionsViewInListMode;
			
			var buttonListHighlight = new UIImageView(listViewImageHighlighted);
			listViewImageHighlighted.Dispose();
			buttonListHighlight.Frame = _buttonFrame;			
			
			// Build a thumbnail button for the selected page's piece
			// TODO duplicate with code to select a piece in the list view
			// TODO shouldn't have to size something from a factory method!
			var thumb0 = ImageFactory.LoadRoundedThumbnail(_series.Pieces[_page]);
			var thumb1 = ImageHelper.ImageToFitSize(thumb0, _listViewImageSize);
			thumb0.Dispose();
			_buttonGalleryThumb = new UIImageView(thumb1);
			thumb1.Dispose();
			
			_buttonGalleryThumb.Hidden = !AppGlobal.CollectionsViewInListMode;
			_buttonGalleryThumb.Frame = _buttonFrame;
			
			// Wire up the flip button
			_buttonInner = UIButton.FromType(UIButtonType.Custom);
			_buttonInner.UserInteractionEnabled = true;
			_buttonInner.Bounds = _buttonListThumb.Bounds;
			_buttonInner.AddSubview(_buttonListThumb);			
			_buttonInner.AddSubview(_buttonGalleryThumb);
			_buttonInner.AddTarget(delegate { 
				_buttonInner.InsertSubviewAbove(_buttonListThumb, buttonListHighlight);
				buttonListHighlight.RemoveFromSuperview();
				Flip();
			}, UIControlEvent.TouchUpInside);
			_buttonInner.AddTarget(delegate { 
				_buttonInner.InsertSubviewAbove(_buttonListThumb, buttonListHighlight);
				buttonListHighlight.RemoveFromSuperview();
				Flip();
			}, UIControlEvent.TouchUpOutside);
			_buttonInner.AddTarget(delegate { 
				_buttonInner.InsertSubviewAbove(buttonListHighlight, _buttonListThumb);
				_buttonListThumb.RemoveFromSuperview();
			}, UIControlEvent.TouchDown);
			
			// Wire up the series info button
			var info = UIButton.FromType(UIButtonType.InfoLight);
			info.UserInteractionEnabled = true;
			info.AddTarget((s, a)=> { ShowSeriesDetails(); }, UIControlEvent.TouchUpInside);
			
			_infoButton = new UIBarButtonItem(info);
			_listButton = new UIBarButtonItem(_buttonInner);
			
			LayoutBarButtonItems();
		}

		private void LayoutBarButtonItems()
		{
			SetRightBarButtons(true);
		}

		private void SetRightBarButtons(bool animated)
		{
			if(AppGlobal.CollectionsViewInListMode)
			{
				this.NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { _listButton }, animated);
			}
			else
			{
				this.NavigationItem.SetRightBarButtonItems(new UIBarButtonItem[] { _listButton, _infoButton }, animated);
			}
		}
		
		
		private SeriesDetailViewController _sdvc;
		
		private void ShowSeriesDetails()
		{
			var customNavigationBar = AppDelegate.NavigationBar;
			customNavigationBar.SetOpaque();
			
			if(_sdvc != null)
			{
				_sdvc.Dispose();
				_sdvc = null;
			}
			_sdvc = new SeriesDetailViewController(_series, this);
			_sdvc.NavigationItem.Title = AppManifest.Current.FirstName + " " + AppManifest.Current.LastName;
			this.NavigationController.PushViewController(_sdvc, true);
		}

		public void Flip()
		{
			AppGlobal.CollectionsViewInListMode = !AppGlobal.CollectionsViewInListMode;
			SetRightBarButtons(false);
			
			// Required to avoid not being able to reach scroll list bottom...
			_listView.View.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);
			if(_queueFraming)
			{
				_galleryView.View.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);	
				_galleryView.SetLoadingTitleTo("Rotating");
				_galleryView.PendingRotate = 0; // In case we came from series detail view
				_queueFraming = false;
			}
			
			UIView.Animate(0.75f, 0.0f, UIViewAnimationOptions.CurveEaseInOut, ()=>
			{
				_flipping = true;
				if (_galleryView.View.Superview == null)
				{	
					FlipToGalleryView();
					FlipToListViewButton();
				}
				else
				{	
					FlipToListView();
					FlipToPieceButton();
				}
			}, ()=>
			{
				_flipping = false;
			});
		}
		
		public void FlipToListViewButton()
		{
			UIView.BeginAnimations("flip_list_button");
			UIView.SetAnimationDuration(0.75f);
			UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);			
			UIView.SetAnimationTransition (UIViewAnimationTransition.FlipFromRight, _listButton.CustomView, true);
			ShowListViewButton();
			UIView.CommitAnimations();
		}
		
		public void FlipToPieceButton()
		{
			UIView.BeginAnimations("flip_piece_button");
			UIView.SetAnimationDuration(0.75f);
			UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);			
			UIView.SetAnimationTransition(UIViewAnimationTransition.FlipFromLeft, _listButton.CustomView, true);
			ShowPieceButton();
			UIView.CommitAnimations();
		}

		public void FlipToListView()
		{
			UIView.SetAnimationTransition(UIViewAnimationTransition.FlipFromRight, this.View, true);
			ShowListView();						
		}
		
		public void FlipToGalleryView()
		{
			UIView.SetAnimationTransition(UIViewAnimationTransition.FlipFromLeft, this.View, true);
			ShowGalleryView();
		}

		private void ShowListView()
		{
			InvokeOnMainThread(()=>
		    {
				_listView.ViewWillAppear(true);
				_galleryView.ViewWillDisappear(true);
				 			
				_galleryView.View.RemoveFromSuperview();
				this.View.AddSubview(_listView.View);
				
				_galleryView.ViewDidDisappear(true);
				_listView.ViewDidAppear(true);
			});
		}

		private void ShowGalleryView()
		{
			InvokeOnMainThread(()=>
		    {
				_galleryView.ViewWillAppear(true);
				_listView.ViewWillDisappear(true);
				
				_listView.View.RemoveFromSuperview();
				this.View.AddSubview(_galleryView.View);
				
				_listView.ViewDidDisappear(true);
				_galleryView.ViewDidAppear(true);
			});
		}

		private void ShowListViewButton()
		{
			_buttonGalleryThumb.Hidden = true;
			_buttonListThumb.Hidden = false;
		}

		private void ShowPieceButton()
		{
			_buttonGalleryThumb.Hidden = false;
			_buttonListThumb.Hidden = true;
		}
		
		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			SetRightBarButtons(false);
			if(AppGlobal.CollectionsViewInListMode)
			{
				_listView.WillRotate(toInterfaceOrientation, duration);		
			}
			else
			{
				_galleryView.WillRotate(toInterfaceOrientation, duration);				
			}
		}
		
		private bool _queueFraming;
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			if(AppGlobal.CollectionsViewInListMode)
			{
				_listView.DidRotate(fromInterfaceOrientation);				
				
				if(!_queueFraming)
				{
					// Completely rebuild the gallery view behind the scenes so it doesn't have to rotate...
					if(_galleryView != null)
					{
						_galleryView.View.RemoveFromSuperview();
						_galleryView.Dispose();
						_galleryView = null;				
					}
					_galleryView = new SeriesGalleryViewController(this, _listView);
					_queueFraming = true;
				}				
			}
			else
			{
				_galleryView.DidRotate(fromInterfaceOrientation);				
			}
		}
		
		public UIInterfaceOrientation PendingRotate
		{
			get
			{
				return this._galleryView.PendingRotate;	
			}
			set
			{
				this._galleryView.PendingRotate = value;	
			}
		}
	}
}

