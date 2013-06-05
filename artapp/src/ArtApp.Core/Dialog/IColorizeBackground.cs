using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public interface IColorizeBackground
	{
		void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath);
	}
	
}
