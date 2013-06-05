using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace ArtApp
{
	public class AppManifest
	{
		private static Gallery _gallery;
	
		public static IList<Artist> Artists
		{
			get
			{
				if(_gallery == null)
				{
					Load();
				}
				
				return _gallery.Artists;	
			}
		}
		
		public static Gallery Gallery
		{
			get
			{
				if(_gallery == null)
				{
					Load();	
				}
				
				return _gallery;
			}
		}
		
		public static Artist Current
		{
			get
			{
				if(_gallery == null)
				{
					Load();
				}
				
				return _gallery.Artists[0];
			}
		}

		static void Load()
		{
			var json = File.ReadAllText("manifest.json");
			_gallery = JsonParser.Deserialize<Gallery>(json);
		}
	}
}

