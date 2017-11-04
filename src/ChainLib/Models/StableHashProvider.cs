using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FastMember;
using Crypto.Shim;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ChainLib.Models
{
    public class StableHashProvider : IHashProvider
    {
        private readonly HashAlgorithm _algorithm;
        private readonly JsonSerializerSettings _settings;

        public StableHashProvider() : this(SHA256.Create()) { }

        public StableHashProvider(HashAlgorithm algorithm)
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

	        var json = JsonConvert.SerializeObject(instance, _settings);
	        return ComputeHashBytes(json);
        } 

        public byte[] ComputeHashBytes(string any)
        {
            var buffer = Encoding.UTF8.GetBytes(any);
            var hash = _algorithm.ComputeHash(buffer);
            return hash;
        }

        public string ComputeHashString(object instance)
        {
            return ComputeHashBytes(instance).ToHex();
        }

        public string ComputeHashString(string any)
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

				// treat computed properties as if they weren't there
	            if (Attribute.IsDefined(member, typeof(ComputedAttribute)))
		            property.ShouldSerialize = instance => false;

				// treat null/empty collection properties as if they weren't there
				else if (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    property.ShouldSerialize = instance => IsEmptyEnumerable(member, instance);

                return property;
            }

	        private static bool IsEmptyEnumerable(MemberInfo member, object instance)
	        {
		        var accessor = TypeAccessor.Create(instance.GetType());
				var property = accessor[instance, member.Name];
		        return !(property is IEnumerable enumerable) || enumerable.GetEnumerator().MoveNext();
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