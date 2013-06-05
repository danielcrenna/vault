namespace copper
{
    /// <summary>
    /// A producer of events that intends to send those events to an attached <see cref="Consumes{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface Produces<out T>
    {
        void Attach(Consumes<T> consumer);
    }
}