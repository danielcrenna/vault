using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace depot
{
    public class CompressionBase
    {
        public static byte[] Unzip(byte[] data)
        {
            using (var input = new MemoryStream(data))
            {
                using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                {
                    using(var output = new MemoryStream())
                    {
                        gzip.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
        }

        public static byte[] Zip(IEnumerable value)
        {
            byte[] buffer;
            using (var stream = new MemoryStream())
            {
                using (var zip = new GZipStream(stream, CompressionMode.Compress))
                {
                    using (var sw = new StreamWriter(zip))
                    {
                        sw.Write(value);
                    }
                }
                buffer = stream.ToArray();
            }
            return buffer;
        }

        public static bool IsCompressed(IList<byte> data)
        {
            // http://stackoverflow.com/questions/4662821/is-there-a-way-to-know-if-the-byte-has-been-compressed-by-gzipstream
            // http://www.ietf.org/rfc/rfc1952.txt
            if (data == null || data.Count < 10)
            {
                return false;
            }
            var header = new byte[] { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 4, 0 };
            for (var i = 0; i < 10; i++)
            {
                if (i == 8)
                {
                    if (data[i] != 2 && data[i] != 4)
                    {
                        return false;
                    }
                }
                if (data[i] != header[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}