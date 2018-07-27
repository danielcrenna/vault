using System;
using System.Collections.Generic;
using System.IO;
using DotLiquid;

namespace expressions
{
    public abstract class MathFunction : Tag
    {
        public virtual Func<double, double> F1 => null;
        public virtual Func<double, double, double> F2 => null;

        private string[] tokenized;

        private static readonly char[] separators = { ',' }; 

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            base.Initialize(tagName, markup, tokens);

            if (!string.IsNullOrWhiteSpace(markup))
            {
                int start = markup.IndexOf("(", StringComparison.Ordinal);
                if (start != -1)
                {
                    int end = markup.LastIndexOf(")", StringComparison.Ordinal);
                    if (end != -1)
                    {
                        try
                        {
                            string expr = markup.Substring(start + 1, end - start - 1);

                            tokenized = expr.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        }
                        catch { }
                    }
                }
            }
        }


        public override void Render(Context context, TextWriter result)
        {
            switch (tokenized.Length)
            {
                case 1:
                {
                    if (F1 != null)
                    {
                        try
                        {
                            var x = tokenized[0];
                            double d;
                            if (double.TryParse(x, out d))
                                result.Write(F1(d));
                        }
                        catch { }
                    }
                    break;
                }

                case 2:
                    {
                        if (F2 != null)
                        {
                            try
                            {
                                var x = tokenized[0];
                                var y = tokenized[1];

                                double d1, d2;
                                if (double.TryParse(x, out d1) && double.TryParse(y, out d2))
                                    result.Write(F2(d1, d2));
                            }
                            catch { }
                        }
                        break;
                    }
            }
        }
    }
}