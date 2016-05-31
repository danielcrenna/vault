using System;
using System.IO;
using System.Net;
using System.Text;
using ICSharpCode.SharpZipLib.Silverlight.GZip;
using ICSharpCode.SharpZipLib.Silverlight.Zip;

namespace Hammock.Silverlight.Compat
{
    public class GzipHttpWebResponse : HttpWebResponse
    {
        private const int ChunkSize = 2048;
       
        private readonly HttpWebResponse _response;

        public override string Method
        {
            get { return _response.Method; }
        }

        public override HttpStatusCode StatusCode
        {
            get { return _response.StatusCode; }
        }

        public override string StatusDescription
        {
            get { return _response.StatusDescription; }
        }

        public GzipHttpWebResponse(HttpWebResponse response)
        {
            _response = response;
        }

        public override void Close()
        {
            _response.Close();
        }

        public override Stream GetResponseStream()
        {
            Stream compressed = null;

            var responseStream = _response.GetResponseStream();

#if !WindowsPhone
            var contentEncoding = _response.Headers["Content-Encoding"];
            if (contentEncoding != null && contentEncoding.Contains("gzip"))
            {
                compressed = new GZipInputStream(responseStream);
            }
            else if (contentEncoding != null && contentEncoding.Contains("deflate"))
            {
                compressed = new ZipInputStream(responseStream);
            }
#else
            byte[] marker;
            responseStream = ReadIntoMemoryStream(responseStream, out marker);
            if (marker.Length > 1 && (marker[0] == 31 && marker[1] == 139))
            {
                compressed = new GZipInputStream(responseStream);
            }
#endif
            if (compressed != null)
            {
                var decompressed = new MemoryStream();
                var size = ChunkSize;
                var buffer = new byte[ChunkSize];
                while (true)
                {
                    size = compressed.Read(buffer, 0, size);
                    if (size > 0)
                    {
                        decompressed.Write(buffer, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
                decompressed.Seek(0, SeekOrigin.Begin);
                return decompressed;
            }

            return responseStream;
        }

        // [DC]: We have to read the entire stream in as HTTP response streams are read-once
        private static MemoryStream ReadIntoMemoryStream(Stream stream, out byte[] marker)
        {
            var buffer = new byte[8192];
            var ms = new MemoryStream();

            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            ms.Position = 0;
            marker = new byte[2];
            ms.Read(marker, 0, 2);
            ms.Position = 0;

            return ms;
        }

        public override long ContentLength
        {
            get { return _response.ContentLength; }
        }

        public override string ContentType
        {
            get { return _response.ContentType; }
        }

        public override WebHeaderCollection Headers
        {
            get { return _response.Headers; }
        }

        public override System.Uri ResponseUri
        {
            get { return _response.ResponseUri; }
        }
    }
}
