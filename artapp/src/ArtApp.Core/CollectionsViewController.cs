using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Diagnostics;
using System.Threading;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.CoreAnimation;

namespace ArtApp
{
	public class CollectionsViewController : CustomDialogViewController
	{
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				foreach(var vc in _vcs)
				{
					vc.Dispose();	
				}
				_vcs.Clear();
				_vcs = null;
			}
			base.Dispose (disposing);
		}
		
		private List<SeriesViewController> _vcs = new List<SeriesViewController>(1);
		private SeriesViewController _vc
		{
			get
			{
				if(_vcs == null || _vcs.Count == 0)
				{
					return null;	
				}
				return _vcs[0];
			}
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		public CollectionsViewController() : base("Images/backgrounds/wall.jpg", null)
		{
			Root = GetRoot();
		}
		
		private RootElement GetRoot()
		{
			IList<Series> coll = AppManifest.Current.Collections;
			
			var root = new RootElement("Collections");
			var section = new Section();
			
			for(var i = 0; i < coll.Count; i++)
			{
				Series series = coll[i];
				var rounded = ImageFactory.LoadRoundedThumbnail(series.Pieces[0]);
				var viewAction = new NSAction(() => { LoadSeries (series); });
				
				var element = new StyledStringElement(series.Title, viewAction) { Image = rounded };
				element.New = series.New.GetValueOrDefault();
				section.Add(element);
			}
			
			root.Add(section);			
			return root;
		}
		
		public void LoadSeries(Series series, Piece piece)
		{
			if(piece != null)
			{
				if(AppGlobal.CollectionsViewInListMode)
				{
					AppGlobal.CollectionsViewInListMode = false;	
				}
			}
			
			ResolveSeriesViewController(series);
			
			if(_vc.NavigationItem.TitleView != null)
			{
				_vc.NavigationItem.TitleView.Dispose();	
				_vc.NavigationItem.TitleView = null;
			}
			var label = new UILabel();
			label.Font = UIFont.BoldSystemFontOfSize(14.0f);
			label.BackgroundColor = UIColor.Clear;
			label.TextColor = UIColor.White;
			label.Text = series.Title;
			label.SizeToFit();
			_vc.NavigationItem.TitleView = label;
			
			this.NavigationController.PushViewController(_vc, true);
			
			if(piece != null)
			{
				_vc.GoToPiece(series, piece);	
			}
		}

		private void ResolveSeriesViewController(Series series)
		{
			if(_vc != null && !_vc.Series.Equals(series))
			{
				_vc.Dispose();
				_vcs.Clear();
				var vc = new SeriesViewController(series);
				_vcs.Add(vc);
			}
			
			if(_vc == null)
			{
				var vc = new SeriesViewController(series);
				_vcs.Add(vc);
			}
		}
		
		public void LoadSeries(Series series)
		{
			LoadSeries(series, null);
		}
		
		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);			
			
			NavigationController.NavigationBar.TopItem.Title = "Collections";
			
			AppDelegate.NavigationBar.SetOpaque();
		}
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			if(_vc != null)
			{
				_vc.PendingRotate = fromInterfaceOrientation;				
			}
			base.DidRotate (fromInterfaceOrientation);
		}
	}
}

