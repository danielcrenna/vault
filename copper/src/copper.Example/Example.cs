using System.Threading;

namespace copper.Examples
{
    public interface Example
    {
        void Execute(AutoResetEvent block);
    }
}