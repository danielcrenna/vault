using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Hammock;

namespace TweetSharp
{
    internal class JsonSerializer : SerializerBase
    {
        public override T Deserialize<T>(RestResponseBase response) 
        {
            if (response == null)
            {
                return default(T);
            }
            if ((int)response.StatusCode >= 500)
            {
                return default(T);
            }

            var content = response.Content;

            if (content.Equals("END STREAMING"))
            {
                return (T)(ITwitterModel)new TwitterUserStreamEnd();
            }

            return (T)DeserializeContent(content, typeof(T));
        }

        public override object DeserializeJson(string content, Type type)
        {
            if (type == typeof (TwitterError))
            {
                return DeserializeContent(content, type);
            }
            else
            {
                return base.DeserializeJson(content, type);
            }
        }

        public override T DeserializeJson<T>(string content)
        {
            return (T)DeserializeContent(content, typeof(T));
        }

        internal object DeserializeContent(string content, Type type)
        {
            if (string.IsNullOrEmpty(content) || content.Trim().Length == 0)
            {
                return null;
            }
            if (type == typeof (TwitterError))
            {
                // {"errors":[{"message":"Bad Authentication data","code":215}]}
                content = content.Trim('\n');
                if (content.StartsWith("{\"errors\":["))
                {
                    var errors = (JArray)JObject.Parse(content)["errors"];
                    if (errors != null)
                    {
                        var result = new TwitterError { RawSource = content };
                        var error = errors.First();
                        result.Message = error["message"].ToString();
                        result.Code = int.Parse(error["code"].ToString());
                        return (ITwitterModel)result;
                    }
                }
                else
                {
                    var unknown = new TwitterError() { RawSource = content };
                    return unknown;
                }
            }

            if(type == typeof(TwitterTrends))
            {
                return DeserializeTrends(content);
            }

            if(type == typeof(TwitterLocalTrends))
            {
                var instance = JArray.Parse(content);
                var inner = instance.First.ToString();
                return DeserializeSingle(inner, type);
            }

            if (type == typeof(TwitterStreamArtifact))
            {
                content = content.Trim('\n');
                if (content.StartsWith("{\"friends\":["))
                {
                    var friends = (JArray) JObject.Parse(content)["friends"];
                    if (friends != null)
                    {
                        var result = new TwitterUserStreamFriends {RawSource = content};
                        var ids = friends.Select(token => token.Value<long>()).ToList();
                        result.Ids = ids;
                        return (ITwitterModel) result;
                    }
                }
                // {"delete":{"status":{"user_id_str":"14363427","id_str":"45331017418014721","id":45331017418014721,"user_id":14363427}}}
                else if (content.StartsWith("{\"delete\":{\"status\":"))
                {
                    var deleted = JObject.Parse(content)["delete"]["status"];
                    if (deleted != null)
                    {
                        var result = new TwitterUserStreamDeleteStatus
                                         {
                                             RawSource = content,
                                             StatusId = deleted["id"].Value<long>(),
                                             UserId = deleted["user_id"].Value<int>()
                                         };
                        return (ITwitterModel) result;
                    }
                }
                else if (content.StartsWith("{\"delete\":{\"direct_message\":"))
                {
                    var deleted = JObject.Parse(content)["delete"]["direct_message"];
                    if (deleted != null)
                    {
                        var result = new TwitterUserStreamDeleteDirectMessage
                        {
                            RawSource = content,
                            DirectMessageId = deleted["id"].Value<long>(),
                            UserId = deleted["user_id"].Value<int>()
                        };
                        return (ITwitterModel)result;
                    }
                }
                else
                {
                    var artifact = JObject.Parse(content);
                    if (artifact["target_object"] != null)
                    {
                        return DeserializeUserStreamEvent(content);
                    }

                    if (artifact["user"] != null)
                    {
                        var tweet = DeserializeSingle(content, typeof(TwitterStatus)) as TwitterStatus;
                        var @event = new TwitterUserStreamStatus {Status = tweet, RawSource = content};
                        return (ITwitterModel) @event;
                    }

                    if (artifact["direct_message"] != null)
                    {
                        var json = artifact["direct_message"].ToString();
                        var dm = DeserializeSingle(json, typeof(TwitterDirectMessage)) as TwitterDirectMessage;
                        var @event = new TwitterUserStreamDirectMessage {DirectMessage = dm, RawSource = json};
                        return (ITwitterModel) @event;
                    }

                    var unknown = new TwitterStreamArtifact {RawSource = content};
                    return (ITwitterModel) unknown;
                }
            }

            if (type == typeof(IEnumerable<TwitterTrends>))
            {
                return DeserializeTrends(content);
            }

            var wantsCollection = typeof(IEnumerable).IsAssignableFrom(type);
            
            var deserialized = wantsCollection
                                   ? DeserializeCollection(content, type)
                                   : DeserializeSingle(content, type);
           
            return deserialized;
        }

        private object DeserializeUserStreamEvent(string content)
        {
            var @event = DeserializeSingle(content, typeof(TwitterUserStreamEventBase)) as TwitterUserStreamEventBase;

            var target = JObject.Parse(content);
            
            var result = new TwitterUserStreamEvent(@event);

            var targetObject = target["target_object"];

            var json = targetObject.ToString();

            if(targetObject["recipient_screen_name"] != null)
            {
                result.TargetObject = DeserializeSingle(json, typeof(TwitterDirectMessage)) as TwitterDirectMessage;
            }
            else if(targetObject["slug"] != null)
            {
                result.TargetObject = DeserializeSingle(json, typeof(TwitterList)) as TwitterList;
            }
            else
            {
                result.TargetObject = DeserializeSingle(json, typeof(TwitterStatus)) as TwitterStatus;
            }

            return (ITwitterModel)result;
        }

        private object DeserializeTrends(string content)
        {
            // --> Current model is not quite right
            // "[{\"trends\":[{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=%23WaysToMakeMeMad\",\"name\":\"#WaysToMakeMeMad\",\"events\":null,\"query\":\"%23WaysToMakeMeMad\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=%23ThoughtsInClass\",\"name\":\"#ThoughtsInClass\",\"events\":null,\"query\":\"%23ThoughtsInClass\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=%23ScarlettHeightsProblems\",\"name\":\"#ScarlettHeightsProblems\",\"events\":null,\"query\":\"%23ScarlettHeightsProblems\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=%22Isaiah%20Thomas%22\",\"name\":\"Isaiah Thomas\",\"events\":null,\"query\":\"%22Isaiah%20Thomas%22\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=OCAP\",\"name\":\"OCAP\",\"events\":null,\"query\":\"OCAP\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=%22Henrik%20Sedin%22\",\"name\":\"Henrik Sedin\",\"events\":null,\"query\":\"%22Henrik%20Sedin%22\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=Russia\",\"name\":\"Russia\",\"events\":null,\"query\":\"Russia\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=%22Kenneth%20Faried%22\",\"name\":\"Kenneth Faried\",\"events\":null,\"query\":\"%22Kenneth%20Faried%22\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=%22Kyrie%20Irving%22\",\"name\":\"Kyrie Irving\",\"events\":null,\"query\":\"%22Kyrie%20Irving%22\"},{\"promoted_content\":null,\"url\":\"http:\\/\\/twitter.com\\/search?q=%23iWontReplyIf\",\"name\":\"#iWontReplyIf\",\"events\":null,\"query\":\"%23iWontReplyIf\"}],\"locations\":[{\"name\":\"Toronto\",\"woeid\":4118}],\"as_of\":\"2013-02-16T05:58:55Z\",\"created_at\":\"2013-02-16T05:56:01Z\"}]"
            var collection = JArray.Parse(content);
            var result = new TwitterTrends { RawSource = content };
            foreach (var item in collection)
            {
                var inner = item["trends"];
                if (inner != null)
                {
                    var trends = (DeserializeCollection(inner.ToString(), typeof (IEnumerable<TwitterTrend>)) as IEnumerable<TwitterTrend>).ToList();
                    result.Trends.AddRange(trends);
                }    
            }
            return result;
        }

        private object DeserializeSingle(string content, Type type)
        {
            var deserialized = DeserializeJson(content, type);
            if (typeof(ITwitterModel).IsAssignableFrom(type))
            {
                ((ITwitterModel)deserialized).RawSource = content;
            }

            // Provide a RawSource for embedded tweets
            if (type == typeof (TwitterSearchResult) && content.StartsWith("{\"statuses\":[{"))
            {
                var array = (JArray) JObject.Parse(content)["statuses"];
                var result = (TwitterSearchResult)deserialized;
                var collection = result.Statuses;
                for (var i = 0; i < collection.Count(); i++)
                {
                    var status = collection.Skip(i).Take(1).Single();
                    status.RawSource = array[i].ToString();
                }
            }
            return deserialized;
        }

        private object DeserializeCollection(string content, Type type)
        {
            if (type == typeof(byte[]))
            {
                var binary = (IEnumerable) Encoding.UTF8.GetBytes(content);
                var deserialized = binary;
                return deserialized;
            }

            IList collection;
            var collectionType = ConstructCollection(out collection, type);

            Type cursor = null;
            var generics = type.GetGenericArguments();
            if(generics.Length > 0)
            {
                var inner = generics[0];
                cursor = typeof(TwitterCursorList<>).MakeGenericType(inner);    
            }

            try
            {
                JArray array = null;
                JObject instance = null;

                instance = ParseInnerContent("trends", content, type, cursor, instance, ref array);
                instance = ParseInnerContent("users", content, type, cursor, instance, ref array);
                instance = ParseInnerContent("lists", content, type, cursor, instance, ref array);
                instance = ParseInnerContent("ids", content, type, cursor, instance, ref array);
                instance = ParseInnerContent("result", content, type, cursor, instance, ref array);

                if(array == null)
                {
                    array = JArray.Parse(content);
                }

                var model = typeof (ITwitterModel).IsAssignableFrom(collectionType);
                var items = array.Select(item => item.ToString());
                if(model)
                {
                    foreach (var c in items)
                    {
                        AddDeserializedItem(c, collectionType, collection);
                    }    
                }
                else
                {
                    foreach (var c in items)
                    {
                        AddDeserializedItemWithoutRawSource(c, collectionType, collection);
                    }
                }
                
                var deserialized = type == cursor
                                       ? BindDeserializedItemsIntoCursorCollection(collection, cursor, instance)
                                       : collection;

                return deserialized;
            }
            catch(JsonReaderException readerException) // <-- Likely 502 
            {
                TraceException(readerException, collectionType, content);

                AddEmptyItem(content, collectionType, collection);
                
                var deserialized = collection;

                return deserialized;
            }
            catch (Exception ex) // <-- Likely entity mismatch (error)
            {
                TraceException(ex, collectionType, content);

                AddDeserializedItem(content, collectionType, collection);
                
                var deserialized = collection;

                return deserialized;
            }
        }

        private static JObject ParseInnerContent(string entity, string content, Type outer,  Type cursor, JObject instance, ref JArray array)
        {
            if (!content.Contains(string.Format("\"{0}\"", entity)))
            {
                return instance;
            }
            instance = JObject.Parse(content);
            array = ParseFromCursorListOrObject(entity, content, outer, cursor, instance);
            return instance;
        }

        private static JArray ParseFromCursorListOrObject(string type, string content, Type outer, Type cursor, JObject instance)
        {
            JArray array;
            if (cursor != null && outer == cursor)
            {
                array = instance != null
                            ? ((JArray)instance[type] ?? JArray.Parse(content))
                            : JArray.Parse(content);
            }
            else
            {
                // [DC]: We need to go one level deeper than "result" in the case of places
                if(type.Equals("result"))
                {
                    instance = JObject.Parse(content);
                    var inner = instance["result"]["places"].ToString();
                    array = JArray.Parse(inner);
                }
                else
                {
                    array = JArray.Parse(content);
                }
            }
            return array;
        }

        private static void TraceException(Exception ex, Type type, string content)
        {
#if !SILVERLIGHT
            Trace.TraceError(string.Concat("TweetSharp: Could not parse content into 'IEnumerable<", type.Name, ">' : '", content));
            Trace.TraceError(ex.Message);
            Trace.TraceError(ex.StackTrace);
#endif
        }

        private static object BindDeserializedItemsIntoCursorCollection(IEnumerable collection, Type cursor, JObject instance)
        {
#if !SILVERLIGHT
            var list = Activator.CreateInstance(
                    cursor, 0, null, new object[] { collection }, CultureInfo.InvariantCulture
                    );
#else
            var list = Activator.CreateInstance(cursor, new object[] {collection});
#endif
            if(instance != null)
            {
                var next = instance["next_cursor"];
                var previous = instance["previous_cursor"];
                ((ICursored)list).NextCursor = (long?)next;
                ((ICursored)list).PreviousCursor = (long?)previous;
            }

            var deserialized = list;
            return deserialized;
        }

        private void AddDeserializedItem(string c, Type type, IList collection)
        {
            var d = Deserialize(c, type);
            ((ITwitterModel)d).RawSource = c;  
            collection.Add(d);
        }

        private void AddDeserializedItemWithoutRawSource(string c, Type type, IList collection)
        {
            var d = Deserialize(c, type);
            collection.Add(d);
        }

        private static void AddEmptyItem(string c, Type type, IList collection)
        {
            var d = Activator.CreateInstance(type);
            ((ITwitterModel)d).RawSource = c;
            collection.Add(d);
        }

        private static Type ConstructCollection(out IList collection, Type type)
        {
            type = type.GetGenericArguments()[0];
            var collectionType = typeof(List<>).MakeGenericType(type);
            collection = (IList)Activator.CreateInstance(collectionType);
            return type;
        }

        public override object Deserialize(RestResponseBase response, Type type)
        {
            return DeserializeJson(response.Content, type);
        }

        public object Deserialize(string content, Type type)
        {
            return DeserializeJson(content, type);
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