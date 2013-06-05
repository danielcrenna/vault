using System;

namespace TweetSharp
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum TwitterSearchResultType
    {
        Mixed,
        Recent,
        Popular
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public enum TwitterProfileImageSize
    {
        Bigger,
        Normal,
        Mini
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public enum TwitterEntityType
    {
        HashTag,
        Mention,
        Url,
        Media
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public enum TwitterPlaceType
    {
        City,
        Neighborhood,
        Country,
        Admin,
        POI
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public enum TwitterMediaType
    {
        Photo
    }


#if !SILVERLIGHT
    [Serializable]
#endif
    public enum TwitterListMode
    {
        Public,
        Private
    }
}