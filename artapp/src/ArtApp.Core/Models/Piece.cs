using System.Collections.Generic;
using System;

namespace ArtApp
{
    public class Piece
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public string Title { get; set; }
        public short Year { get; set; }
        public string Media { get; set; }
        public int Index { get; set; }
		public string Source { get; set; }
		public bool? Sold { get; set; }
		public int? LimitedPrints { get; set; }
    }
}
