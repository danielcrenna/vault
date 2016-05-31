using System;

namespace linger.Tests
{
    [Serializable]
    public class HelloWorldJob
    {
        public bool Perform()
        {
            Console.WriteLine("Hello, world!");
            return true;
        }
    }
}