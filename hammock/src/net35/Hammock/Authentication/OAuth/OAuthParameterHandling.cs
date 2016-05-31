using System;
using System.Runtime.Serialization;

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthParameterHandling
    {
#if !SILVERLIGHT && !Smartphone && !ClientProfiles && !NET20 && !MonoTouch && !NETCF
        [EnumMember] HttpAuthorizationHeader,
        [EnumMember] UrlOrPostParameters
#else
      HttpAuthorizationHeader,
        UrlOrPostParameters
#endif
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthSignatureTreatment
    {
#if !SILVERLIGHT && !Smartphone && !ClientProfiles && !NET20 && !MonoTouch && !NETCF
        [EnumMember]
        Escaped,
        [EnumMember]
        Unescaped
#else
      Escaped,
        Unescaped
#endif
    }
}