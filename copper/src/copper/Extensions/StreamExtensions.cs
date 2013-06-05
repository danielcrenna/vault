using System.IO;
using System.Threading.Tasks;

namespace copper.Extensions
{
    internal static class StreamExtensions
    {
        public static Task<int> ReadAsync(this Stream source, byte[] buffer, int offset, int count)
        {
            return Task<int>.Factory.FromAsync(source.BeginRead, source.EndRead, buffer, offset, count, null);
        }

        public static Task WriteAsync(this Stream destination, byte[] buffer, int offset, int count)
        {
            return Task.Factory.FromAsync(destination.BeginWrite, destination.EndWrite, buffer, offset, count, null);
        }

        public static async Task CopyToAsync(this Stream source, Stream destination)
        {
            var buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
            }
        }

        // Original source: Jon Skeet, obviously
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