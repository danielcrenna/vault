using System.Linq;
using Dapper;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class ProfileRepository : IProfileRepository
    {
        public void Save(Profile profile)
        {
            var db = UnitOfWork.Current;
            if(profile.Id == 0)
            {
                db.Insert(profile);
            }
            else
            {
                db.Update(profile);
            }
        }

        public Profile GetByKey(string key)
        {
            var db = UnitOfWork.Current;
            var user = db.Query<Profile>("SELECT * FROM Profile WHERE Key = @Key", new { Key = key }).SingleOrDefault();
            return user;
        }
    }
}