using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.MessageUI;
using MonoTouch.MapKit;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.CoreGraphics;

namespace ArtApp
{
	public class PieceDetailViewController : SemiModalViewController 
	{
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_view != null)
				{
					_view.Dispose();
					_view = null;
				}
				if(_pfvc != null)
				{
					_pfvc.Dispose();
					_pfvc = null;
				}
			}
			base.Dispose(disposing);
		}
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return false;
		}
		
		private Piece _piece;
		private SeriesViewController _parent;
		private MFMailComposeViewController _mail;
		private PieceDetailView _view;
		
		public PieceDetailViewController(Piece piece, SeriesViewController parent)
		{
			_piece = piece;
			_parent = parent; // Required to push the full view on to the parent stack
		}
		
		public PieceDetailViewController(IntPtr handle) : base(handle)
		{
			
		}
		
		private PieceFullViewController _pfvc;
		public void PushFullView()
		{
			this.DismissSemiModalViewController(this);			
			_pfvc = InitializeFullView();			
			_parent.NavigationController.PushViewController(_pfvc, true);
		}

		private PieceFullViewController InitializeFullView()
		{
			_pfvc = new PieceFullViewController(_piece, _parent);						
			var label = new UILabel();
			label.Font = UIFont.BoldSystemFontOfSize(15.0f);
			label.BackgroundColor = UIColor.Clear;
			label.TextColor = UIColor.White;
			label.ShadowColor = UIColor.Black;
			label.ShadowOffset = new SizeF(0, -1);
			label.Text = _piece.Title;
			label.SizeToFit();
			_pfvc.NavigationItem.TitleView = label;
			return _pfvc;
		}
		
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			
			if(_view != null)
			{
				_view.Dispose();
				_view = null;
			}	

			// The piece detail view covers the whole screen, since it creates its own empty space semi-modally
			_view = new PieceDetailView(_piece, new RectangleF(
				0, 0, 
				UIScreen.MainScreen.Bounds.Width, 
				UIScreen.MainScreen.Bounds.Height
			));
			_view.Parent = this;
			
			if(_view.BuyButton != null)
			{
				_view.BuyButton.TouchUpInside += (o, e) =>
				{
					if (MFMailComposeViewController.CanSendMail)
					{
						_mail = new CustomMailer();						
						_mail.SetSubject("Purchase Inquiry");
			            _mail.SetMessageBody("Hi " + AppManifest.Current.FirstName + ",\r\n\r\nI would like to purchase your work titled '" + _piece.Title + "'.\r\n\r\nPlease let me know how I may do so.\r\n\r\nThanks!", false);
						_mail.SetToRecipients(new string[] { AppManifest.Current.Email });
			            _mail.Finished += HandleMailFinished;
						
						var controller = (CustomNavigationController)AppDelegate.NavigationController;
						
						controller.PresentModalViewController(_mail, true);
						_view.BuyButton.Cancel();
			        }
					else
					{
			            UIAlertView alert = new UIAlertView("Mail Alert", "Mail Unavailable", null, "Please try again later.", null);
		        		alert.Show();
						_view.BuyButton.Cancel();
			        }				
				};
			}
			
			_view.PictureTapped += delegate
			{
				PushFullView();
			};

			_view.OffsideTapped += delegate
			{
				this.DismissSemiModalViewController(this);	
			};

			View = _view;
		}
		
		private void HandleMailFinished(object sender, MFComposeResultEventArgs e)
		{
			if (e.Result == MFMailComposeResult.Sent)
			{
		        UIAlertView alert = new UIAlertView("Mail Alert", "Mail Sent", null, "Good luck!", null);
		        alert.Show();
		    }
			else if (e.Result == MFMailComposeResult.Failed)
			{
				UIAlertView alert = new UIAlertView("Mail Alert", "Mail Failed", null, "Please try again later.", null);
		        alert.Show();
			}
								
			e.Controller.DismissModalViewControllerAnimated(true);
		}
	}
}

