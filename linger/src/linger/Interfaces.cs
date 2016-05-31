using System;

namespace linger
{
    public interface Before
    {
        void Before();
    }

    public interface Perform
    {
        bool Perform();
    }

    public interface After
    {
        void After();
    }

    public interface Success
    {
        void Success();
    }

    public interface Error
    {
        void Error(Exception error);
    }

    public interface Failure
    {
        void Failure();
    }

    public interface Halt
    {
        void Halt(bool immediate);
    }
}