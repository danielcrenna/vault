using Paging;

namespace cohort.Models
{
    public interface IBrokenLinkRepository
    {
        IPagedEnumerable<BrokenLink> GetPage(int? page, int? count);
        BrokenLink GetByPath(string path);
        void Save(BrokenLink link);
        void Delete(BrokenLink link);
        void DeleteAll();
    }
}