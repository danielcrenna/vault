using System.Web.Security;

namespace cohort
{
    public class CohortRoleProvider : RoleProvider
    {
        private readonly IRoleRepository _repository;

        public CohortRoleProvider()
        {
            _repository = Cohort.Container.Resolve<IRoleRepository>();
        }

        public override bool IsUserInRole(string email, string roleName)
        {
            return _repository.IsUserInRole(email, roleName);
        }

        public override string[] GetRolesForUser(string email)
        {
            return _repository.GetRolesForUser(email);
        }

        public override void CreateRole(string roleName)
        {
            _repository.CreateRole(roleName);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            return _repository.DeleteRole(roleName, throwOnPopulatedRole);
        }

        public override bool RoleExists(string roleName)
        {
            return _repository.RoleExists(roleName);
        }

        public override void AddUsersToRoles(string[] emails, string[] roleNames)
        {
            _repository.AddUsersToRoles(emails, roleNames);
        }

        public override void RemoveUsersFromRoles(string[] emails, string[] roleNames)
        {
            _repository.RemoveUsersFromRoles(emails, roleNames);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return _repository.GetUsersInRole(roleName);
        }

        public override string[] GetAllRoles()
        {
            return _repository.GetAllRoles();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return _repository.FindUsersInRole(roleName, usernameToMatch);
        }

        public override string ApplicationName { get; set; }
    }
}