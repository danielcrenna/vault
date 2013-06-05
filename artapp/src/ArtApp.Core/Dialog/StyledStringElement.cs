using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using ArtApp;

namespace MonoTouch.Dialog
{
	public class StyledStringElement : StringElement, IColorizeBackground
	{
		static NSString [] skey = { new NSString (".1"), new NSString (".2"), new NSString (".3"), new NSString (".4") };
		
		public StyledStringElement (string caption) : base (caption) {}
		public StyledStringElement (string caption, NSAction tapped) : base (caption, tapped)
		{
			style = UITableViewCellStyle.Value1;
		}
		public StyledStringElement (string caption, string value) : base (caption, value) 
		{
			style = UITableViewCellStyle.Value1;	
		}
		public StyledStringElement (string caption, string value, UITableViewCellStyle style) : base (caption, value) 
		{ 
			this.style = style;
			SelectionStyle = UITableViewCellSelectionStyle.Blue;
		}
		
		protected UITableViewCellStyle style;
		public UIFont Font;
		public UIFont SubtitleFont;
		public UIColor TextColor;
		public UILineBreakMode LineBreakMode = UILineBreakMode.WordWrap;
		public int Lines = 0;
		public UITableViewCellAccessory Accessory = UITableViewCellAccessory.None;
		
		// To keep the size down for a StyleStringElement, we put all the image information
		// on a separate structure, and create this on demand.
		ExtraInfo extraInfo;
		
		class ExtraInfo {
			public UIImage Image; // Maybe add BackgroundImage?
			public UIColor BackgroundColor, DetailColor;
			public Uri Uri, BackgroundUri;
			public bool Activity;
			public bool New;
		}

		ExtraInfo OnImageInfo ()
		{
			if (extraInfo == null)
				extraInfo = new ExtraInfo ();
			return extraInfo;
		}
		
		// Uses the specified image (use this or ImageUri)
		public UIImage Image {
			get {
				return extraInfo == null ? null : extraInfo.Image;
			}
			set {
				OnImageInfo ().Image = value;
				extraInfo.Uri = null;
				if(value != null)
				{
					// Turn off the loading activity spinner
					OnImageInfo().Activity = false;
				}
			}
		}
		
		public bool Activity
		{
			get
			{
				return extraInfo == null ? false : extraInfo.Activity;	
			}
			set
			{
				OnImageInfo().Activity = value;	
			}
		}
		
		public bool New
		{
			get
			{
				return extraInfo == null ? false : extraInfo.Activity;	
			}
			set
			{
				OnImageInfo().New = value;	
			}
		}
		
		// Loads the image from the specified uri (use this or Image)
		public Uri ImageUri
		{
			get
			{
				return extraInfo == null ? null : extraInfo.Uri;
			}
			set
			{
				OnImageInfo ().Uri = value;
				extraInfo.Image = null;
			}
		}
		
		// Background color for the cell (alternative: BackgroundUri)
		public UIColor BackgroundColor
		{
			get
			{
				return extraInfo == null ? null : extraInfo.BackgroundColor;
			}
			set
			{
				OnImageInfo ().BackgroundColor = value;
				extraInfo.BackgroundUri = null;
			}
		}
		
		public UIColor DetailColor
		{
			get
			{
				return extraInfo == null ? null : extraInfo.DetailColor;
			}
			set
			{
				OnImageInfo ().DetailColor = value;
			}
		}
		
		// Uri for a Background image (alternatiev: BackgroundColor)
		public Uri BackgroundUri {
			get
			{
				return extraInfo == null ? null : extraInfo.BackgroundUri;
			}
			set
			{
				OnImageInfo ().BackgroundUri = value;
				extraInfo.BackgroundColor = null;
			}
		}
			
		protected virtual string GetKey (int style)
		{
			return skey [style];
		}
		
		public UITableViewCellSelectionStyle SelectionStyle { get; set; }
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var key = GetKey ((int) style);
			var cell = tv.DequeueReusableCell (key);
			if (cell == null)
			{
				// Set the cell as a badge cell (hidden when text is blank)
				if(extraInfo != null && extraInfo.New)
				{
					cell = new BadgeTableViewCell(style, key);
				}
				else
				{
					cell = new UITableViewCell(style, key);
				}
				cell.SelectionStyle = SelectionStyle;
			}
			
			PrepareCell (cell);
			return cell;
		}
		
		private UIActivityIndicatorView _spinner;
		private UIImage _whiteback;
		
		private void PrepareCell (UITableViewCell cell)
		{
			cell.Accessory = Accessory;
			var tl = cell.TextLabel;
			tl.Text = Caption;
			tl.TextAlignment = Alignment;
			tl.TextColor = TextColor ?? UIColor.Black;
			tl.Font = Font ?? UIFont.BoldSystemFontOfSize (17);
			tl.LineBreakMode = LineBreakMode;
			tl.Lines = Lines;	
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
			{
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			}
			
			if (extraInfo == null)
			{
				cell.ContentView.BackgroundColor = null;
				tl.BackgroundColor = null;
			}
			else
			{
				var imgView = cell.ImageView;
				UIImage img;
				
				if (extraInfo.Image != null)
				{
					img = extraInfo.Image;
				}
				else 
				{
					img = null;
				}
				
				imgView.Image = img;
				
				// http://stackoverflow.com/questions/1269188/iphone-sdk-adding-a-uiactivityindicatorview-to-a-uitableviewcell
				if(extraInfo.Activity)
				{
					if(_spinner == null)
					{
						_spinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
						_spinner.Frame = new System.Drawing.RectangleF(0, 0, 25, 25);
						_whiteback = UIImageEx.FromFile(@"Images/whiteback.png");
					}
					cell.ImageView.Image = _whiteback;
					cell.ImageView.AddSubview(_spinner);
					if(!_spinner.IsAnimating)
					{
						_spinner.StartAnimating();		
					}				
				}
				else
				{
					if(_spinner != null)
					{
						_spinner.StopAnimating();
						_spinner.RemoveFromSuperview();
						_spinner = null;
						_whiteback = null;
					}
				}
				
				if (cell.DetailTextLabel != null)
				{
					cell.DetailTextLabel.TextColor = extraInfo.DetailColor ?? UIColor.Black;
				}
				
				var badgeCell = cell as BadgeTableViewCell;
				if(badgeCell != null && extraInfo.New)
				{
					badgeCell.BadgeView.TextLabel.Text = @"New";
					badgeCell.BadgeView.BadgeColor = new UIColor(0.388f, 0.686f, 0.239f, 1.0f);
				}
				
				if (cell.DetailTextLabel != null)
				{
					cell.DetailTextLabel.Lines = Lines;
					cell.DetailTextLabel.LineBreakMode = LineBreakMode;
					cell.DetailTextLabel.Font = SubtitleFont ?? UIFont.SystemFontOfSize (14);
				}
			}
		}	
	
		void ClearBackground (UITableViewCell cell)
		{
			cell.BackgroundColor = UIColor.White;
			cell.TextLabel.BackgroundColor = UIColor.Clear;
		}

		void IColorizeBackground.WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			if (extraInfo == null)
			{
				ClearBackground (cell);
				return;
			}
			
			if (extraInfo.BackgroundColor != null)
			{
				cell.BackgroundColor = extraInfo.BackgroundColor;
				cell.TextLabel.BackgroundColor = UIColor.Clear;
			}
			else 
			{
				ClearBackground (cell);
			}
		}
		
		public void Reload()
		{
			var root = GetImmediateRootElement ();
			if (root == null || root.TableView == null)
			{
				return;
			}
			root.TableView.ReloadRows (new NSIndexPath [] { IndexPath }, UITableViewRowAnimation.Fade);
		}
	}
}


