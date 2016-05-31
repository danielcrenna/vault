using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hammock.Attributes;
using Hammock.Attributes.Validation;
using Hammock.Web;

namespace Hammock.Extensions
{
    internal static class WebExtensions
    {
        public static void ParseNamedAttributes<T>(this IWebQueryInfo info,
                                                   IEnumerable<PropertyInfo> properties,
                                                   IDictionary<string, string> transforms,
                                                   IDictionary<string, string> collection) 
            where T : Attribute, INamedAttribute
        {
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<T>(true);

                foreach (var attribute in attributes)
                {
                    var value = transforms.ContainsKey(property.Name)
                                    ? transforms[property.Name]
                                    : property.GetValue(info, null);

                    if (value == null)
                    {
                        continue;
                    }

                    var header = value.ToString();
                    if (!header.IsNullOrBlank())
                    {
                        collection.Add(attribute.Name, header);
                    }
                }
            }
        }

        public static string UriMinusQuery(string uri, WebParameterCollection parameters)
        {
            int queryPos = uri.IndexOf('?');
            if (queryPos < 0)
            {
                return uri;
            }
            var query = uri.Substring(queryPos + 1).ParseQueryString();
            foreach(var key in query.Keys)
            {
                parameters.Add(key, query[key].UrlDecode());
            }

            return uri.Substring(0, queryPos);
        }

        public static string ToBasicAuthorizationHeader(string username, string password)
        {
            var token = "{0}:{1}".FormatWith(username, password).GetBytes().ToBase64String();
            return "Basic {0}".FormatWith(token);
        }

        public static void ParseValidationAttributes(this IWebQueryInfo info,
                                                     IEnumerable<PropertyInfo> properties,
                                                     IDictionary<string, string> collection) 
        {
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<ValidationAttribute>(true);

                foreach (var attribute in attributes)
                {
                    // Support multiple transforms
                    var value = collection.ContainsKey(property.Name)
                                    ? collection[property.Name]
                                    : property.GetValue(info, null);
                    
                    var transformed = attribute.TransformValue(property, value);
                    if (transformed.IsNullOrBlank())
                    {
                        continue;
                    }

                    if(collection.ContainsKey(property.Name))
                    {
                        collection[property.Name] = transformed;
                    }
                    else
                    {
                        collection.Add(property.Name, transformed);
                    }
                }
            }
        }
    }
}