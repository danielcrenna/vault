using System;

namespace ArtApp
{
	public class PagedViewEventArgs : EventArgs
	{
		public int Page { get; private set; }
		
		public PagedViewEventArgs(int page)
		{
			Page = page;	
		}
	}
	
}

