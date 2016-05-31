using Munq;

namespace container
{
    public class DefaultLifetime : ILifetime
    {
        private readonly IRegistration _registration;
        public DefaultLifetime(IRegistration registration)
        {
            _registration = registration;
        }
        public void Permanent()
        {
            _registration.AsContainerSingleton();
        }
        public void Thread()
        {
            _registration.AsThreadSingleton();
        }
        public void Request()
        {
            _registration.AsRequestSingleton();
        }
        public void Session()
        {
            _registration.AsSessionSingleton();
        }
        public void AlwaysNew()
        {
            _registration.AsAlwaysNew();
        }
    }
}