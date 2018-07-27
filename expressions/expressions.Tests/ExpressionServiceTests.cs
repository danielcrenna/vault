using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace expressions.Tests
{
    public class ExpressionServiceTests
    {
        private readonly ITestOutputHelper _output;

        public ExpressionServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("100", null, null, 100)]                                                            // 100 => 100
        [InlineData("a", new[] { "a" }, new object[] { 50 }, 50)]                                       // a => 50
        [InlineData("100 + a", new[] { "a" }, new object[] { 50 }, 150)]                                // 100 + a => 150
        [InlineData("100 + a + b", new[] { "a", "b" }, new object[] { 50, 5.5 }, 155.5)]                // 100 + a + b => 155.5
        [InlineData("100 + a + b + c", new[] { "a", "b", "c" }, new object[] { 50, 5.5, 0.1 }, 155.6)]  // 100 + a + b + c => 155.6
        public void These_are_valid_expressions(string expression, string[] parameterNames, object[] values, object expected)
        {
            var trace = new TraceSource(nameof(ExpressionService));
            trace.Switch.Level = SourceLevels.All;
            trace.Listeners.Add(new XunitTraceListener(_output));

            using (ExpressionService service = new ExpressionService(ExpressionFeatures.None, trace))
            {
                Type[] types = values?.Select(v => v.GetType()).ToArray() ?? Type.EmptyTypes;
                object actual = service.Evaluate(expression, parameterNames, types, values);
                Assert.Equal(expected, actual);
            }   
        }
    }
}