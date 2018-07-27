using System;
using System.Linq;
using Xunit;

namespace expressions.Tests
{
    public class ExpressionServiceTests
    {
        [Theory]

        //
        // Basic:
        // 
        [InlineData("100", null, null, 100)]                                                // 100 => 100
        [InlineData("a", new[] { "a" }, new object[] { 50 }, 50)]                           // a => 50
        [InlineData("100 + a", new[] { "a" }, new object[] { 50 }, 150)]                    // 100 + a => 150
        [InlineData("100 + a + b", new[] { "a", "b" }, new object[] { 50, 5.5 }, 155.5)]    // 100 + a + b => 155.5

        //
        // Functions:
        //
        [InlineData("sqr(a)", new[] { "a" }, new object[] { 9 }, 81)]
        public void These_are_valid_expressions(string expression, string[] parameterNames, object[] values, object expected)
        {
            ExpressionService service = new ExpressionService();
            Type[] types = values?.Select(v => v.GetType()).ToArray() ?? new Type[0];
            object actual = service.Evaluate(expression, parameterNames, values, types);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Supports_dot_notation()
        {
            //string expression = "100 + a.SomeProperty";
            //object[] values = new object[] { new { SomeProperty = 50; }};
            //Type[] types = values?.Select(v => v.GetType()).ToArray() ?? new Type[0];
            //ExpressionService service = new ExpressionService();
            //object actual = service.Evaluate(expression, parameterNames, values, types);
            //Assert.Equal(expected, actual);
        }
    }
}