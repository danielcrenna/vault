using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;

namespace ArtApp
{
	public class GalleryImageView : UIImageView
	{
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				Lights.Image.Dispose();
				Lights.Image = null;
				Lights.Dispose();
			}
			base.Dispose (disposing);
		}
		
		public LightImageView Lights { get; set; }
		
		public GalleryImageView(UIImage image) : base (image)
		{
			
		}
	}	
}

