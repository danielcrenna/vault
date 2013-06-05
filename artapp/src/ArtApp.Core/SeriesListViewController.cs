using System;
using MonoTouch.UIKit;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using System.Threading;
using MonoTouch.Dialog;

namespace ArtApp
{
	[Register("SeriesListViewController")]
	public class SeriesListViewController : CustomDialogViewController
	{
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(_section != null)
				{
					_section.Dispose();
					_section = null;
				}
			}
			base.Dispose(disposing);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		private SeriesViewController _parent;
		private Section _section;
			
		public SeriesListViewController(SeriesViewController parent) : base("Images/backgrounds/wall.jpg", null)
		{
			_parent = parent;
			EnableSearch = true;
			SearchPlaceholder = "Search for a piece";
			Root = GetRoot();
			View.Frame = parent.View.Frame;
		}
		
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);	
			AppDelegate.NavigationController.NavigationBar.TopItem.Title = "";	
		}
		
		public void SelectPieceInList(int page)
		{
			foreach(StyledStringElement e in _section.Elements)
			{
				e.Accessory = UITableViewCellAccessory.None;
			}
			CheckPiece (page);
			ReloadData(); // Required to refresh UI with new checkmark
		}

		public void CheckPiece(int page)
		{
			var selected = (StyledStringElement)_section.Elements[page];
			selected.Accessory = UITableViewCellAccessory.Checkmark;
		}
		
		private RootElement GetRoot()
		{
			IList<Piece> pieces = _parent.Series.Pieces;			
						
			var root = new RootElement("");
			_section = new Section(string.Concat(_parent.Series.Title, " (", _parent.Series.Years , ")"));
									
			for(var i = 0; i < pieces.Count; i++)
			{
				var page = i;
				var piece = pieces[page];
				
				var viewAction = new NSAction(() =>
				{
					GoToPage(page);
				});				
				
				var element = new StyledStringElement(piece.Title, viewAction);		
				element.Activity = true;
				_section.Add(element);
								
				ThreadPool.QueueUserWorkItem(s =>
				{
					var rounded = ImageFactory.LoadRoundedThumbnail(piece);
					InvokeOnMainThread(()=>
					{
						element.Image = rounded;	
						element.Reload();
					});
				});											
			}
						
			// Set the default check box
			CheckPiece(0);
			root.Add(_section);		
			
			View.SetNeedsDisplay();
			return root;
		}

		public void GoToPage(int page, bool flip = true)
		{
			// Checkmark piece
			SelectPieceInList(page);
			
			_parent.Page = page;
												
			if(flip)
			{
				_parent.Flip();	
			}
		}
		
		public override void WillRotate (UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);
		}
		
		public override void DidAnimateFirstHalfOfRotation (UIInterfaceOrientation toInterfaceOrientation)
		{
			base.DidAnimateFirstHalfOfRotation (toInterfaceOrientation);
		}
		
		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			SetScrollToTop();
		}
	}
}

