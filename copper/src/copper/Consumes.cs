using System.Threading.Tasks;

namespace copper
{
    /// <summary>
    /// An event handler; contains the processing or storage logic for when an event is received
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface Consumes<in T>
    {
        bool Handle(T @event);
        Task<bool> HandleAsync(T @event);
    }
}