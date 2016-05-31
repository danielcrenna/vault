using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using DotLiquid;

namespace email.Templates
{
    internal static class HashExtensions
    {
        private static readonly IDictionary<Type, PropertyInfo[]> CachedStatics = new Dictionary<Type, PropertyInfo[]>();

        public static Hash FromDynamic(dynamic source)
        {
            var result = new Hash();
            if (source != null)
            {
                if (source is ExpandoObject)
                {
                    return Hash.FromDictionary((IDictionary<string, object>)source);
                }
                var type = (Type)source.GetType();
                if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
                {
                    throw new NotImplementedException("I don't feel like coding this up right now; use a static type?");
                }
                PropertyInfo[] properties;
                if (CachedStatics.ContainsKey(type))
                {
                    properties = CachedStatics[type];
                }
                else
                {
                    properties = type.GetProperties();
                    CachedStatics.Add(type, properties);
                }
                foreach (var property in properties)
                {
                    result[property.Name] = property.GetValue(source, null);
                }
            }
            return result;
        }
    }
}