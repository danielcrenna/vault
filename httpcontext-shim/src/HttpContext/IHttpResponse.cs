namespace HttpContextShim
{
    public interface IHttpResponse
    {
        object Inner { get; }
    }
}