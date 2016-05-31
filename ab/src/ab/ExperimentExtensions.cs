using System.Collections.Generic;
using System.Linq;

namespace ab
{
    internal static class ExperimentExtensions
    {
        public static IDictionary<int, double> ConversionRateByAlternative(this Experiment experiment)
        {
            var participants = experiment.ParticipantsByAlternative();
            var converted = experiment.ConvertedByAlternative();
            
            var hash = experiment.EmptyHash<double>();
            for (var i = 1; i <= experiment.Alternatives.Count(); i++)
            {
                var conversionRate = participants[i] > 0 ? (converted[i] / (double)participants[i]) * 100 : 0;
                hash[i] = conversionRate;
            }
            return hash;
        }

        public static IDictionary<int, int> ParticipantsByAlternative(this Experiment experiment)
        {
            var hash = experiment.EmptyHash<int>();
            foreach (var participant in experiment.Participants)
            {
                if (!participant.Shown.HasValue)
                {
                    continue;
                }
                hash[participant.Shown.Value]++;
            }
            return hash;
        }

        public static IDictionary<int, int> ConvertedByAlternative(this Experiment experiment)
        {
            var hash = experiment.EmptyHash<int>();
            foreach (var participant in experiment.Participants)
            {
                if (!participant.Shown.HasValue)
                {
                    continue;
                }
                if (participant.Converted)
                {
                    hash[participant.Shown.Value]++;
                }
            }
            return hash;
        }

        public static IDictionary<int, int> ConversionsByAlternative(this Experiment experiment)
        {
            var hash = experiment.EmptyHash<int>();
            foreach (var participant in experiment.Participants)
            {
                if (!participant.Shown.HasValue)
                {
                    continue;
                }
                hash[participant.Shown.Value] += participant.Conversions;
            }
            return hash;
        }

        private static IDictionary<int, T> EmptyHash<T>(this Experiment experiment)
        {
            var hash = new Dictionary<int, T>();
            for (var i = 1; i <= experiment.Alternatives.Count(); i++)
            {
                hash.Add(i, default(T));
            }
            return hash;
        }
    }
}