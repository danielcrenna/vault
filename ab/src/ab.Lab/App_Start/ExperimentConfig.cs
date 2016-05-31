using System.Linq;

namespace ab.Lab
{
    public class ExperimentConfig
    {
        public static void Register()
        {
            Experiments.Register(
                name: "Jokes on link", 
                description: "Testing to prove that more people will click the link if there's a joke on it.",
                metrics: new [] { "Button clicks" },                             // Associates ticks against the "Button clicks" counter with this experiment
                alternatives: new object[] { false, true },                      // Typed experiment alternatives ; default is common "A/B" binary case
                conclude: experiment => experiment.Participants.Count()  == 10,  // Optional criteria for automatically concluding an experiment; default is never
                score: null, /* ... */                                           // Optional criteria for choosing best performer by index; default is best converting alternative
                splitOn: null /* ... */                                          // Optional criteria for splitting a cohort by the number of alternatives; default is a simple hash split
            );
        }
    }
}