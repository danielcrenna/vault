namespace container
{
    public interface ILifetime
    {
        void AlwaysNew();
        void Permanent();
        void Thread();
        void Request();
        void Session();
    }
}