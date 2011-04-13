using System;
using System.Collections.Generic;
using System.Linq;
using metrics.Core;
using Newtonsoft.Json;

namespace metrics.Serialization
{
    internal class MetricItem
    {
        public string Name { get; set; }
        public IMetric Metric { get; set; }
    }

    /// <summary>
    /// Properly serializes a metrics hash
    /// </summary>
    internal class MetricsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is IDictionary<MetricName, IMetric>))
            {
                return;
            }

            var collection = (IDictionary<MetricName, IMetric>)value;
            var container = new List<MetricItem>(collection.Count);
            container.AddRange(collection.Select(item => new MetricItem {Name = item.Key.Name, Metric = item.Value}));
            var serialized = Serializer.Serialize(container);

            writer.WriteRawValue(serialized);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof (IDictionary<MetricName, IMetric>).IsAssignableFrom(objectType);
        }
    }
}
