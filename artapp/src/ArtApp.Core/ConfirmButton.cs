using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.CoreGraphics;

namespace ArtApp
{
	// Ported from: https://github.com/mikeahmarani/MAConfirmButton
	
	public class ConfirmButton : UIButton
	{
		protected override void Dispose (bool disposing)
		{
			if(disposing)
			{
				if(_colorLayer != null)
				{
					_colorLayer.Dispose();
					_colorLayer = null;
				}
			}
			base.Dispose (disposing);
		}
		
		public const float Height = 26.0f;
		public const float Padding = 20.0f;
		public const float FontSize = 14.0f;
		
		private string _title;
		private string _confirm;
		private string _disabled;
		private UIColor _tint;
		
		private CALayer _colorLayer;
					
		public ConfirmButton(string disabled) : base(new RectangleF())
		{
			_disabled = disabled;
			
			Layer.NeedsDisplayOnBoundsChange = true;
			
			var size = TitleLabel.StringSize(_disabled, UIFont.BoldSystemFontOfSize(FontSize)); 
			
			Frame = new RectangleF(Frame.X, Frame.Y, size.Width + Padding, Height);
			
			SetTitle(_disabled, UIControlState.Normal);
			SetTitleColor(UIColor.FromWhiteAlpha(0.6f, 1.0f), UIControlState.Normal);
			SetTitleShadowColor(UIColor.FromWhiteAlpha(1.0f, 1.0f), UIControlState.Normal);
			
			TitleLabel.TextAlignment = UITextAlignment.Center;
			TitleLabel.ShadowOffset = new SizeF(0.0f, 1.0f);
			TitleLabel.BackgroundColor = UIColor.Clear;
			TitleLabel.Font = UIFont.BoldSystemFontOfSize(FontSize);
			_tint = UIColor.FromWhiteAlpha(0.85f, 1.0f);
			SetupLayers();			
		}
				
		public ConfirmButton(string title, string confirm) : base(new RectangleF())
		{
			_title = title;
			_confirm = confirm;
			
			Layer.NeedsDisplayOnBoundsChange = true;
			
			var size = TitleLabel.StringSize(_title, UIFont.BoldSystemFontOfSize(FontSize));
		    			
			Frame = new RectangleF(Frame.X, Frame.Y, size.Width + Padding, Height);
			SetTitle(_title, UIControlState.Normal);
			SetTitleColor (UIColor.White, UIControlState.Normal);
			SetTitleShadowColor(UIColor.FromWhiteAlpha(0, 0.5f), UIControlState.Normal); 
			
			TitleLabel.TextAlignment = UITextAlignment.Center;
			TitleLabel.ShadowOffset = new SizeF(0, -1);
			TitleLabel.BackgroundColor = UIColor.Clear;
			TitleLabel.Font = UIFont.BoldSystemFontOfSize(FontSize);	
			_tint = new UIColor(0.220f, 0.357f, 0.608f, 1.0f);
			
			SetupLayers();			
		}
		
		public void Toggle()
		{
			TitleLabel.Alpha = 0;

			SizeF size;
			
			if(!string.IsNullOrEmpty(_disabled))
			{
				SetTitle(_disabled, UIControlState.Normal);;
				SetTitleColor(UIColor.FromWhiteAlpha(0.6f, 1.0f), UIControlState.Normal);
				SetTitleShadowColor(UIColor.FromWhiteAlpha(1.0f, 1.0f), UIControlState.Normal);
				TitleLabel.ShadowOffset = new SizeF(0.0f, 1.0f);
				size = TitleLabel.StringSize(_disabled, UIFont.BoldSystemFontOfSize(FontSize));
			}
			else if(!string.IsNullOrEmpty(_confirm) && Selected)
			{
				SetTitle(_confirm, UIControlState.Normal);
				size = TitleLabel.StringSize(_confirm, UIFont.BoldSystemFontOfSize(FontSize));
			}
			else
			{		
				SetTitle(_title, UIControlState.Normal);
				size = TitleLabel.StringSize(_title, UIFont.BoldSystemFontOfSize(FontSize));
			}
			
			size.Width += Padding;
			float offset = size.Width - Frame.Size.Width;
			
			CATransaction.Begin();
			CATransaction.AnimationDuration = 0.25f;
			CATransaction.CompletionBlock = ()=>
			{
				// Re-adjust button frame for new touch area, move layers back now that animation is done
				var frameRect = new RectangleF(Frame.X - offset, Frame.Y, Frame.Size.Width + offset, Frame.Size.Height);
				Frame = frameRect;
		
				CATransaction.DisableActions = true;
				foreach(var layer in Layer.Sublayers)
				{
					layer.Frame = new RectangleF(layer.Frame.X + offset, layer.Frame.Y, layer.Frame.Size.Width, layer.Frame.Size.Height);
				}
				CATransaction.Commit();
				TitleLabel.Alpha = 1;
				SetNeedsLayout();
			};
			
			UIColor greenColor = new UIColor(0.439f, 0.741f, 0.314f, 1.0f);
			
			// Animate color change
			var colorAnimation = CABasicAnimation.FromKeyPath(@"backgroundColor");
			colorAnimation.RemovedOnCompletion = false;
			colorAnimation.FillMode = CAFillMode.Forwards;
			
			// http://stackoverflow.com/questions/5821181/how-do-i-convert-a-cgcolor-into-an-nsobject-when-using-monotouch
			if(!string.IsNullOrEmpty(_disabled))
			{
				colorAnimation.From = Runtime.GetNSObject(greenColor.CGColor.Handle);
				colorAnimation.To = Runtime.GetNSObject(UIColor.FromWhiteAlpha(0.85f, 1.0f).CGColor.Handle);
			}
			else 
			{
				colorAnimation.From = Selected ? Runtime.GetNSObject(_tint.CGColor.Handle) : Runtime.GetNSObject(greenColor.CGColor.Handle);
				colorAnimation.To = Selected ? Runtime.GetNSObject(greenColor.CGColor.Handle) : Runtime.GetNSObject(_tint.CGColor.Handle);
			}
			
			_colorLayer.AddAnimation(colorAnimation, @"colorAnimation");
			
			// Animate layer scaling
			foreach(var layer in this.Layer.Sublayers)
			{
				layer.Frame = new RectangleF(layer.Frame.X - offset, layer.Frame.Y, layer.Frame.Size.Width + offset, layer.Frame.Size.Height);
			}
			
			CATransaction.Commit();

			this.SetNeedsDisplay();	
		}
		
		public void SetupLayers()
		{
			var bevelLayer = new CAGradientLayer();
			bevelLayer.Frame = new RectangleF(0, 0, Frame.Width, Frame.Height);
			bevelLayer.Colors = 
				new MonoTouch.CoreGraphics.CGColor[] { 
				UIColor.FromWhiteAlpha(0, 0.5f).CGColor,
				UIColor.White.CGColor,
			};
			bevelLayer.CornerRadius = 4.0f;
			bevelLayer.NeedsDisplayOnBoundsChange = true;
			
			_colorLayer = new CALayer(Layer);
			_colorLayer.Frame = new RectangleF(0, 1, Frame.Width, Frame.Height - 2);
			_colorLayer.BorderColor = new UIColor(0, 0, 0, 0.1f).CGColor;
			_colorLayer.BackgroundColor = _tint.CGColor;
			_colorLayer.BorderWidth = 1.0f;
			_colorLayer.CornerRadius = 4.0f;
			_colorLayer.NeedsDisplayOnBoundsChange = true;

			var colorGradient = new CAGradientLayer();
			colorGradient.Frame = new RectangleF(0, 1, Frame.Width, Frame.Height - 2);
			colorGradient.Colors = 
				new CGColor[] { 
				UIColor.FromWhiteAlpha(1, 0.1f).CGColor,
				UIColor.FromWhiteAlpha(0.2f, 0.1f).CGColor
			};
			colorGradient.Locations = 
				new NSNumber[] {
				new NSNumber(0.0f),
				new NSNumber(1.0f)
			};
			colorGradient.CornerRadius = 4.0f;
			colorGradient.NeedsDisplayOnBoundsChange = true;

			Layer.AddSublayer(bevelLayer);
			Layer.AddSublayer(_colorLayer);
			Layer.AddSublayer(colorGradient);
			BringSubviewToFront(this.TitleLabel);
		}		
		
		public override bool Selected
		{
			get 
			{
				return base.Selected;
			}
			set
			{
				base.Selected = value;
				this.Toggle();
			}
		}
		
		public void Disable(string text)
		{
			_disabled = text;
			Toggle();
		}
		
		public void Enable()
		{
			_disabled = null;
			Toggle();
		}
		
		private bool _confirmed;
		
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			if(string.IsNullOrEmpty(_disabled) && !_confirmed)
			{
				Darken();
				base.TouchesBegan(touches, evt);
			}
		}
				
		private UIButton _cancelOverlay;
				
		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			if(string.IsNullOrEmpty(_disabled) && !_confirmed)
			{
				var point = (touches.AnyObject as UITouch).LocationInView(this.Superview);
				if(!Frame.Contains(point))
				{
					Lighten();
					base.TouchesCancelled(touches, evt);
				}
				else if(Selected)
				{
					Lighten();
					_confirmed = true;
					_cancelOverlay.RemoveFromSuperview();
					_cancelOverlay = null;
					base.TouchesEnded(touches, evt);
				}
				else
				{
					Lighten();
					Selected = true;
					_cancelOverlay = UIButton.FromType(UIButtonType.Custom);
					_cancelOverlay.Frame = new RectangleF(0, 0, 1024, 1024);
					_cancelOverlay.AddTarget(delegate { Cancel(); }, UIControlEvent.TouchDown);
					Superview.AddSubview(_cancelOverlay);
					Superview.BringSubviewToFront(this);
				}
			}	
		}
		
		public void Cancel()
		{
			if(_cancelOverlay != null)
			{
				_cancelOverlay.RemoveFromSuperview();
				_cancelOverlay = null;
			}
			_confirmed = false;
			Selected = false;
		}
		
		public void SetAnchor(PointF anchor)
		{
			// Top-right point of the view (MUST BE SET LAST)
			Frame = new RectangleF(new PointF(anchor.X - Frame.Size.Width, anchor.Y), Frame.Size);
		}
		
		public override UIColor TintColor
		{
			get 
			{
				return _tint;
			}
			set
			{
				float h, s, b, a;
				value.GetHSBA(out h, out s, out b, out a);
				_tint = UIColor.FromHSB(h, s + 0.15f, b);
				_colorLayer.BackgroundColor = _tint.CGColor;
				SetNeedsDisplay();
			}
		}
		
		private void SetTintColor(UIColor color)
		{
			
		}
	
		private CALayer _darkenLayer;
		
		public void Darken()
		{
			_darkenLayer = new CALayer(Layer);
			_darkenLayer.Frame = new RectangleF(0, 0, Frame.Width, Frame.Height);
			_darkenLayer.BackgroundColor = UIColor.FromWhiteAlpha(0.0f, 0.2f).CGColor;
			_darkenLayer.CornerRadius = 4.0f;
			_darkenLayer.NeedsDisplayOnBoundsChange = true;
			Layer.AddSublayer(_darkenLayer);
		}
		
		public void Lighten()
		{	
			if(_darkenLayer != null)
			{
				_darkenLayer.RemoveFromSuperLayer();
				_darkenLayer = null;
			}
		}
	}
}

