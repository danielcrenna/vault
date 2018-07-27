using System;
using DotLiquid;

namespace expressions
{
    public static class InternalFunctions
    {
        public static void Register()
        {
            Template.RegisterTag<SquareRoot>("sqr");
            Template.RegisterTag<Max>("max");
            Template.RegisterTag<Min>("min");
            Template.RegisterTag<Abs>("abs");
            
            Template.RegisterFilter(typeof(InternalFunctions));
        }

        public static string InlineFunctions(string input)
        {
            return input;
        }

        public static string Clean(string input)
        {
            return input;
        }

        public class SquareRoot : MathFunction
        {
            public override Func<double, double> F1 => Math.Sqrt;
        }

        public class Max : MathFunction
        {
            public override Func<double, double, double> F2 => Math.Max;
        }

        public class Min : MathFunction
        {
            public override Func<double, double, double> F2 => Math.Min;
        }

        public class Abs : MathFunction
        {
            public override Func<double, double> F1 => Math.Abs;
        }
    }
}