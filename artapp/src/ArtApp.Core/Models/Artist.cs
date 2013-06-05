using System.Collections.Generic;

namespace ArtApp
{
    public class Artist 
    {
		public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Url { get; set; }
		public string Email { get; set; }
        public string Biography { get; set; }
		public string Image { get; set; }
		public string Location { get; set; }
		public string Title { get; set; }
		public string Twitter { get; set; }
		public bool UpdateBioImageFromTwitter { get; set; }
		
		public List<Series> Collections { get; set; }
		
		public Artist()
		{
			Collections = new List<Series>();	
		}
    }
	
}