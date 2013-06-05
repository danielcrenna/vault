using System;

namespace TweetSharp
{
    public interface ITweetable : ITwitterModel 
    {
        long Id { get; }
        string Text { get; }
        string TextAsHtml { get; }
        ITweeter Author { get; }
        DateTime CreatedDate { get; }
        TwitterEntities Entities { get; }
        string RawSource { get; set; }
    }

    public interface ITweeter : ITwitterModel
    {
        string ScreenName { get; }
        string ProfileImageUrl { get; }
    }
}