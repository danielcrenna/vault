using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ab
{
    /// <summary>
    /// <seealso href="https://github.com/assaf/vanity/blob/master/lib/vanity/experiment/ab_test.rb" />
    /// </summary>
    public class ABTest : IExperimentType 
    {
        internal IEnumerable<double[]> ZToProbability { get; private set; }

        public ABTest()
        {
            var a = 50.0;

            // build an array of [z-score, percentage]
            var normDist = new List<double[]>();
            for(var x = 0.0; x <= 3.1; x+=0.01)
            {
                normDist.Add(new[] { x, a += 1 / Math.Sqrt(2 * Math.PI) * Math.E.Pow((-x.Pow(2) / 2)) });        
            }

            // We're really only interested in 90%, 95%, 99% and 99.9%.
            ZToProbability = new[] { 90, 95, 99, 99.9 }.Select(pct => new []  { normDist.First(_ => _[1] >= pct)[0], pct }).Reverse();
        }

        public virtual double ProbabilityOfScore(double score)
        {
            score = score.Abs();
            var probability = ZToProbability.FirstOrDefault(pair => score >= pair[0]);
            return probability != null ? probability.Last() : 0;
        }

        public virtual Score Score(Experiment experiment, double probability = 90.0)
        {
            var score = new Score();

            // sort by conversion rate
            var sorted = experiment.Score(experiment);
            var @base = sorted.Skip(1).Take(1).First(); // sorted[-2]

            // calculate z-score
            var cohort = experiment.ParticipantsByAlternative();
            var pc = @base.Value;
            var nc = cohort[@base.Key];
            foreach (var alt in sorted)
            {
                var p = alt.Value;
                var index = alt.Key;
                var n = cohort[index];
                var zscore = (p - pc)/((p * (1 - p) / n) + (pc * (1 - pc) / nc)).Abs().Pow(0.5);
                zscore = double.IsNaN(zscore) ? 0 : zscore;
                score.Alternatives.Add(new Alternative
                {
                    Index = index,
                    Name = experiment.Alternatives[index - 1].ToString(),
                    Label = "Option " + (char)(index + 64),
                    Value = alt.Value,
                    ZScore = zscore,
                    Probability = ProbabilityOfScore(zscore)
                });
            }
            score.Base = score.Alternatives[@base.Key - 1];

            // difference is measured from least performant
            KeyValuePair<int, double> least;
            if ((least = sorted.FirstOrDefault(kv => kv.Value > 0)).Value > 0)
            {
                foreach (var alt in sorted.Where(alt => alt.Value > least.Value))
                {
                    score.Alternatives[alt.Key].Difference = (alt.Value - least.Value) / least.Value * 100;
                }
                score.Least = score.Alternatives[least.Key - 1];
            }

            // best alternative is one with highest conversion rate (best shot).
            if (sorted.Last().Value > 0.0)
            {
                score.Best = score.Alternatives[sorted.Last().Key - 1];
            }

            // choice alternative can only pick best if we have high probability (>90%)
            score.Choice = experiment.Outcome.HasValue
                               ? score.Alternatives[experiment.Outcome.Value - 1]
                               : (score.Best != null && score.Best.Probability >= probability ? score.Best : null);

            return score;
        }

        public virtual string Conclusion(Score score, double probability = 90.0)
        {
            var sb = new StringBuilder();

            var participants = score.Alternatives.Count();
            switch(participants)
            {
                case 0:
                    sb.Append("There are no participants in this experiment. ");
                    break;
                case 1:
                    sb.Append("There is one participant in this experiment. ");
                    break;
                default:
                    sb.AppendFormat("There are {0} participants in this experiment. ", participants);
                    break;
            }

            // only interested in sorted alternatives that converted
            var sorted = score.Alternatives.Where(alt => alt.Value > 0.0).OrderBy(alt => alt.Value).Reverse().ToList();
            if(sorted.Count() > 1)
            {
                // start with alternatives that have conversion, from best to worst, then alternatives with no conversion.
                sorted = sorted.Intersect(score.Alternatives).ToList();
                var best = sorted[0];
                var second = sorted[1];
                if (best.Value > second.Value)
                {
                    var diff = ((best.Value - second.Value)/second.Value*100).Round();
                    var better = diff > 0 ? string.Format(" ({0:d}% better than {1})", diff, second.Label) : "";
                    sb.AppendFormat("The best choice is {0}: it converted at {1:f1}{2}.", best.Label, best.Value, better);
                    if(best.Probability >= probability)
                    {
                        sb.AppendFormat("With {0:d}% probability this result is statistically significant.", score.Best.Probability);
                    }
                    else
                    {
                        sb.Append("This result is not statistically significant, you should continue this experiment.");
                    }
                    sorted.Remove(best);
                }
                foreach(var alt in sorted)
                {
                    if(alt.Value > 0.0)
                    {
                        sb.AppendFormat("{0} converted at {1:f1}%", alt.Label, alt.Value*100);
                    }
                    else
                    {
                        sb.AppendFormat("{0} did not convert.", alt.Label);
                    }
                }
            }
            else
            {
                sb.Append("This experiment has not run long enough to find a clear winner.");
            }
            if(score.Choice != null)
            {
                sb.Append(score.Choice.Name + " selected as the best alternative.");
            }
            return sb.ToString();
        }
    }
}