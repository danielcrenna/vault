using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace ArtApp
{
	[Register("CustomNavigationController")]
	public class CustomNavigationController : UINavigationController 
	{
		public CustomNavigationController (IntPtr handle) : base(handle)
		{
			
		}
		
		// http://aralbalkan.com/2334
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return AppGlobal.MayRotate(toInterfaceOrientation);	
		}
	}
}

