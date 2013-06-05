using System;
using MonoTouch.UIKit;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using MonoTouch.Foundation;
using System.Collections.Generic;
using MonoTouch.CoreGraphics;

namespace ArtApp
{
	public class SeriesGalleryViewController : UIViewController
	{
		public PagedViewController Pager
		{
			get
			{
				return _pager;	
			}
		}
		
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(_pending != null)
				{
					_pending.Dispose();
					_pending = null;
				}
				if(_pager != null)
				{
					_pager.Dispose();
					_pager = null;
				}
			}
			
			base.Dispose(disposing);	
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		public bool Rotating
		{
			get
			{
				return _rotating;				
			}
		}
		
		private bool _rotating;
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			if(!_rotating)
			{
				base.DidRotate(fromInterfaceOrientation);
				if(_pending != null && _pending.Superview != null)
				{
					_pending.RemoveFromSuperview();
					_pending.Dispose();
					_pending = null;
				}
				Rotate();
			}
		}
		
		private UIImageView _pending;
		
		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{	
			base.WillRotate(toInterfaceOrientation, duration);
			
			var landscape = toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft ||
							toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
			
			var views = new List<UIView>(this.View.Subviews);
			_pending = AddPendingBackground(landscape);
			foreach(var view in views)
			{
				view.RemoveFromSuperview();
			}
		}
		
		private bool _isLandscape;
		public bool IsLandscape
		{
			get
			{
				return _isLandscape;		
			}
		}
		
		private SeriesViewController _parent;
		private SeriesListViewController _sibling;
		private PagedViewController _pager;
		private LoadingView _loading;
		
		public SeriesGalleryViewController(SeriesViewController parent, SeriesListViewController sibling)
		{
			_parent = parent;
			_sibling = sibling;
			_isLandscape = Util.IsLandscape();
		}
		
		public UIInterfaceOrientation PendingRotate { get; set; }
		
		// State control necessary for flip action...
		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			
			// We were in one orientation and are now trying to pop into the other
			if(PendingRotate != 0)
			{
				this.DidRotate(PendingRotate);
				PendingRotate = 0;
			}
			
			if(_parent.Flipping && !_queuedPage.HasValue)
			{
				SetPage(_pager.Page);					
			}
			
			SetPiecesAffectedByLightSwitch();
		}
		
		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			
			if(_parent.Flipping)
			{
				AppDelegate.NavigationBar.SetOpaque();
				AppDelegate.NavigationBar.TopItem.Title = "";
			}					
		}

		private void SetPiecesAffectedByLightSwitch()
		{
			if(_pager == null)
			{
				return;
			}
			
			AppGlobal.PiecesInView = _pager.Pages;
		}
		
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			
			if(_parent.Flipping)
			{
				AppDelegate.NavigationBar.SetTransparent();
				AppDelegate.NavigationBar.TopItem.Title = _parent.Series.Title;
			}
		}

		public void Rotate()
		{
			_rotating = true;
			AppGlobal.RotationPending = true;
			ResetLoadingViewWithTitle("Rotating");
			InitializeWithCurrentOrientation();
			_isLandscape = !_isLandscape;
		}	
				
		public void SetLoadingTitleTo(string title)
		{
			_loading.Text = title;	
		}
		
		public void ResetLoadingViewWithTitle(string title)
		{
			if(_loading != null)
			{
				_loading.RemoveFromSuperview();
				_loading.Dispose();
				_loading = null;
			}
			_loading = new LoadingView(title);
			_loading.SetTransformForCurrentOrientation(false);
		}
				
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();			
			ResetLoadingViewWithTitle("Loading");
			InitializeWithCurrentOrientation();
		}
		
		private UIButton _lightButton;
		private void InitializeWithCurrentOrientation()
		{
			var background = AddPendingBackground(Util.IsLandscape());
						
			if(this._loading.Superview == null)
			{
				this.View.AddSubview(_loading);	
			}
			_loading.Show();
			
			RemovePager ();
			
			ThreadPool.QueueUserWorkItem(s =>
			{
				var startAt = _rotating && _pager != null ? _pager.Page : 0;
				
				InitializePager(startAt);
				
				SetPiecesAffectedByLightSwitch();
				
				InvokeOnMainThread(() =>
			    {
					// Swap the placeholder image for the gallery
					this.View.AddSubview(_pager.View);
					background.RemoveFromSuperview();
					background.Dispose();
					
					PlaceLightButton();
					
					_loading.CompleteAndDismissWithTitle("Ready!", 1.0f, CompleteLoad);
										
					// Slide to a piece if one is queued for display
					if(_queuedPage.HasValue)
					{
						SetPage(_queuedPage.Value);
						_queuedPage = null;
					}
				});
			});
		}

		public void RemovePager()
		{
			if(_pager != null && _rotating)
			{
				_pager.View.RemoveFromSuperview();
				_pager.Dispose();
			}
		}

		public void InitializePager(int startAt)
		{
			_pager = new PagedViewController();
			_pager.PagedViewDataSource = new SeriesDataSource(_parent.Series, _parent);
			_pager.PageChanged += delegate(object sender, PagedViewEventArgs e)
			{
				_parent.Page = e.Page;
				_sibling.SelectPieceInList(_parent.Page);
			};			
			_pager.ReloadPages(startAt);
		}

		private void PlaceLightButton()
		{
			// Add or move the light button overlay
			_lightButton = _lightButton ?? BuildLightButton();
			
			if(_lightButton.Superview != null)
			{
				_lightButton.RemoveFromSuperview();	
			}
			
			// Flipping from list view can "dirty" this value, so choose it by orientation explicitly instead
			var bounds = Util.IsLandscape() ? new RectangleF(0, 0, 480, 320 - 20) : new RectangleF(0, 0, 320, 480 - 20);
					
			_lightButton.Frame = new RectangleF(
				bounds.Width - _lightButton.Bounds.Width, 
				bounds.Height - _lightButton.Bounds.Height, 
				_lightButton.Bounds.Width, 
				_lightButton.Bounds.Height
			);

			View.AddSubview(_lightButton);	
		}

		public UIImageView AddPendingBackground(bool landscape)
		{
			var backgroundImage = ImageFactory.BuildCroppedGalleryBackground(landscape);
			var background = new UIImageView(backgroundImage);
			backgroundImage.Dispose();
			
			background.Frame = GetGalleryDimensions(landscape);

			View.AddSubview(background);
			return background;
		}
		
		private int? _queuedPage;
		public int? QueuedPage
		{
			get
			{
				return _queuedPage;
			}
			set
			{
				_queuedPage = value;
			}
		}
				
		public void SetPage(int page)
		{
			if(_pager == null)
			{
				_queuedPage = page;
				return;	
			}
			_pager.Page = page;
		}
		
		private UIButton BuildLightButton()
		{
			var off = UIImage.FromBundle("Images/gallery/lights_off.png");
			var on = UIImage.FromBundle("Images/gallery/lights_on.png");
			
			var button = UIButton.FromType(UIButtonType.Custom);
			button.UserInteractionEnabled = true;
			button.Bounds = new RectangleF(0, 0, off.Size.Width, off.Size.Height);
			button.SetBackgroundImage(off, UIControlState.Normal);
			button.SetBackgroundImage(on, UIControlState.Highlighted);
			button.AddTarget(delegate { ToggleLights(); }, UIControlEvent.TouchUpInside);
			
			off.Dispose();
			on.Dispose();
			
			return button;
		}
		
		private void CompleteLoad()
		{
			InvokeOnMainThread(()=> // Invoked on a callback
			{
				ToggleLights();
				_loading.RemoveFromSuperview();
				_loading.Dispose();
				_loading = null;			
				_rotating = false;
				AppGlobal.RotationPending = false;
			});
		}
		
		private void ToggleLights()
		{
			foreach(var vc in AppGlobal.PiecesInView)
			{
				vc.ToggleLights();
			}
		}
		
		private RectangleF GetGalleryDimensions(bool landscape)
		{
			RectangleF frame;
			if(!landscape)
			{
				frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - DimensionSet.StatusBarHeight);
			}
			else
			{
				frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width - DimensionSet.StatusBarHeight);
			}
			return frame;
		}
	}
}

