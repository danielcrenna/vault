using System;

namespace cohort
{
    public class BrokenLink
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public string Referer { get; set; }
        public int Count { get; set; }
        public DateTime LastOccurrence { get; set; }

        public BrokenLink()
        {
            Count = 1;
            LastOccurrence = DateTime.Now;
        }
    }
}