using System;
using System.Collections.Generic;
using System.Linq;

namespace ab
{
    internal class ViewModelMapper
    {
        public class ExperimentViewModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
            public string CreatedAt { get; set; }
            public IEnumerable<AlternativeViewModel> Alternatives { get; set; }
            public bool Active { get; set; }
            public DateTime? ConcludedAt { get; set; }
            public string Claims { get; set; }
        }
        public class AlternativeViewModel
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public int Participants { get; set; }
            public int Converted { get; set; }
            public double ConversionRate { get; set; }
            public bool Showing { get; set; }
            public bool Choice { get; set; }
        }

        public static IEnumerable<ExperimentViewModel> ProjectExperiments(IEnumerable<Experiment> allExperiments)
        {
            return allExperiments.Select(experiment =>
            {
                var score = experiment.Type.Score(experiment);
                return new ExperimentViewModel
                {
                    Name = experiment.Name,
                    Description = experiment.Description,
                    Type = "(A/B Test)",
                    CreatedAt = experiment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Alternatives = ProjectAlternatives(experiment, score),
                    Active = !experiment.ConcludedAt.HasValue,
                    Claims = experiment.Type.Conclusion(score),
                    ConcludedAt = experiment.ConcludedAt
                };
            });
        }

        public static IEnumerable<AlternativeViewModel> ProjectAlternatives(Experiment experiment, Score score)
        {
            var index = 1;
            foreach (var alternative in experiment.Alternatives)
            {
                var value = alternative.ToString();
                var vm = new AlternativeViewModel
                {
                    Name = "Option " + (char)(index + 64),
                    Participants = experiment.ParticipantsByAlternative()[index],
                    Converted = experiment.ConvertedByAlternative()[index],
                    ConversionRate = experiment.ConversionRateByAlternative()[index],
                    Showing = experiment.AlternativeValue.ToString() == value,
                    Choice = score.Choice != null && score.Choice.Index == index,
                    Value = value
                };
                index++;
                yield return vm;
            }
        }

        //public static IEnumerable<MetricViewModel> ProjectMetrics(IEnumerable<KeyValuePair<MetricName, IMetric>> allMetrics)
        //{
        //    return Enumerable.Empty<MetricViewModel>();

        //    var samples = SampleRepository.GetAllHashedByDay();
        //    return allMetrics.Select(metric =>
        //    {
        //        var name = metric.Key.Name.Replace(M.Header, string.Empty);
        //        var splitOn = name.IndexOf(M.Separator, StringComparison.Ordinal);
        //        var day = name.Substring(splitOn + 2);
        //        name = name.Substring(0, splitOn);
        //        var vm = new MetricViewModel
        //        {
        //            Name = name,
        //            Samples = samples
        //        };
        //        return vm;
        //    });
        //}
    }
}