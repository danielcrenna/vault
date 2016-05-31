using System;
using System.Runtime.Serialization;

namespace Hammock.Web
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum HttpPostParameterType
    {
#if !SILVERLIGHT && !Smartphone && !ClientProfiles && !NET20 && !MonoTouch && !NETCF
        [EnumMember] Field,
        [EnumMember] File
#else
      Field,
        File
#endif
    }
}