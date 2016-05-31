using System;
using System.Net;
using System.Text;
using Hammock.Extensions;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestRequest : RestBase
    {
        private object _entity;
        private object _expectEntity;

        protected internal virtual Web.WebHeaderCollection ExpectHeaders { get; set; }
        public virtual HttpStatusCode? ExpectStatusCode { get; set; }
        public virtual string ExpectStatusDescription { get; set; }
        public virtual string ExpectContent { get; set; }
        public virtual string ExpectContentType { get; set; }
        
        public RestRequest()
        {
            Initialize();
        }

        private void Initialize()
        {
            ExpectHeaders = new Web.WebHeaderCollection();
        }

        public virtual object Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                if (Equals(_entity, value))
                {
                    return;
                }

                _entity = value;
                OnPropertyChanged("Entity");
                RequestEntityType = _entity == null ? null : _entity.GetType();
            }
        }

        public virtual object ExpectEntity
        {
            get
            {
                return _expectEntity;
            }
            set
            {
                if (_expectEntity != null && _expectEntity.Equals(value))
                {
                    return;
                }

                _expectEntity = value;
                OnPropertyChanged("ExpectEntity");
            }
        }

        public virtual Type ResponseEntityType { get; set; }
        public virtual Type RequestEntityType { get; set; }

        public string BuildEndpoint(RestClient client)
        {
            var sb = new StringBuilder();

            var path = Path.IsNullOrBlank()
                           ? client.Path.IsNullOrBlank() ? "" : client.Path
                           : Path;

            var versionPath = VersionPath.IsNullOrBlank()
                                  ? client.VersionPath.IsNullOrBlank() ? "" : client.VersionPath
                                  : VersionPath;
            var skipAuthority = client.Authority.IsNullOrBlank();

            sb.Append(skipAuthority ? "" : client.Authority);
            sb.Append(skipAuthority ? "" : client.Authority.EndsWith("/") ? "" : "/");
            sb.Append(skipAuthority ? "" : versionPath.IsNullOrBlank() ? "" : versionPath);
            if (!skipAuthority && !versionPath.IsNullOrBlank())
            {
                sb.Append(versionPath.EndsWith("/") ? "" : "/");
            }
            sb.Append(path.IsNullOrBlank() ? "" : path.StartsWith("/") ? path.Substring(1) : path);

            var queryStringHandling = QueryHandling ?? client.QueryHandling ?? Hammock.QueryHandling.None;

            switch (queryStringHandling)
            {
                case Hammock.QueryHandling.AppendToParameters:
                    return WebExtensions.UriMinusQuery(sb.ToString(), Parameters);
                default:
                    return sb.ToString();
            }
        }

        public void ExpectHeader(string name, string value)
        {
            ExpectHeaders.Add(name, value);
        }
    }
}


