using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Hammock.Extras.Serialization
{
    /// <summary>
    /// Resolves all property names to JSON conventional standard,
    /// i.e. JSON name "this_is_a_property" will map to the class property ThisIsAProperty.
    /// Also finds any property names suffixed with "Attribute" and converts them
    /// into @properties for XML serialization.
    /// </summary>
    public class JsonConventionResolver : DefaultContractResolver
    {
        public class ToStringComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return x.ToString().CompareTo(y.ToString());
            }
        }

        protected override IList<JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            return CreatePropertiesImpl(properties);
        }

        private static IList<JsonProperty> CreatePropertiesImpl(IList<JsonProperty> properties)
        {
            foreach (var property in properties)
            {
                property.PropertyName = PascalCaseToElement(property.PropertyName);
            }

            // @'s must come first
            var result = properties;

            ArrayList.Adapter((IList)result).Sort(new ToStringComparer());

            return result;
        }

        private static string PascalCaseToElement(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var attributeForSerialization = input.EndsWith("Attribute");
            if(attributeForSerialization)
            {
                input = input.Substring(0, input.LastIndexOf("Attribute"));
            }
            
            var result = new StringBuilder();
            result.Append(char.ToLowerInvariant(input[0]));
            
            for (var i = 1; i < input.Length; i++)
            {
                if (char.IsLower(input[i]))
                {
                    result.Append(input[i]);
                }
                else
                {
                    result.Append("_");
                    result.Append(char.ToLowerInvariant(input[i]));
                }
            }

            return attributeForSerialization? string.Concat("@", result) : result.ToString();
        }
    }
}