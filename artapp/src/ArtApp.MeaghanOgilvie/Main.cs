using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.ComponentModel;
using System.IO;

namespace ArtApp
{
	public class Application
	{
		private static void Main (string[] args)
		{
			// Necessary to avoid linking out the AppDelegate
			Console.WriteLine(typeof(AppDelegate).Name);			

			FixMonoTouchErrors();
			
			UIApplication.Main(args, null, "AppDelegate");
		}
		
		public static void FixMonoTouchErrors()
	    {
	        new GuidConverter();
	        new StringConverter();
	        new DateTimeConverter();
	        new CharConverter();
	        new Int16Converter();
	        new Int32Converter();
	        new Int64Converter();
	        new DecimalConverter();
	        new NullableConverter(typeof(Int16?));
	        new NullableConverter(typeof(Int32?));
	        new NullableConverter(typeof(Int64?));
	        new NullableConverter(typeof(decimal?));
	        new NullableConverter(typeof(DateTime?));
			new NullableConverter(typeof(bool?));
			new BooleanConverter();
	    }
	}
}

