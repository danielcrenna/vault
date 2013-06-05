using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace ArtApp
{
	// Original source: https://github.com/samsoffes/sstoolkit/blob/master/SSToolkit

	[Register("CustomWindow")]
	public class CustomWindow : UIWindow
	{
		private static CustomWindow _window;
		private bool _hidesVignette;
		
		public CustomWindow(IntPtr handle) : base(handle)
		{
			
		}
		
		public bool HidesVignette
		{
			get
			{
				return _hidesVignette;	
			}
			set
			{
				SetHidesVignette(value);
			}
		}
		
		public static CustomWindow DefaultWindow
		{
			get
			{
				return _window ?? (_window = new CustomWindow());
			}
		}
		
		public CustomWindow() : base(UIScreen.MainScreen.Bounds)
		{
			BackgroundColor = UIColor.Clear;
			WindowLevel = UIWindow.LevelStatusBar + 1.0f;
		}

		public void SetHidesVignette(bool hide)
		{
			_hidesVignette = hide;
			UserInteractionEnabled = !hide;
			SetNeedsDisplay();
		}
		
		public override void Draw(RectangleF rect)
		{
			if (_hidesVignette)
			{
				return;
			}

			var imageName = Util.IsPad() ? @"Images/SSVignetteiPad.png" : @"Images/SSVignetteiPhone.png";
			UIImage image = UIImage.FromBundle(imageName);
		
			var screenSize = UIScreen.MainScreen.Bounds.Size;
			image.Draw(new RectangleF((float)Math.Round((screenSize.Width - image.Size.Width) / 2.0f), 
										 (float)Math.Round((screenSize.Height - image.Size.Height) / 2.0f), 
										 image.Size.Width, image.Size.Height));
			image.Dispose();
		}
	}
}

