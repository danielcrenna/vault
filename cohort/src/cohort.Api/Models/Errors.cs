namespace cohort.API.Models
{
    public static class Errors
    {
        public static Error RequiresHttps { get; set; }
        public static Error ResourceNotFound { get; set; }
        public static Error MediaTypeNotSupported { get; set; }
        public static Error MissingAuthorizationHeader { get; set; }
        public static Error AuthenticationDeclined { get; set; }

        static Errors()
        {
            RequiresHttps = new Error("This API is only accessible over HTTPS.", "https_required");
            ResourceNotFound = new Error("A resource with type '{0}' could not be found at this location.", "resource_not_found");
            MediaTypeNotSupported = new Error("This API does not know how to respond to the requested media type. Please inspect your 'Accept' header to ensure it specifies one of the following supported media types: {0}", "media_type_not_supported");
            MissingAuthorizationHeader = new Error("This API requires authorization, but this request did not provide an authorization header value", "authorization_required");
            AuthenticationDeclined = new Error("The authorization provided is not valid.", "authentication_declined");
        }
    }
}