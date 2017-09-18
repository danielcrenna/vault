using System;
using NaiveCoin.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Tests
{
    public class WhenObjectIsHashed : IClassFixture<ObjectHashProviderFixture>
    {
        private readonly ObjectHashProviderFixture _provider;

        public WhenObjectIsHashed(ObjectHashProviderFixture provider)
        {
            _provider = provider;
        }

        [Fact]
        public void Property_order_doesnt_matter()
        {
            var foo = new { Foo = "A", Bar = "B" };
            var bar = new { Bar = "B", Foo = "A" };

            Assert.Equal
            (
                _provider.Value.ComputeHash(foo), 
                _provider.Value.ComputeHash(bar)
            );
        }


        [Fact]
        public void Properties_that_dont_equal_have_different_hashes()
        {
            var foo = new { Foo = "A", Bar = "A" };
            var bar = new { Bar = "B", Foo = "A" };

            Assert.NotEqual
            (
                _provider.Value.ComputeHash(foo),
                _provider.Value.ComputeHash(bar)
            );
        }


        [Fact]
        public void Properties_with_no_value_dont_matter()
        {
            var foo = new Stub  { A = "A", B = "B" };
            var bar = new Stub  { B = "B", A = "A", C = null };

            Assert.Equal
            (
                _provider.Value.ComputeHash(foo),
                _provider.Value.ComputeHash(bar)
            );
        }
        
        private struct Stub
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
        }
    }

    public static class Inline
    {
        public static string Template(Func<string> func)
        {
            return func();
        }
    }
}


namespace NaiveCoin.Types
{
    public class Type_a441c2cfd233eefe04f93ea5
    {
        public string Property_983d { get; set; }
        public string Property_06b84b7832 { get; set; }
        public string Property_4b3d7c6943 { get; set; }

    }
}
