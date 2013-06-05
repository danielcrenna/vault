using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace ArtApp
{
	public class BadgeTableViewCell : UITableViewCell
	{
		private BadgeView _badgeView;
		
		public BadgeView BadgeView
		{
			get
			{
				return _badgeView;	
			}
		}
		
		public BadgeTableViewCell(UITableViewCellStyle style, string reuseIdentifier) : base(style, reuseIdentifier)
		{
			_badgeView = new BadgeView(new RectangleF(0.0f, 0.0f, 55.0f, 20.0f));
			_badgeView.BackgroundColor = UIColor.Clear;
			_badgeView.BadgeAlignment = BadgeViewAlignment.Right;
			AccessoryView = _badgeView;
		}
	}
}

