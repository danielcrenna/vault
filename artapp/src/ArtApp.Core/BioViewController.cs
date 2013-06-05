using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using System.Net;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;

namespace ArtApp
{
	public class BioViewController : CustomDialogViewController 
	{
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);
		}
		
		public const int PadX = 4;
		
		private Bio _bio;
		private BioView _view;
		private UIViewElement _tweetBox;
		private Section _main;
		
		public BioViewController() : base("Images/backgrounds/wall.jpg", null)
		{
			_bio = new Bio();			
			Root = GetRoot();
		}
		
		public RootElement GetRoot()
		{
			CreateTweetView("...");
			
			var width = View.Bounds.Width - 30 - PadX * 2;
			var frame = new RectangleF(PadX, 0, width, 100);
			
			var headerView = new UIView(frame);			
			_view = new BioView(_bio, frame, true);
			headerView.Add(_view);
			
			// Speech bubble triangle
			var triangleFrame = new RectangleF(Util.IsPad() ? 63 : 43, _view.Bounds.Height - 7, 16, 8);
			var triangle = new TriangleView (UIColor.FromRGB(247, 247, 247), UIColor.FromRGB(171, 171, 171)) { Frame = triangleFrame };
			headerView.Add(triangle);
						
			_view.UrlTapped += delegate
			{
				WebViewController.OpenUrl(this, _bio.Url);
			};
					
			_main = new Section(headerView)
			{
				_tweetBox
			};
			
			var text = new StyledMultilineElement(AppManifest.Current.Biography);
			text.TextColor = UIColor.DarkGray;
			text.LineBreakMode = UILineBreakMode.WordWrap;
			text.Font = UIFont.ItalicSystemFontOfSize(15);
			text.DetailColor = text.TextColor;	
			text.SelectionStyle = UITableViewCellSelectionStyle.None;
						
			var secondary = new Section("About " + AppManifest.Current.FirstName)
			{
				text
			};
			
			var root = new RootElement("Bio")
			{
				_main,
				secondary
		 	};			
			
			// Required for resizing bubble for new tweets
			root.UnevenRows = true;
			
			return root;
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear(animated);
			this.NavigationController.NavigationBar.TopItem.Title = "Bio";
		}
		
		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate (toInterfaceOrientation, duration);			
		}
		
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			this.SetScrollToTop();			
			
			 ((TweetView)_tweetBox.View).Layout();
			var tweetRect = new RectangleF (PadX, 0, View.Bounds.Width - 30 - PadX * 2, ((TweetView)_tweetBox.View).Height);			
			_tweetBox.View.Frame = tweetRect;
			_tweetBox.View.SetNeedsDisplay();
		}
		
		private void CreateTweetView(string tweet)
		{
			var boxWidth = View.Bounds.Width - 30 - PadX * 2;
			var tweetRect = new RectangleF (PadX, 0, boxWidth, 100);
						
			var tweetView = new TweetView(tweetRect, tweet, (t) =>
			{
				string url = t.Value;
				switch(t.Type)
				{
					case TweetView.TweetType.Url:
						break;
					case TweetView.TweetType.Mention:
						url = "http://twitter.com/" + t.Value;
						break;
					case TweetView.TweetType.Hashtag:
						url = "http://twitter.com/search/" + Uri.EscapeDataString(t.Value);
						break;
				}
				WebViewController.OpenUrl(this, url, true /* enableTitle */);
			}, null);
			_tweetBox = new UIViewElement("Twitter", tweetView, false);
		}
		
		
		private bool _fetching;
		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			
			// http://stackoverflow.com/questions/7436870/resizing-monotouch-dialog-styledmultilineelement-after-an-async-call
			// NameResolutionFailure on device...
			Util.PushNetworkActive();
			
			if(_bio.UpdateImage)
			{
				_view.ShowActivity();
			}
			
			if(!_fetching)
			{
				_fetching = true;
				ThreadPool.QueueUserWorkItem(s =>
				{					
					try
					{
						var client = new WebClient();
						var json = client.DownloadString(string.Format("https://api.twitter.com/users/show/{0}.json?include_entities=1", _bio.Twitter));
						var user = JsonParser.FromJson(json);
					    client.Dispose(); 
						
						// http://a2.twimg.com/profile_images/1558350756/image.jpg
						// http://a2.twimg.com/profile_images/1558350756/image_normal.jpg
						//var url = user["profile_image_url"].ToString().Replace("_normal", "_reasonably_small"); // Gets the 128px version
						var url = user["profile_image_url"].ToString().Replace("_normal", ""); // Gets the full version
						
						// Sometimes Twitter does not return the inline status (this is obviously a bug)
						string tweet;
						if(!user.ContainsKey("status"))
						{
							tweet = "...";
						}
						else
						{
							tweet = ((IDictionary<string, object>)user["status"])["text"].ToString();
						}		
						
						UIImage image = null;
						
						// Update twitter image on disk
						if(_bio.UpdateImage)
						{
							NSUrl imageUrl = NSUrl.FromString(url);
							NSData imageData = NSData.FromUrl(imageUrl);

							image = UIImage.LoadFromData(imageData);
							image = ImageHelper.RoundAndSquare(image, 14);

							ImageCache.StoreAs("latest_twitter", image);
							imageData.Dispose();
						}
						else
						{
							image = _view.GetDefaultBioImage();	
						}
						
						InvokeOnMainThread(()=>
						{
						 	Util.PopNetworkActive();						
							_bio.Image = image;				
							_main.Remove(_tweetBox);
							CreateTweetView(tweet);
							_main.Add(_tweetBox);						
							_view.AddOrUpdateBio(_bio);
							
							image.Dispose();						
							this.View.SetNeedsDisplay();
						});
					}
					catch
					{

					}
					finally
					{
						InvokeOnMainThread(()=>
						{
							_view.HideActivity();
							_fetching = false;
						});	
					}
				});
			}
		}
	}
}

