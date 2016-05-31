using System;
using NUnit.Framework;

namespace container.Tests
{
    [TestFixture]
    public class DefaultContainerTests
    {
        [Test]
        public void Removals_dont_cause_exceptions()
        {
            Assert.DoesNotThrow(() =>
            {
                var container = new DefaultContainer();

                container.Register<IFoo>(r => new Foo());
                Assert.IsTrue(container.Remove<IFoo>());
                Assert.IsFalse(container.Remove<IFoo>());

                container.Register<IFoo>("MyFoo", r=> new Foo());
                Assert.IsTrue(container.Remove<IFoo>("MyFoo"));
                Assert.IsFalse(container.Remove<IFoo>("VitaminFail"));
            });
        }

        [Test]
        public void Can_resolve_returns_true_for_arbitrary_type()
        {
            var container = new Container();
            container.Register<IFoo>(r => new Foo());
            Assert.IsTrue(container.CanResolve<Bar>());
        }

        [Test]
        public void Can_resolve_returns_true_for_arbitrary_untyped_type()
        {
            var container = new Container();
            container.Register(typeof(IFoo), r => new Foo());
            Assert.IsTrue(container.CanResolve<Bar>());
        }

        [Test]
        public void Can_resolve_untyped_type()
        {
            var container = new Container();
            container.Register(typeof(IFoo), r => new Foo());
            Assert.IsNotNull(container.Resolve<Bar>());
        }

        [Test]
        public void Null_resolutions_do_not_result_in_exceptions()
        {
            // Simple case of missing dependency
            var container = new Container();
            var foo = container.Resolve<IFoo>();
            Assert.IsNull(foo);

            // Advanced case of missing inner dependency on registered type
            container = new Container();
            container.Register(r => new Bar(r.Resolve<IFoo>()));
            var bar = container.Resolve<Bar>();
            Assert.IsNotNull(bar);
            Assert.IsNull(bar.Foo);
        }

        public interface IFoo
        {

        }
        public class Foo : IFoo
        {

        }
        public class Bar
        {
            public IFoo Foo { get; private set; }

            public Bar(IFoo foo)
            {
                Foo = foo;
            }

            public void Baz()
            {
                Console.WriteLine(Foo);
            }
        }
    }
}
