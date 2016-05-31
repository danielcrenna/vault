using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dapper;
using NUnit.Framework;
using tophat;
using tuxedo;

namespace bulky.Tests.Fixtures
{
    public class BulkCopyFixture
    {
        public static Dialect Dialect { get; set; }
        
        public static void BulkCopyUsers(int trials, bool trace = false)
        {
            var users = ResetUsers(trials);
            var sw = Stopwatch.StartNew();
            UnitOfWork.Current.BulkCopy(users);
            var elapsed = sw.Elapsed;
            var count = AssertInsertCount(users.Count, elapsed, trace);
            if (trace)
            {
                Console.WriteLine("Inserting {0} records took {1}", count, elapsed);
            }
        }

        public static List<User> ResetUsers(int trials)
        {
            var sql = "DELETE FROM " + QualifiedUser();
            UnitOfWork.Current.Execute(sql);
            var users = GetInsertCollection(trials).ToList();
            return users;
        }

        public static IEnumerable<User> GetInsertCollection(int number)
        {
            for (var i = 1; i < number + 1; i++)
            {
                yield return new User { Email = String.Format("user{0}@email.com", i) };
            }
        }

        public static int AssertInsertCount(int expected, TimeSpan elapsed, bool trace = false)
        {
            int actual;
            try
            {
                actual = UnitOfWork.Current.Query<int>("SELECT COUNT(1) FROM " + QualifiedUser()).Single();
            }
            catch
            {
                // SQlite...
                actual = (int)UnitOfWork.Current.Query<long>("SELECT COUNT(1) FROM " + QualifiedUser()).Single();    
            }
            
            Assert.AreEqual(expected, actual);
            return actual;
        }

        private static string QualifiedUser()
        {
            return Dialect.StartIdentifier + "User" + Dialect.EndIdentifier;
        }
     
    }
}