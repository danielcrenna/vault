using System.Linq;
using System.Web;

namespace ab
{
    public static class AB
    {
        private static readonly IExperimentRepository ExperimentRepository;
        static AB()
        {
            ExperimentRepository = new ExperimentRepository();
        }

        /// <summary>
        /// Returns whether the current identity is in an alternate group than the control group. 
        /// Useful for simple AB tests with two alternatives. 
        /// </summary>
        /// <example>
        /// if(AB.Test("My experiment"))
        /// {
        ///     // Show the alternative   
        /// }
        /// else
        /// {
        ///     // Show the control
        /// }
        /// </example>
        /// <param name="experiment"></param>
        /// <returns></returns>
        public static bool Test(string experiment)
        {
            return Group(experiment) != 1;
        }
        
        /// <summary>
        /// Returns the value of the current identity's experiment group. Groups are 1-based.
        /// </summary>
        /// <example>
        /// switch(AB.Group("My experiment"))
        /// {
        ///     case 1:
        ///         // Show this
        ///         break;
        ///     case 2:
        ///         // Show that
        ///         break;
        ///     case 3:
        ///         // Show the other thing
        ///         break;
        /// }
        /// </example>
        /// <param name="experiment"></param>
        /// <returns></returns>
        public static int Group(string experiment)
        {
            var exp = GetExperiment(experiment);
            return exp == null ? 1 : exp.AlternativeIndex;
        }

        /// <summary>
        /// Returns the value of the current experiment choice, as determined by the experiment identity.
        /// </summary>
        /// <example>
        /// <p>
        ///     Hello, @AB.Value("Polite or rude")!
        /// </p>
        /// </example>
        /// <param name="experiment"></param>
        /// <returns></returns>
        public static IHtmlString Value(string experiment)
        {
            var exp = GetExperiment(experiment);
            var choice = exp == null ? "?" : exp.AlternativeValue;
            return new HtmlString(choice.ToString());
        }

        /// <summary>
        /// Returns a specified value for the current experiment choice.
        /// You must provide at least as many values as experiment alternatives, otherwise this call is equivalent to <see cref="Value"/>.
        /// Extraneous values are ignored.
        /// </summary>
        /// <example>
        /// <p>
        ///     Hello, @AB.OneOf("Polite or rude", "smarty-pants", "dumby")!
        /// </p>
        /// </example>
        /// <param name="experiment"></param>
        /// <param name="values"> </param>
        /// <returns></returns>
        public static IHtmlString OneOf(string experiment, params string[] values)
        {
            var exp = GetExperiment(experiment);
            if(exp == null) return new HtmlString("?");
            return values.Length < exp.Alternatives.Count() ? Value(experiment) : new HtmlString(values[exp.AlternativeIndex - 1]);
        }

        private static Experiment GetExperiment(string experiment)
        {
            var exp = ExperimentRepository.GetByName(experiment);
            return exp;
        }
    }
}