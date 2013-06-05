using System;

namespace Hammock.Attributes.Specialized
{
#if !SILVERLIGHT
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ParameterAttribute : Attribute, INamedAttribute
    {
        public ParameterAttribute(string name)
        {
            Name = name;
        }

        #region INamedAttribute Members

        public virtual string Name { get; private set; }

        #endregion
    }
}