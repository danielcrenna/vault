using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using cohort.API.Models;

namespace cohort.API.Handlers
{
    /// <summary>
    /// Rejects API calls that don't accept our supported media types
    /// </summary>
    public class AcceptNegotiationHandler : DelegatingHandler
    {
        private readonly string _acceptedMedia;
        private readonly HttpConfiguration _configuration;
        private const string AllMediaTypes = "*/*";
        
        public AcceptNegotiationHandler(HttpConfiguration configuration)
        {
            _configuration = configuration;
            _acceptedMedia = string.Join(", ", _configuration.Formatters.SelectMany(f => f.SupportedMediaTypes).Select(f => f.MediaType));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var acceptHeader = request.Headers.Accept;
            if (acceptHeader.All(x => x.MediaType != AllMediaTypes))
            {
                if (!CanServeRequestMediaType(_configuration, acceptHeader))
                {
                    return Task<HttpResponseMessage>.Factory.StartNew(() =>
                    {
                        var response = request.CreateResponse(HttpStatusCode.NotAcceptable, Errors.MediaTypeNotSupported.ToHttpError(_acceptedMedia));
                        return response;
                    }, cancellationToken);
                }
            }
            return base.SendAsync(request, cancellationToken);
        }

        private static bool CanServeRequestMediaType(HttpConfiguration configuration, IEnumerable<MediaTypeWithQualityHeaderValue> acceptHeaderValues)
        {
            var mediaTypeWithQualityHeaderValues = acceptHeaderValues as List<MediaTypeWithQualityHeaderValue> ?? acceptHeaderValues.ToList();
            return (from formatter in configuration.Formatters
                    from mediaType in mediaTypeWithQualityHeaderValues
                    where formatter.SupportedMediaTypes.Contains(mediaType)
                    select formatter).Any();
        }
    }
}