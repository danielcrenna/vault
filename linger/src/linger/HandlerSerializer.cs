using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace linger
{
    internal static class HandlerSerializer
    {
        private static readonly BinaryFormatter Formatter;

        static HandlerSerializer()
        {
            Formatter = new BinaryFormatter();
        }

        public static byte[] Serialize<T>(T obj) where T : class
        {
            using (var ms = new MemoryStream())
            {
                Formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] data) where T : class
        {
            using (var ms = new MemoryStream(data))
            {
                return (T)Formatter.Deserialize(ms);
            }
        }
    }
}