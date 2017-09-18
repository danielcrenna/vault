using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FastMember;
using NaiveCoin.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NaiveCoin.Models
{
    public class StableObjectHashProvider : IObjectHashProvider
    {
        private readonly HashAlgorithm _algorithm;
        private readonly JsonSerializerSettings _settings;

        public StableObjectHashProvider() : this(SHA256.Create()) { }

        public StableObjectHashProvider(HashAlgorithm algorithm)
        {
            _algorithm = algorithm;
            _settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new StableHashResolver()
            };
        }

        public byte[] ComputeHashBytes(object instance)
        {
            if (instance is string s)
                return ComputeHashBytes(s);

            return ComputeHashBytes(JsonConvert.SerializeObject(instance, _settings));
        }

        public byte[] ComputeHashBytes(string any)
        {
            var buffer = Encoding.UTF8.GetBytes(any);
            var hash = _algorithm.ComputeHash(buffer);
            return hash;
        }

        public string ComputeHash(object instance)
        {
            return ComputeHashBytes(instance).ToHex();
        }

        public string ComputeHash(string any)
        {
            return ComputeHashBytes(any).ToHex();
        }

        private static readonly IDictionary<Type, IList<JsonProperty>> Map =
            new Dictionary<Type, IList<JsonProperty>>();

        private class StableHashResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                var accessor = TypeAccessor.Create(property.PropertyType);
                if (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    property.ShouldSerialize = instance => !(accessor[instance, member.Name] is IEnumerable enumerable) || enumerable.GetEnumerator().MoveNext();
                return property;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                lock (Map)
                {
                    if (!Map.TryGetValue(type, out var properties))
                    {
                        Map.Add(type, properties = base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList());
                    }
                    return properties;
                }
                
            }
        }
    }
}