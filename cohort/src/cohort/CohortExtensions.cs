using System.Linq;
using cohort.Models;

namespace cohort
{
    public static class CohortExtensions
    {
        private const string RoleKey = "__Cohort__UserInRole";

        public static bool IsAuthenticated(this User user)
        {
            return user != null;
        }

        /// <summary>
        /// Determines whether the current <see cref="User" /> is granted a given role. 
        /// This check is request-
        /// Alternatively, you can install the <see cref="CohortRoleProvider" /> in Web.config and use
        /// the default <code>User.IsInRole(roleName)</code> method from your views:
        /// <example>
        ///     <system.web>
        ///         <roleManager defaultProvider="CohortRoleProvider" enabled="true" cacheRolesInCookie="true">
        ///             <providers>
        ///                 <clear />
        ///                 <add name="CohortRoleProvider" type="cohort.CohortRoleProvider" />
        ///             </providers>
        ///         </roleManager>
        ///     </system.web>
        /// </example>
        /// </summary>
        public static bool IsInRole(this User user, string roleName)
        {
            if(user == null || string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }
            if(user.Roles == null || user.Roles.Length == 0)
            {
                return false;
            }
            bool isInRole;
            var hash = user.Roles.ToDictionary(role => role, role => true);
            return hash.TryGetValue(roleName, out isInRole) && isInRole;
        }

        public static bool IsSuperUser(this User user)
        {
            var role = Cohort.Site.Auth.SuperUserRole;
            return !string.IsNullOrWhiteSpace(role) && IsInRole(user, Cohort.Site.Auth.SuperUserRole);
        }

        public static bool IsAdmin(this User user)
        {
            var role = Cohort.Site.Auth.AdminRole;
            var inRole = !string.IsNullOrWhiteSpace(role) && IsInRole(user, Cohort.Site.Auth.AdminRole);
            return inRole || IsSuperUser(user);
        }
    }
}