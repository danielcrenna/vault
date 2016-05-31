using System;

namespace Hammock.Attributes.Specialized
{
#if !SILVERLIGHT
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UserAgentAttribute : Attribute
    {

    }
}