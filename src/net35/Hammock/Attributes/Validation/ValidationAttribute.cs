using System;
using System.Reflection;

namespace Hammock.Attributes.Validation
{
#if !SILVERLIGHT
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ValidationAttribute : Attribute, IValidatingAttribute
    {
        public virtual string TransformValue(PropertyInfo property, object value)
        {
            return value.ToString();
        }
    }
}


