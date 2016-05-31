using System.Collections.Generic;
using ServiceStack.Text;

namespace linger
{
    public static class JsonSerializer
    {
        static JsonSerializer()
        {
            JsConfig.PropertyConvention = JsonPropertyConvention.Lenient;
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.ExcludeTypeInfo = true;
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
        }

        public static string Serialize<T>(T value)
        {
            var json = ServiceStack.Text.JsonSerializer.SerializeToString(value);
            return json;
        }

        public static string SerializeJobs(IEnumerable<ScheduledJob> value)
        {
            var json = ServiceStack.Text.JsonSerializer.SerializeToString(new { Jobs = value });
            return json;
        }
    }
}