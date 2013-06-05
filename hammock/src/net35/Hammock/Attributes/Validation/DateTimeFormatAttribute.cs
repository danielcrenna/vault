using System;
using Hammock.Extensions;

namespace Hammock.Attributes.Validation
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class DateTimeFormatAttribute : ValidationAttribute
    {
        public DateTimeFormatAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; private set; }

        public override string TransformValue(System.Reflection.PropertyInfo property, object value)
        {
#if !Smartphone && !NETCF
            DateTime result;
            return DateTime.TryParse(value.ToString(), out result) && !Format.IsNullOrBlank()
                       ? result.ToString(Format)
                       : base.TransformValue(property, value);
#else
          try
            {
                var result = DateTime.Parse(value.ToString());
                return Format.IsNullOrBlank()
                           ? result.ToString(Format)
                           : base.TransformValue(property, value);
            }
            catch (Exception)
            {
                return base.TransformValue(property, value);
            }
#endif
        }
    }
}