using System;
using System.Linq;
using Dapper;
using tophat;
using tuxedo.Dapper;

namespace cohort.Sqlite
{
    public class RoleRepository : IRoleRepository
    {
        public bool IsUserInRole(string email, string roleName)
        {
            var user = UnitOfWork.Current.Query<string>("SELECT u.Email FROM User u, Role r, UserRole ur WHERE ur.RoleId = r.Id AND ur.UserId = u.Id AND r.Description = @roleName AND u.Email = @email", new { email, roleName }).SingleOrDefault();
            return email.Equals(user);
        }

        public string[] GetRolesForUser(string email)
        {
            var users = UnitOfWork.Current.Query<string>("SELECT r.Description FROM User u, Role r, UserRole ur WHERE ur.RoleId = r.Id AND ur.UserId = u.Id AND u.Email = @Email", new { Email = email }).ToArray();
            return users;
        }

        public void CreateRole(string roleName)
        {
            UnitOfWork.Current.Insert(new Role { Description = roleName });
        }

        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (!throwOnPopulatedRole)
            {
                var deleted = UnitOfWork.Current.Query<long>("DELETE FROM Role WHERE Description = @Description", new { Description = roleName }).Single();
                return deleted == 1;
            }
            if (GetUsersInRole(roleName).Any())
            {
                throw new Exception("Role has users associated with it");
            }
            return DeleteRole(roleName, false);
        }

        public bool RoleExists(string roleName)
        {
            return UnitOfWork.Current.Query("SELECT Description FROM Role WHERE Description = @Description", new { Description = roleName }).SingleOrDefault() != null;
        }

        public void AddUsersToRoles(string[] emails, string[] roleNames)
        {
            foreach (var email in emails)
            {
                foreach (var role in roleNames)
                {
                    var userId = UnitOfWork.Current.Query<long>("SELECT Id FROM User WHERE Email = @Email", new { Email = email }).SingleOrDefault();
                    if (userId == default(long))
                    {
                        continue;
                    }
                    var roleId = UnitOfWork.Current.Query<long>("SELECT Id FROM Role WHERE Description = @Description", new { Description = role }).SingleOrDefault();
                    if (roleId == default(long))
                    {
                        CreateRole(role);
                        roleId = UnitOfWork.Current.Query<long>("SELECT Id FROM Role WHERE Description = @Description", new { Description = role }).SingleOrDefault();
                    }
                    UnitOfWork.Current.Execute("INSERT INTO UserRole (UserId, RoleId) VALUES (@UserId, @RoleId)", new { UserId = userId, RoleId = roleId });
                }
            }
        }

        public void RemoveUsersFromRoles(string[] emails, string[] roleNames)
        {
            foreach (var email in emails)
            {
                foreach (var role in roleNames)
                {
                    var userId = UnitOfWork.Current.Query<long>("SELECT Id FROM User WHERE Email = @Email", new { Email = email }).SingleOrDefault();
                    if (userId == default(long))
                    {
                        throw new Exception("User not found");
                    }
                    var roleId = UnitOfWork.Current.Query<long>("SELECT Id FROM Role WHERE Description = @Description", new { Description = role }).SingleOrDefault();
                    if (roleId != default(long))
                    {
                        UnitOfWork.Current.Query("DELETE FROM UserRole WHERE UserId = @UserId AND RoleId = @RoleId", new { UserId = userId, RoleId = roleId });
                    }
                }
            }
        }

        public string[] GetUsersInRole(string roleName)
        {
            var users = UnitOfWork.Current.Query<string>("SELECT u.Email FROM User u, Role r, UserRole ur WHERE ur.RoleId = r.Id AND ur.UserId = u.Id AND d.Description = @roleName", new { roleName }).ToArray();
            return users;
        }

        public string[] GetAllRoles()
        {
            var roles = UnitOfWork.Current.Query<string>("SELECT Description FROM Role").ToArray();
            return roles;
        }

        public string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            var users =
                UnitOfWork.Current.Query<string>(
                    "SELECT u.Email FROM User u, Role r, UserRole ur WHERE ur.RoleId = d.Id AND ur.UserId = u.Id AND r.Description = @roleName AND u.Email LIKE '%@email%'",
                    new { email = usernameToMatch, roleName }).ToArray();
            return users;
        }
    }
}