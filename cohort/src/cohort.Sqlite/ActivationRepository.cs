using System.Linq;
using Dapper;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class ActivationRepository : IActivationRepository
    {
        public Activation FindByHash(string hash)
        {
            var db = UnitOfWork.Current;
            var activation = db.Query<Activation>("SELECT * FROM Activation WHERE Hash = @Hash", new { Hash = hash }).SingleOrDefault();
            return activation;
        }

        public Activation FindById(int id)
        {
            var db = UnitOfWork.Current;
            var activation = db.Query<Activation>("SELECT * FROM Activation WHERE Id = @Id", new { Id = id }).SingleOrDefault();
            return activation;
        }

        public void Delete(Activation activation)
        {
            UnitOfWork.Current.Delete<Activation>(new { activation.Id });
        }

        public void Save(Activation activation)
        {
            var db = UnitOfWork.Current;
            var exists = FindById(activation.Id);
            if(exists == null)
            {
                db.Insert(activation);
            }
            else
            {
                db.Update(activation);
            }
        }
    }
}