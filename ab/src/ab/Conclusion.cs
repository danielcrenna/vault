using System;

namespace ab
{
    public class Conclusion
    {
        public static Lazy<Func<Experiment, bool>> Default = new Lazy<Func<Experiment, bool>>(ExperimentsDoNotConcludeAutomatically);

        private static Func<Experiment, bool> ExperimentsDoNotConcludeAutomatically()
        {
            return (experiment) => false;
        }
    }
}