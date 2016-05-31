using System;
using System.Linq;
using Dapper;
using cohort.Models;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class TokenRepository : ITokenRepository
    {
        public void Save(Token token)
        {
            var db = UnitOfWork.Current;
            db.Insert(token);
        }

        public Token GetByValue(Guid value)
        {
            var db = UnitOfWork.Current;
            var token = db.Query<Token>("SELECT * FROM Token WHERE Value = @Value", new { Value = value}).SingleOrDefault();
            return token;
        }
    }
}