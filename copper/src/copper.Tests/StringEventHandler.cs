using System.Threading.Tasks;

namespace copper.Tests
{
    public class StringEventHandler : Consumes<StringEvent>
    {
        public bool Handled { get; private set; }

        public bool Handle(StringEvent @event)
        {
            Handled = true;
            return true;
        }

        public async Task<bool> HandleAsync(StringEvent @event)
        {
            return await Task.Factory.StartNew(() => Handle(@event));
        }
    }
}