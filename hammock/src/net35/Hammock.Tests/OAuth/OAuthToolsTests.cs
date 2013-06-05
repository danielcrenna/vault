using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hammock.Authentication.OAuth;
using Hammock.Web;
using RestCore.Tests.Helpers;
using NUnit.Framework;

namespace Hammock.Tests.OAuth
{
    [TestFixture]
    public class OAuthToolsTests
    {
        [Test]
        public void Can_construct_http_request_url()
        {
            const string expected = "http://example.com/resource";
            var input = "HTTP://Example.com:80/resource?id=123".AsUri();
            var actual = OAuthTools.ConstructRequestUrl(input);

            Assert.AreEqual(expected, actual);
            Console.WriteLine(actual);
        }

        [Test]
        public void Can_construct_https_request_url()
        {
            const string expected = "https://example.com/resource";
            var input = "HTTPS://Example.com:443/resource?id=123".AsUri();
            var actual = OAuthTools.ConstructRequestUrl(input);

            Assert.AreEqual(expected, actual);
            Console.WriteLine(actual);
        }

        [Test]
        public void Can_construct_non_standard_port_http_request_url()
        {
            const string expected = "http://example.com:8080/resource";
            var input = "HTTP://Example.com:8080/resource?id=123".AsUri();
            var actual = OAuthTools.ConstructRequestUrl(input);

            Assert.AreEqual(expected, actual);
            Console.WriteLine(actual);
        }

        [Test]
        public void Can_construct_non_standard_port_https_request_url()
        {
            const string expected = "https://example.com:8080/resource";
            var input = "HTTPS://Example.com:8080/resource?id=123".AsUri();
            var actual = OAuthTools.ConstructRequestUrl(input);

            Assert.AreEqual(expected, actual);
            Console.WriteLine(actual);
        }

        [Test]
        public void Can_generate_nonce()
        {
            var nonce = OAuthTools.GetNonce();

            Assert.IsNotNull(nonce);
            Assert.IsTrue(nonce.Length == 16);
            Console.WriteLine(nonce);

            var next = OAuthTools.GetNonce();

            Assert.AreNotEqual(nonce, next);
            Console.WriteLine(next);
        }

        [Test]
        public void Can_generate_timestamp()
        {
            var timestamp = OAuthTools.GetTimestamp();
            Assert.IsNotNull(timestamp);
            Assert.IsTrue(timestamp.Length == 10, "What century is this?");
            Console.WriteLine(timestamp);
        }

        [Test]
        public void Can_guarantee_random_nonces_in_succession()
        {
            var nonces = new List<string>();
            for (var i = 0; i < 10000; i++)
            {
                var nonce = OAuthTools.GetNonce();
                var timestamp = DateTime.Now;

                Console.WriteLine(nonce + ":" + timestamp);

                if (nonces.Contains(nonce))
                {
                    Assert.Fail("non-unique nonce seed generated");
                }
                else
                {
                    nonces.Add(nonce);
                }
            }
        }
#if !Smartphone
        [Test]
        public void Can_guarantee_random_nonces_in_succession_multithreaded()
        {
            const int threads = 16;
            const int totalNonces = 30000;
            const int noncesPerThread = totalNonces / threads;
            var nonces = new List<string>();
            var noncesLock = new object();
            var dupes = new List<string>();
            var dupesLock = new object();
            var sem = new Semaphore(0, threads);
            var ts = new ThreadStart(() =>
                                         {
                                             sem.WaitOne();
                                             try
                                             {
                                                 var localNonces = new List<string>();
                                                 for (var i = 0; i < noncesPerThread; i++)
                                                 {
                                                     var nonce = OAuthTools.GetNonce();
                                                     localNonces.Add(nonce);
                                                 }
                                                 lock (nonces)
                                                 {
                                                     var localDupes = from s in nonces 
                                                                      where localNonces.Contains(s) 
                                                                      select s;
                                                     if (localDupes.Any())
                                                     {
                                                         lock(dupesLock)
                                                         {
                                                             dupes.AddRange(localDupes);
                                                         }
                                                     }
                                                     nonces.AddRange(localNonces.Except(localDupes));
                                                 }
                                             }
                                             finally
                                             {
                                                 sem.Release();
                                             }
                                         });
            var workerThreads = new Thread[threads];
            for (var i = 0; i < threads; i++)
            {
                workerThreads[i] = new Thread(ts) { IsBackground = false, Name = "thread" + i };
                workerThreads[i].Start();
            }

            sem.Release(threads);
            foreach (var t in workerThreads)
            {
                t.Join();
            }
            Assert.IsEmpty(dupes, "Found {0} duplicated nonces generated during test", dupes.Count);
            lock (noncesLock)
            {
                Assert.AreEqual(totalNonces, nonces.Count);
            }
        }
#endif

        [Test]
        public void Can_sort_and_normalize_parameters()
        {
            var input = new WebParameterCollection
                            {
                                {"a", "1"},
                                {"f", "50"},
                                {"f", "25"},
                                {"z", "t"},
                                {"f", "a"},
                                {"c", "hi there"},
                                {"z", "p"},
                            };

            const string expected = "a=1&c=hi%20there&f=25&f=50&f=a&z=p&z=t";
            var actual = OAuthTools.NormalizeRequestParameters(input);
            Console.WriteLine(actual);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_sort_and_normalize_parameters_excluding_signature()
        {
            var input = new WebParameterCollection
                            {
                                {"a", "1"},
                                {"f", "50"},
                                {"f", "25"},
                                {"z", "t"},
                                {"oauth_signature", "signature"},
                                {"f", "a"},
                                {"c", "hi there"},
                                {"z", "p"},
                            };

            const string expected = "a=1&c=hi%20there&f=25&f=50&f=a&z=p&z=t";
            var actual = OAuthTools.NormalizeRequestParameters(input);
            Console.WriteLine(actual);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_url_encode_with_uppercase_hexadecimals()
        {
            const string expected = "What%20century%20is%20this%3F";
            var actual = OAuthTools.UrlEncodeRelaxed("What century is this?");

            Assert.AreEqual(expected, actual);
            Console.WriteLine(actual);
        }

        [Test]
        public void Can_strict_url_encode_complex_string()
        {
            const string expected = "%21%3F%22%3B%3A%3C%3E%5C%5C%7C%60%23%24%25%5E%26%2A%2B-_%7B%7D%5B%5D";
            const string sequence = @"!?"";:<>\\|`#$%^&*+-_{}[]";

            var actual = OAuthTools.UrlEncodeStrict(sequence);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_relax_url_encode_complex_string()
        {
            // Doesn't URL encode ! or * in this sequence
            const string expected = "!%3F%22%3B%3A%3C%3E%5C%5C%7C%60%23%24%25%5E%26*%2B-_%7B%7D%5B%5D";
            const string sequence = @"!?"";:<>\\|`#$%^&*+-_{}[]";

            var actual = OAuthTools.UrlEncodeRelaxed(sequence);
            Assert.AreEqual(expected, actual);
        }
    }
}