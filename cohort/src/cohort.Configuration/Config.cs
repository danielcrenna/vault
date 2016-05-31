using container;

namespace cohort.Configuration
{
    public class Config
    {
        public static Container Container { get; private set; }

        static Config()
        {
            Container = new Container();
        }
    }
}