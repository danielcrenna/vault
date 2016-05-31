using System;
using Newtonsoft.Json;

namespace TweetSharp
{
#if !SILVERLIGHT && !WINRT
    [Serializable]
#endif
    public enum TwitterSearchResultType
    {
        Mixed,
        Recent,
        Popular
    }

#if !SILVERLIGHT && !WINRT
    [Serializable]
#endif
    public enum TwitterProfileImageSize
    {
        Bigger,
        Normal,
        Mini
    }

#if !SILVERLIGHT && !WINRT
    [Serializable]
#endif
    public enum TwitterEntityType
    {
        HashTag,
        Mention,
        Url,
        Media
    }

#if !SILVERLIGHT && !WINRT
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

#if !SILVERLIGHT && !WINRT
    [Serializable]
#endif
    public enum TwitterMediaType
    {
        Photo,
				Video,
				AnimatedGif
    }


#if !SILVERLIGHT && !WINRT
    [Serializable]
#endif
    public enum TwitterListMode
    {
        Public,
        Private
    }
}