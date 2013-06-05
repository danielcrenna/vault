using System;
using System.Collections.Generic;
using System.IO;
using Hammock;
using Hammock.Serialization;
using Newtonsoft.Json;
using TweetSharp.Serialization;

namespace TweetSharp
{
    public abstract class SerializerBase : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;
        
        protected SerializerBase()
            : this(new JsonSerializerSettings
                       {
                           MissingMemberHandling = MissingMemberHandling.Ignore,
                           NullValueHandling = NullValueHandling.Ignore,
                           DefaultValueHandling = DefaultValueHandling.Include,
                           ContractResolver = new JsonConventionResolver(),
                           Converters = new List<JsonConverter>
                                            {
                                                new TwitterDateTimeConverter(),
                                                new TwitterWonkyBooleanConverter(),
                                                new TwitterGeoConverter(),
                                                new TwitterRateLimitResourceConverter()
                                            }
                       })
        {
            
        }

        protected SerializerBase(JsonSerializerSettings settings)
        {
            _serializer = new Newtonsoft.Json.JsonSerializer
                              {
                                  ConstructorHandling = settings.ConstructorHandling,
                                  ContractResolver = settings.ContractResolver,
                                  ObjectCreationHandling = settings.ObjectCreationHandling,
                                  MissingMemberHandling = settings.MissingMemberHandling,
                                  DefaultValueHandling = settings.DefaultValueHandling,
                                  NullValueHandling = settings.NullValueHandling
                              };

            foreach (var converter in settings.Converters)
            {
                _serializer.Converters.Add(converter);
            }
        }

        public abstract T Deserialize<T>(RestResponseBase response);
       
        public abstract object Deserialize(RestResponseBase response, Type type);
        
        public virtual object DeserializeJson(string content, Type type)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize(jsonTextReader, type);
                }
            }
        }

        public virtual T DeserializeJson<T>(string content)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
        
        public virtual string SerializeJson(object instance, Type type)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.Formatting = Formatting.Indented;
                    jsonTextWriter.QuoteChar = '"';

                    _serializer.Serialize(jsonTextWriter, instance);

                    var result = stringWriter.ToString();
                    return result;
                }
            }
        }

        public abstract string Serialize(object instance, Type type);

        public abstract string ContentType { get; }

#if NET40
        public dynamic DeserializeDynamic(RestResponseBase response)
        {
            throw new NotSupportedException();
        }
#endif
    }
}