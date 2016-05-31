using System;

namespace Hammock.Attributes.Specialized
{
#if !SILVERLIGHT
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class HeaderAttribute : Attribute, INamedAttribute
    {
        public HeaderAttribute(string name)
        {
            Name = name;
        }

        #region INamedAttribute Members

        public virtual string Name { get; private set; }

        #endregion
    }
}