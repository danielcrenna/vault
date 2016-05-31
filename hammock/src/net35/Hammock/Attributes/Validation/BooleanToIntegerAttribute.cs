using System;
using System.Reflection;

namespace Hammock.Attributes.Validation
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class BooleanToIntegerAttribute : ValidationAttribute
    {
        public override string TransformValue(PropertyInfo property, object value)
      {
#if !Smartphone && !NETCF
            bool result;
            return bool.TryParse(value.ToString(), out result)
                       ? result ? "1" : "0"
                       : base.TransformValue(property, value);
#else
        try
            {
                var result = bool.Parse(value.ToString());
                return result ? "1" : "0";
            }
            catch (Exception)
            {
                return base.TransformValue(property, value);
            }
#endif
        }
    }
}