using System.Collections.Generic;
using cohort.Models;
using Dapper;
using tophat;

namespace cohort.Sqlite
{
    public class ContactListRepository : IContactListRepository
    {
        public IEnumerable<Contact> GetAll()
        {
            var db = UnitOfWork.Current;
            return db.Query<Contact>("SELECT * FROM Contact");
        }
    }
}