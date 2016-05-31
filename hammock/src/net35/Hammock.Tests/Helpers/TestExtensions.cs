using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;

namespace RestCore.Tests.Helpers
{
    public static class TestExtensions
    {
#if !Smartphone
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[]) field.GetCustomAttributes(typeof (DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
#endif

        public static string ToXml<T>(this T instance)
        {
            var type = typeof (T);
#if !Smartphone
            if (!type.IsSerializable)
            {
                return String.Empty;
            }
#endif

            var sb = new StringBuilder();
            var serializer = new XmlSerializer(type);

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, instance);
            }

            return sb.ToString();
        }

        public static T FromXml<T>(this string xml)
        {
            T type;

            var serializer = new XmlSerializer(typeof (T));

            using (var reader = new StringReader(xml))
            {
                type = (T) serializer.Deserialize(reader);
                reader.Close();
            }

            return type;
        }

        public static Uri AsUri(this string url)
        {
            return new Uri(url);
        }
    }
}