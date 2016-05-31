using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Hammock.Authentication;
using Hammock.Caching;
using Hammock.Model;
using Hammock.Retries;
using Hammock.Serialization;
using Hammock.Tasks;
using Hammock.Web;
using Hammock.Streaming;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#else
using System.Collections.Specialized;
#endif

namespace Hammock
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum QueryHandling
    {
        /// <summary>
        /// Query strings present in paths are left alone.
        /// </summary>
        None,
        /// <summary>
        /// Query string pairs present in paths are added to the parameter collection,
        /// where they are appended back in the case of GET, HEAD, DELETE, and OPTIONS, or added to the
        /// request body in the case of POST, or PUT.
        /// </summary>
        AppendToParameters
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class RestBase : PropertyChangedBase
    {
        private byte[] _postContent;
        private TaskOptions _taskOptions;
        private RetryPolicy _retryPolicy;

        public WebParameterCollection GetAllHeaders()
        {
            var headers = new WebParameterCollection();

            var parameters = Headers.AllKeys.Select(key => new WebPair(key, Headers[key]));
            foreach (var parameter in parameters)
            {
                headers.Add(parameter.Name, parameter.Value);
            }

            return headers;
        }

        protected virtual internal NameValueCollection Headers { get; set; }
        protected virtual internal WebParameterCollection Parameters { get; set; }
        protected virtual internal WebParameterCollection Cookies { get; set; }
        protected virtual internal ICollection<HttpPostParameter> PostParameters { get; set; }
        protected virtual internal byte[] PostContent
        {
            get
            {
                return _postContent;
            }
            set
            {
                _postContent = value;
                if (value != null && (Method != WebMethod.Post && Method != WebMethod.Put))
                {
                    Method = WebMethod.Post;
                }
            }
        }

        public virtual Func<RestRequest, RestResponseBase, Type> GetErrorResponseEntityType { get; set; }
        public virtual string UserAgent { get; set; }
        public virtual WebMethod? Method { get; set; }
        public virtual IWebCredentials Credentials { get; set; }
        public virtual Encoding Encoding { get; set; }
        public virtual bool TraceEnabled { get; set; }

        protected RestBase()
        {
            Initialize();
        }

        private void Initialize()
        {
            Headers = new NameValueCollection(0);
            Parameters = new WebParameterCollection();
            Cookies = new WebParameterCollection(0);
            PostParameters = new List<HttpPostParameter>(0);
        }

#if !Silverlight
        public virtual ServicePoint ServicePoint { get; set; }
        public virtual bool? FollowRedirects { get; set; }
#endif

        public virtual QueryHandling? QueryHandling { get; set; }
        public virtual string Proxy { get; set; }
        public virtual TimeSpan? Timeout { get; set; }
        public virtual string VersionPath { get; set; }
        public virtual ISerializer Serializer { get; set; }
        public virtual IDeserializer Deserializer { get; set; }
        public virtual ICache Cache { get; set; }
        public virtual CacheOptions CacheOptions { get; set; }
        public virtual RetryPolicy RetryPolicy
        {
            get { return _retryPolicy; }
            set
            {
                if (_retryPolicy == value)
                {
                    return;
                }
                _retryPolicy = value;
                RetryState = new TaskState();
            }
        }

        public virtual TaskOptions TaskOptions
        {
            get { return _taskOptions; }
            set
            {
                if (_taskOptions == value)
                {
                    return;
                }
                _taskOptions = value;
                TaskState = new TaskState();
            }
        }

        public virtual bool IsFirstIteration
        {
            get
            {
                if (RetryState != null)
                {
                    return RetryState.RepeatCount == 0;
                }
                if (TaskState != null)
                {
                    return TaskState.RepeatCount == 0;
                }
                return true; 
            }
        }
        
        public virtual ITaskState TaskState { get; set; }
        public virtual ITaskState RetryState { get; set; }
        public virtual StreamOptions StreamOptions { get; set; }
        public virtual Func<string> CacheKeyFunction { get; set; }
        public virtual DecompressionMethods? DecompressionMethods { get; set; }
        public virtual IWebQueryInfo Info { get; set; }
        public virtual string Path { get; set; }
        public virtual object Tag { get; set; }
#if !NETCF
        public virtual CookieContainer CookieContainer { get; set; }
#endif
        public virtual void AddHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        public virtual void AddParameter(string name, string value)
        {
            Parameters.Add(name, value);
        }

        [Obsolete("Use CookieContainer instead.")]
        public virtual void AddCookie(string name, string value)
        {
            Cookies.Add(new HttpCookieParameter(name, value));
        }

        [Obsolete("Use CookieContainer instead.")]
        public virtual void AddCookie(Uri domain, string name, string value)
        {
            Cookies.Add(new HttpCookieParameter(name, value) { Domain = domain });
        }

        public virtual void AddField(string name, string value)
        {
            var field = new HttpPostParameter(name, value);
            PostParameters.Add(field);
        }

        public virtual void AddFile(string name, string fileName, string filePath)
        {
            AddFile(name, fileName, filePath, "application/octet-stream", "form-data");
        }

        public virtual void AddFile(string name, string fileName, string filePath, string contentType)
        {
            AddFile(name, fileName, filePath, contentType, "form-data");
        }

        public virtual void AddFile(string name, string fileName, Stream stream)
        {
            AddFile(name, fileName, stream, "application/octet-stream", "form-data");
        }

        public virtual void AddFile(string name, string fileName, Stream stream, string contentType)
        {
            AddFile(name, fileName, stream, contentType, "form-data");
        }

        public virtual void AddFile(string name, string fileName, string filePath, string contentType, string contentDisposition)
        {
            var parameter = HttpPostParameter.CreateFile(name, fileName, filePath, contentType, contentDisposition);
            PostParameters.Add(parameter);
        }

        public virtual void AddFile(string name, string fileName, Stream stream, string contentType, string contentDisposition)
        {
            var parameter = HttpPostParameter.CreateFile(name, fileName, stream, contentType, contentDisposition);
            PostParameters.Add(parameter);
        }

        public virtual void AddPostContent(byte[] content)
        {
            if (PostContent == null)
            {
                PostContent = content;
            }
            else
            {
                var original = PostContent.Length;
                var current = content.Length;

                var final = new byte[current + original];
                Array.Copy(PostContent, 0, final, 0, original);
                Array.Copy(content, 0, final, original, current);

                PostContent = final;
            }
        }
    }

    public class RetryEventArgs : EventArgs
    {
        public virtual RestClient Client { get; set; }
        public virtual RestRequest Request { get; set; }
    }

    public class FileProgressEventArgs : EventArgs
    {
        public virtual string FileName { get; set; }
        public virtual long BytesWritten { get; set; }
        public virtual long TotalBytes { get; set; }
    }
}