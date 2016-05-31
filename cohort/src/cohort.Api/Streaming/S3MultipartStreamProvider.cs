using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace cohort.API.Streaming
{
    public class S3MultipartStreamProvider : MultipartStreamProvider
    {
        private AmazonS3Client _client;
        private PutObjectRequest _pending;

        public S3MultipartStreamProvider()
        {
            InitializeClient();
        }

        private void InitializeClient()
        {
            _client = new AmazonS3Client(ConfigurationManager.AppSettings["AmazonKey"],
                                         ConfigurationManager.AppSettings["AmazonSecret"]);
        }

        public override Task ExecutePostProcessingAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var response = _client.PutObject(_pending);
                Console.WriteLine(response);
            });
        }
        
        // Called by the main framework on each Content-Disposition header, i.e. once per file
        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            // This should be a multipart upload in order to avoid buffering everything into memory!

            _pending = new PutObjectRequest()
                     .WithBucketName(ConfigurationManager.AppSettings["AmazonBucket"])
                     .WithContentType(headers.ContentType.MediaType)
                     .WithCannedACL(S3CannedACL.PublicRead)
                     .WithKey(string.Format("user/media/images/{0}{1}", Guid.NewGuid(), Path.GetExtension(GetLocalFileName(headers))));

            _pending.InputStream = new MemoryStream(); // need an efficient buffering or fanout stream here for file parts / resume

            return _pending.InputStream;
        }

        public string GetLocalFileName(HttpContentHeaders headers)
        {
            var name = !string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName) ? headers.ContentDisposition.FileName : "untitled.png";
            return name.Replace("\"", string.Empty);
        }
    }
}