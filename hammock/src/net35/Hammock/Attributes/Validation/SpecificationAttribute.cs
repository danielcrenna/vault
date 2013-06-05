using System;
using Hammock.Extensions;
using Hammock.Specifications;
using Hammock.Validation;

namespace Hammock.Attributes.Validation
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class SpecificationAttribute : ValidationAttribute
    {
        public SpecificationAttribute(Type specificationType)
        {
            if (!specificationType.Implements(typeof (ISpecification)))
            {
                throw new ValidationException("You must provide a valid specification type.");
            }

            SpecificationType = specificationType as ISpecification;
        }

        public ISpecification SpecificationType { get; private set; }

        public override string TransformValue(System.Reflection.PropertyInfo property, object value)
        {
            if(SpecificationType != null && !value.Satisfies(SpecificationType))
            {
                var message =
                    "The value for '{0}' does not satisfy {1}."
                        .FormatWith(property.Name, SpecificationType);

                throw new ValidationException(message);
            }

            return base.TransformValue(property, value);
        }
    }
}