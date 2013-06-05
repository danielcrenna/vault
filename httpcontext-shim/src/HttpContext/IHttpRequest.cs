namespace HttpContextShim
{
    public interface IHttpRequest
    {
        bool IsLocal { get; }
        string UserHostAddress { get; }
        object Inner { get; }
    }
}