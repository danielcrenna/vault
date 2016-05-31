using System;
using System.Linq;
using cohort.Models;
using Dapper;
using Paging;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class BrokenLinkRepository : IBrokenLinkRepository
    {
        public IPagedEnumerable<BrokenLink> GetPage(int? page, int? count)
        {
            var db = UnitOfWork.Current;
            var lower = count * page - 1 ;
            var offset = lower + count - 1;

            var total = db.Execute("SELECT COUNT(1) FROM BrokenLink");
            var links = db.Query<BrokenLink>("SELECT * FROM BrokenLink ORDER BY Count, LastOccurrence LIMIT @Count, @Offset", new { Count = count, Offset = offset });
            var result = new PagedEnumerable<BrokenLink>(links, page.Value, count.Value, total);
            return result;
        }

        public BrokenLink GetByPath(string path)
        {
            var db = UnitOfWork.Current;
            var user = db.Query<BrokenLink>("SELECT * FROM BrokenLink WHERE Path = @Path ORDER BY LastOccurrence", new { Path = path }).SingleOrDefault();
            return user;
        }

        public void Save(BrokenLink link)
        {
            if (!LinkIsValid(link)) return;
            var db = UnitOfWork.Current;
            var existing = GetByPath(link.Path);
            if(existing != null)
            {
                db.Update<BrokenLink>(new { existing.Path, Count = existing.Count + 1, LastOccurrence = DateTime.Now }, new { existing.Path });
            }
            else
            {
                db.Insert(link);
            }
        }

        public void Delete(BrokenLink link)
        {
            if (!LinkIsValid(link)) return;
            var db = UnitOfWork.Current;
            db.Delete<BrokenLink>(new { link.Path });
        }

        public void DeleteAll()
        {
            UnitOfWork.Current.DeleteAll<BrokenLink>();
        }

        private static bool LinkIsValid(BrokenLink link)
        {
            return link != null && !string.IsNullOrWhiteSpace(link.Path);
        }
    }
}