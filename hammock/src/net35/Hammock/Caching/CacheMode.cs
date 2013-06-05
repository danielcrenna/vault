using System;
using System.Runtime.Serialization;

namespace Hammock.Caching
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum CacheMode
    {
#if !SILVERLIGHT && !Smartphone && !ClientProfiles && !NET20 && !MonoTouch && !NETCF
        [EnumMember] NoExpiration,
        [EnumMember] AbsoluteExpiration,
        [EnumMember] SlidingExpiration
#else
      NoExpiration,
        AbsoluteExpiration,
        SlidingExpiration
#endif
    }
}