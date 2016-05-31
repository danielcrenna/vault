using System.Web.Http;

namespace cohort.API.Models
{
    public class Error
    {
        public string Message { get; set; }
        public string Type { get; set; }

        public Error()
        {

        }

        public Error(string message, string type)
        {
            Message = message;
            Type = type;
        }

        public HttpError ToHttpError(params object[] args)
        {
            var http = new HttpError();
            http["Message"] = string.Format(Message, args);
            http["Type"] = Type;
            return http;
        }
    }
}