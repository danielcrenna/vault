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
	public class CustomMailer : MFMailComposeViewController
	{
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}	
	}
}
