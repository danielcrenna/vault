using System;
using Hammock.Extensions;
using Hammock.Validation;

namespace Hammock.Attributes.Validation
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class RequiredAttribute : ValidationAttribute
    {
        public override string TransformValue(System.Reflection.PropertyInfo property, object value)
        {
            if(value == null)
            {
                var message = "The property {0} is required, but was null.".FormatWith(property.Name);
                throw new ValidationException(message);
            }
            return base.TransformValue(property, value);
        }
    }
}