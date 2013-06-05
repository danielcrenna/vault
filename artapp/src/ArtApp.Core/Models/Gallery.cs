using System.Collections.Generic;

namespace ArtApp
{
	public class Gallery
	{
		public string Name { get; set; }
		
		public List<Artist> Artists { get; set; }
		
		public Gallery()
		{
			Artists = new List<Artist>();	
		}
	}
}