// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.CoreLocation;
using System.Globalization;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.ObjCRuntime;
using System.Net.NetworkInformation;

namespace ArtApp
{
	public static class Util
	{
		static object _network = new object();
		static int active;
		
		public static void PushNetworkActive()
		{
			lock (_network)
			{
				active++;
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
			}
		}
		
		public static void PopNetworkActive()
		{
			lock (_network)
			{
				active--;
				if (active == 0)
				{
					UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				}	
			}
		}
		
		public static void WithNetworkActivity(Action closure)
		{
			PushNetworkActive();
			closure();
			PopNetworkActive();
		}
		
		public static bool IsPad()
		{
			return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
		}
		
		public static bool IsPhone()
		{
			return UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad;
		}
		
		public static bool IsRetina()
		{
			if(!UIDevice.CurrentDevice.CheckSystemVersion(4, 0)) // Protect iOS 3
			{
				return false;
			}			
			return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone && UIScreen.MainScreen.Scale == 2.0;	
		}
		
		public static bool IsPortrait()
		{
			var sbOrientation = UIApplication.SharedApplication.StatusBarOrientation;
			switch(sbOrientation)
			{
				case UIInterfaceOrientation.Portrait:
				case UIInterfaceOrientation.PortraitUpsideDown:
					return true;
				default:
					return false;
			}		
		}
		
		public static bool IsLandscape()
		{
			switch(UIApplication.SharedApplication.StatusBarOrientation)
			{
				case UIInterfaceOrientation.LandscapeLeft:
				case UIInterfaceOrientation.LandscapeRight:
					return true;
				default:
					return false;
			}	
		}
		
		public static float PixelScreenWidth
		{
			get
			{
				return Util.IsPad() ? Util.IsLandscape() ? 1024 : 768 :
					   Util.IsLandscape() ? Util.IsRetina() ? 960 : 480 : 
					   Util.IsRetina() ? 640 : 320;	
			}
		}
	}
}
