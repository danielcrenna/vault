using System;

#if NET40
using System.Dynamic;
#endif

namespace Hammock
{
    public interface IRestClient
    {
#if !Silverlight

#if NET40
        RestResponse<dynamic> RequestDynamic(RestRequest request);
#endif
        RestResponse Request(RestRequest request);
        RestResponse Request();
        
        RestResponse<T> Request<T>(RestRequest request);
        RestResponse<T> Request<T>();
#endif

#if !WindowsPhone
        IAsyncResult BeginRequest();
        IAsyncResult BeginRequest<T>();
        
        IAsyncResult BeginRequest(RestRequest request, RestCallback callback);
        IAsyncResult BeginRequest(RestRequest request, RestCallback callback, object userState);

        IAsyncResult BeginRequest<T>(RestRequest request, RestCallback<T> callback);
        IAsyncResult BeginRequest<T>(RestRequest request, RestCallback<T> callback, object userState);

#if NET40
        IAsyncResult BeginRequestDynamic();
        IAsyncResult BeginRequestDynamic(RestRequest request, RestCallback<dynamic> callback);
        IAsyncResult BeginRequestDynamic(RestRequest request, RestCallback<dynamic> callback, object userState);
        RestResponse<dynamic> EndRequestDynamic(IAsyncResult result);
#endif

        IAsyncResult BeginRequest(RestRequest request);
        IAsyncResult BeginRequest(RestRequest request, object userState);
        IAsyncResult BeginRequest<T>(RestRequest request);
        IAsyncResult BeginRequest<T>(RestRequest request, object userState);
        
        IAsyncResult BeginRequest(RestCallback callback);
        IAsyncResult BeginRequest<T>(RestCallback<T> callback);
        
        RestResponse EndRequest(IAsyncResult result);
        RestResponse<T> EndRequest<T>(IAsyncResult result);
#else
        void BeginRequest();
        void BeginRequest<T>();

        void BeginRequest(RestRequest request, RestCallback callback);
        void BeginRequest(RestRequest request, RestCallback callback, object userState);

        void BeginRequest<T>(RestRequest request, RestCallback<T> callback);
        void BeginRequest<T>(RestRequest request, RestCallback<T> callback, object userState);

        void BeginRequest(RestRequest request);
        void BeginRequest(RestRequest request, object userState);
        void BeginRequest<T>(RestRequest request);
        void BeginRequest<T>(RestRequest request, object userState);

        void BeginRequest(RestCallback callback);
        void BeginRequest<T>(RestCallback<T> callback);
#endif
    }
}