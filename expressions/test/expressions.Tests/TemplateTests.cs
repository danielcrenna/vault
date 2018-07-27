using System;
using System.Linq;
using DotLiquid;
using Xunit;

namespace expressions.Tests
{
    public class TemplateTests
    {
        [Fact]
        public void Is_this_even_possible()
        {
            var a = 9;
            var r = Expression.Evaluate(a);
            Assert.Equal(3, r);
        }

        public static class Expression
        {
            static Expression() {  InternalFunctions.Register(); }

            public static dynamic Evaluate(dynamic a)
            {
                Template t = Template.Parse("{{ 'sqr(a)' | inline_functions | clean }}");
                var result = t.Render(Hash.FromAnonymousObject(new
                {
                    a
                }));
                return result;
            }
        }




        [Theory]
        [InlineData("{% sqr(9) %}", "3")]
        [InlineData("{% max(9, 10) %}", "10")]
        [InlineData("{% min(9, 10) %}", "9")]
        [InlineData("{% abs(-100) %}", "100")]
        public void Can_render_internal_functions(string source, string expected)
        {
            InternalFunctions.Register();
            Template template = Template.Parse(source);
            string actual = template.Render();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("{{ '100 + a' | inline_functions | clean }}", new[] { "a" }, new object[] { 50 }, 150)]
        public void Can_inset_expressions_into_template_syntax(string source, string[] parameterNames, object[] values, object expected)
        {
            // 1. Prepare / Sanitize
            InternalFunctions.Register();
            Template template = Template.Parse(source);
            string expression = template.Render();

            // 2. Compile
            IExecutionContext context = ExpressionCompiler.Compile(ExpressionFeatures.Default, expression, parameterNames);
            Type[] types = values?.Select(v => v.GetType()).ToArray() ?? new Type[0];

            // 3. Execute
            object actual = context.Invoke(ExpressionFeatures.Default, types, values);
            Assert.Equal(expected, actual);
        }
    }
}
