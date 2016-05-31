using ServiceStack.Text;

namespace ab
{
    internal static class Serializer
    {
        static Serializer()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.PropertyConvention = JsonPropertyConvention.Lenient;
        }
        public static string ToJson(dynamic model)
        {
            return JsonSerializer.SerializeToString(model);
        }
    }
}