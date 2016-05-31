using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using cohort.API.Models;
using cohort.Api.Models;

namespace cohort.Api.Controllers
{
    public abstract class ResourceController<T> : ApiController where T : class, IResource, new()
    {
        private readonly IResourceRepository<T> _repository;
        
        protected ResourceController(IResourceRepository<T> repository)
        {
            _repository = repository;
        }
        
        // http://stackoverflow.com/questions/924472/paging-in-a-rest-collection
        // http://www.javaworld.com/community/node/8295
        public virtual ResourceCollection<T> Get()
        {
            return Get(null, null);
        }

        public virtual ResourceCollection<T> Get([FromUri]int? offset, [FromUri]int? count)
        {
            var resources = _repository.Get(offset, count);
            AddPagingToEntityGraph(resources);
            return resources;
        }

        private void AddPagingToEntityGraph(ResourceCollection<T> resources)
        {
            var requestUrl = Request.RequestUri.ToString();
            if (resources.HasNextPage)
            {
                if (requestUrl.Contains("offset="))
                {
                    resources.NextPage = requestUrl.Replace(string.Format("offset={0}", resources.PageIndex),
                                                            string.Format("offset={0}", resources.PageIndex + 1));
                }
                else
                {
                    resources.NextPage = string.Format("{0}&offset={1}", requestUrl, resources.PageIndex + 1);
                }
            }
            if (resources.HasPreviousPage)
            {
                if (requestUrl.Contains("offset="))
                {
                    resources.PreviousPage = requestUrl.Replace(string.Format("offset={0}", resources.PageIndex),
                                                                string.Format("offset={0}", resources.PageIndex - 1));
                }
                else
                {
                    resources.PreviousPage = string.Format("{0}&offset={1}", requestUrl, resources.PageIndex - 1);
                }
            }
        }

        public virtual HttpResponseMessage Get(int id)
        {
            var request = ControllerContext.Request;
            var resource = _repository.Get(id);
            HttpResponseMessage response;
            if(resource == null)
            {
                response = request.CreateResponse(HttpStatusCode.NotFound, Errors.ResourceNotFound.ToHttpError(new T().Type));
                return response;
            }
            response = request.CreateResponse(HttpStatusCode.OK, resource);
            return response;
        }

        public virtual HttpResponseMessage Post(T resource)
        {
            _repository.Save(resource);
            var response = Request.CreateResponse(HttpStatusCode.Created, resource);
            response.Headers.Location = new Uri(Request.RequestUri, string.Format("{0}/{1}", resource.Type, resource.Id));
            return response;
        }
    }
}