using DotLiquid;

namespace Expressions
{
    public static class Expression
    {
        public static dynamic Evaluate(dynamic a, dynamic b)
        {
            var t = Template.Parse("{{ 'sqr(a)' | inline_functions | clean }}");
            var result = t.Render(Hash.FromAnonymousObject(new
            {
                a, b
            }));
            return result;
        }
    }
}
