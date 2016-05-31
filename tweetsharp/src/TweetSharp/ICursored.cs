namespace TweetSharp
{
    public interface ICursored
    {
        long? NextCursor { get; set; }
        long? PreviousCursor { get; set; }
    }
}