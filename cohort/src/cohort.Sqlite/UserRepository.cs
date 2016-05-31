using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Dapper;
using cohort.Models;
using cohort.Services;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class UserRepository : IUserRepository
    {
        private readonly ISecurityService _security;
        private readonly IRoleRepository _roles;

        public UserRepository(ISecurityService security, IRoleRepository roles)
        {
            _security = security;
            _roles = roles;
        }

        public void Save(User user)
        {
            var db = UnitOfWork.Current;
            var now = DateTime.Now;
            if (db.Query<User>("SELECT * FROM User WHERE Id = @Id", new {user.Id }).SingleOrDefault() != null)
            {
                db.Update<User>(new { EndDate = now }, new { user.Id });
            }
            user.Id = 0;
            user.StartDate = now;
            user.EndDate = null;
            db.Insert(user);
        }

        public IEnumerable<User> GetAll()
        {
            var db = UnitOfWork.Current;
            const string sql = "SELECT * FROM User WHERE EndDate IS NULL";
            var users = db.Query<User>(sql);
            return users;
        }
        
        public User GetByEmail(string email, bool activatedOnly = false)
        {
            var db = UnitOfWork.Current;
            var sql = "SELECT * FROM User WHERE Email = @Email AND EndDate IS NULL";
            if (activatedOnly) sql += " AND IsActivated = 1";

            var user = db.Query<User>(sql, new { Email = email }).SingleOrDefault();
            return user;
        }

        public User GetByUsername(string username, bool activatedOnly = false)
        {
            var db = UnitOfWork.Current;
            var sql = "SELECT * FROM User WHERE Username = @Username AND EndDate IS NULL";
            if (activatedOnly) sql += " AND IsActivated = 1";
            var user = db.Query<User>(sql, new { Username = username }).SingleOrDefault();
            return user;
        }

        public User GetByIdentity(string usernameOrEmail, bool activatedOnly = false)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail)) return null;
            return GetByUsername(usernameOrEmail, activatedOnly) ?? GetByEmail(usernameOrEmail, activatedOnly);
        }

        public User CreateFromEmail(string email, string password = null, bool activate = true)
        {
            var user = new User
            {
                Culture = Thread.CurrentThread.CurrentUICulture.Name,
                IsActivated = activate,
                Email = email,
                Password = _security.Hash(password ?? Guid.NewGuid().ToString()),
                JoinedOn = DateTime.Now
            };
            return user;
        }

        public User CreateFromUsername(string username, string password = null, bool activate = true)
        {
            var user = new User
            {
                Culture = Thread.CurrentThread.CurrentUICulture.Name,
                IsActivated = activate,
                Username = username,
                Password = _security.Hash(password ?? Guid.NewGuid().ToString()),
                JoinedOn = DateTime.Now
            };
            return user;
        }

        public User CreateFromWindowsIdentity(WindowsIdentity identity)
        {
            var user = new User
            {
                Username = identity.Name,
                Culture = Thread.CurrentThread.CurrentUICulture.Name,
                IsActivated = true,
                Email = UserPrincipal.Current.EmailAddress ?? "Unknown",
                Password = _security.Hash(Guid.NewGuid().ToString()),
                JoinedOn = DateTime.Now
            };
            return user;
        }
    }
}