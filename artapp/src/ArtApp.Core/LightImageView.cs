using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;

namespace ArtApp
{
	public class LightImageView : UIImageView
	{
		private bool _isOn;
		
		public bool On
		{
			get
			{
				return _isOn;	
			}
		}
		
		public bool Off
		{
			get
			{
				return !_isOn;	
			}
		}
		
		public LightImageView(UIImage image) : base(image)
		{
			this.Alpha = 0.0f;
		}
		
		public void TurnOn()
		{
			_isOn = true;			
			UIView.Animate(0.33f, delegate { this.Alpha = 1.0f; });
		}
		
		public void TurnOff()
		{
			_isOn = false;
			UIView.Animate(0.33f, delegate { this.Alpha = 0.0f; });
		}
	}
}
