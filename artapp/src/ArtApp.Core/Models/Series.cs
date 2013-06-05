using System.Collections.Generic;
using System.Linq;

namespace ArtApp
{
	public class Series
	{
		public string Title { get; set; }		
		public bool? New { get; set; }		
		public string Description { get; set; }
		
		public string Years
		{
			get
			{
				var years = Pieces.Select(p => p.Year.ToString()).Distinct().OrderBy(y => y);				
				return years.Count() < 3 ? string.Join(", ", years.ToArray()) : years.First() + " - " + years.Last();
			}
		}
		
		public float TallestPieceHeight
		{
			get
			{
				return Pieces.Max(p => p.Height);		
			}
		}
		
		public float WidestPieceWidth
		{
			get
			{
				return Pieces.Max(p => p.Width);		
			}
		}
		
		private List<Piece> _pieces;
		
		public List<Piece> Pieces
		{ 
			get 
			{
				return _pieces.OrderBy (p => p, new PieceComparer ()).ToList();
			}
			set 
			{
				_pieces = value;
			}
		}

		public Series()
		{
			Pieces = new List<Piece> ();
		}
        
		private class PieceComparer : IComparer<Piece>
		{
			public int Compare (Piece x, Piece y)
			{
				return x.Index.CompareTo (y.Index);
			}
		}
		
		public void Add(Piece piece)
		{
			_pieces.Add(piece);	
		}
	}
}