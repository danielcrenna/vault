using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hammock.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hammock.Extras.Serialization
{
    public class JsonSerializer : SerializerBase
    {
        public JsonSerializer()
        {
            
        }

        public JsonSerializer(JsonSerializerSettings settings) : base(settings)
        {
            
        }
        
#if NET40
        public override dynamic DeserializeDynamic<T>(RestResponse<T> response)
        {
            var result = JsonParser.Deserialize(response.Content);
            return result;
        }
#endif

        public override T Deserialize<T>(RestResponse<T> response)
        {
            var token = JToken.FromObject(response.Content);

            return token.Type == JTokenType.Array
                       ? DeserializeCollection<T>(token)
                       : DeserializeSingle<T>(response.Content);
        }

        private T DeserializeSingle<T>(string content)
        {
            var deserialized = DeserializeJson<T>(content);
            return deserialized;
        }

        private T DeserializeCollection<T>(IEnumerable<JToken> array)
        {
            IList collection;
            var type = ConstructCollection<T>(out collection);

            try
            {
                var items = array.Select(item => item.ToString());
                foreach (var d in items.Select(item => DeserializeJson(item, type)))
                {
                    collection.Add(d);
                }
                var deserialized = (T)collection;
                return deserialized;
            }
            catch (JsonReaderException) // <-- Likely unexpected content type 
            {
                var deserialized = (T)collection;
                return deserialized;
            }
            catch (Exception) // <-- Likely entity mismatch (error)
            {
                var deserialized = (T)collection;
                return deserialized;
            }
        }

        private static Type ConstructCollection<T>(out IList collection)
        {
            var args = typeof(T).GetGenericArguments();
            var type = args.Length == 0 ? typeof (T) : args[0];
            var collectionType = typeof(List<>).MakeGenericType(type);
            collection = (IList)Activator.CreateInstance(collectionType);
            return type;
        }
        
        public override object Deserialize(RestResponse response, Type type)
        {
            return DeserializeJson(response.Content, type);
        }

        public override string Serialize(object instance, Type type)
        {
            return SerializeJson(instance, type);
        }

        public override string ContentType
        {
            get { return "application/json"; }
        }
    }
}