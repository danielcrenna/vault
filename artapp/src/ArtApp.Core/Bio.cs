using System;
using MonoTouch.UIKit;

namespace ArtApp
{
	public class Bio
	{
		public string Name { get; private set; }
		public string Url { get; private set; }
		public string Location { get; private set; }	
		public string Title { get; private set; }
		public string Twitter { get; private set; }
		public bool UpdateImage { get; private set; }
		
		public UIImage Image { get; set; }
		
		public Bio()
		{
			var manifest = AppManifest.Current;
			
			Name = string.Concat(manifest.FirstName, " ", manifest.LastName);
			Url = manifest.Url;
			Location = manifest.Location;
			Title = manifest.Title;
			Twitter = manifest.Twitter;
			UpdateImage = false;
		}
	}
}

