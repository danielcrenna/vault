namespace cohort
{
    public interface IRoleRepository
    {
        bool IsUserInRole(string email, string roleName);
        string[] GetRolesForUser(string email);
        void CreateRole(string roleName);
        bool DeleteRole(string roleName, bool throwOnPopulatedRole);
        bool RoleExists(string roleName);
        void AddUsersToRoles(string[] emails, string[] roleNames);
        void RemoveUsersFromRoles(string[] emails, string[] roleNames);
        string[] GetUsersInRole(string roleName);
        string[] GetAllRoles();
        string[] FindUsersInRole(string roleName, string usernameToMatch);
    }
}