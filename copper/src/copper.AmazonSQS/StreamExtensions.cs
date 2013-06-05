using System.IO;

namespace copper.AmazonSQS
{
    // Original source: Jon Skeet, obviously
    internal static class StreamExtensions
    {
        public static byte[] ReadFully(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }
                return memoryStream.ToArray();
            }
        }
    }
}