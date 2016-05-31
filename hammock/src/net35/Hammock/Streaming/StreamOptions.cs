using System;

namespace Hammock.Streaming
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class StreamOptions
    {
        public virtual TimeSpan? Duration { get; set; }
        public virtual int? ResultsPerCallback { get; set; }
    }
}
