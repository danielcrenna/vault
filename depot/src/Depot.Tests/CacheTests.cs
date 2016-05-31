using System.IO;
using System.Threading;
using depot.Tests.Extensions;
using NUnit.Framework;

namespace depot.Tests
{
    public abstract class CacheTests
    {
        protected ICache Cache;
        protected IFileCacheDependency FileCacheDependency;

        [Test]
        public virtual void Is_null_when_cache_missed()
        {
            Assert.IsNull(Cache.Get("unknown_untyped_key"), "unknown_untyped_key");
            Assert.IsNull(Cache.Get<string>("unknown_reference_typed_key"), "unknown_reference_typed_key");
            Assert.AreEqual(default(int), Cache.Get<int>("unknown_value_typed_key"), "unknown_value_typed_key");
        }

        [Test]
        public virtual void Can_get_with_add_function()
        {
            const string key = "Can_get_with_add_function";
            Cache.Get(key, () => "value");
            Assert.AreEqual(Cache.Get<string>(key), "value");
        }

        [Test]
        public virtual void Can_cache_with_absolute_expiry()
        {
            const string key = "Can_cache_with_absolute_expiry";
            Cache.Set(key, "value", 1.Seconds().FromNow());
            Thread.Sleep(2.Seconds());
            Assert.IsNull(Cache.Get<string>(key), "Cache didn't expire in time");
        }

        [Test]
        public virtual void Can_cache_with_sliding_expiry()
        {
            const string key = "Can_cache_with_sliding_expiry";
            Cache.Set(key, "value", 1.Seconds());
            Thread.Sleep(2.Seconds());
            Assert.IsNull(Cache.Get<string>(key), "Cache didn't expire in time");
        }

        [Test]
        public virtual void Can_set_and_override()
        {
            const string key = "Can_set_and_override";
            Cache.Set(key, "value");
            Cache.Set(key, "value2");
            Assert.AreEqual(Cache.Get<string>(key), "value2", "Cache did not override value when setting");
        }

        [Test]
        public virtual void Can_add_and_skip()
        {
            const string key = "Can_add_and_skip";
            Cache.Add(key, "value");
            Cache.Add(key, "value2");
            Assert.AreEqual(Cache.Get<string>(key), "value", "Cache did not skip value when adding");
        }

        [Test]
        public virtual void Can_cache_with_file_dependency()
        {
            const string file = "Can_cache_with_file_dependency";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            File.WriteAllText(file, "value");

            var directoryName = new FileInfo(file).DirectoryName;
            Assert.IsNotNull(directoryName);
            
            var dependencyPath = Path.Combine(directoryName, file);
            Assert.IsTrue(File.Exists(dependencyPath));

            var dependency = FileCacheDependency.Create(dependencyPath);
            Cache.Add("key", "value", dependency);
            
            Assert.AreEqual("value", Cache.Get<string>("key"), "Cache did not store original content");
            File.Delete(file);

            Thread.Sleep(500);  // Otherwise, cache hit comes too fast on the heels of invalidating it

            File.WriteAllText(file, "value2");
            Assert.AreEqual(null, Cache.Get<string>("key"), "Cache did not invalidate when file changed");
        }
    }
}