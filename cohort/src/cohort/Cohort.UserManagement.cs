using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Web;
using cohort.Models;
using depot;

namespace cohort
{
    public static partial class Cohort
    {
        private const string UserKey = "__Cohort__User__";
        private const string ProfileKey = "__Cohort__Profile__";

        public static User User
        {
            get
            {
                var key = GetUserScopedKeyFor(UserKey);
                if (key == null) return null;

                // Need to be sure of all invalidation paths here (or always update memory to match DB record?)
                var user = Depot.ObjectCache.Get(key, LoadUser);
                return user;
            }
        }

        public static Profile Profile
        {
            get
            {
                var key = GetUserScopedKeyFor(ProfileKey);
                if (key == null) return null;

                var profile = Depot.ObjectCache.Get(key, LoadProfile);
                return profile;
            }
        }

        private static User LoadUser()
        {
            var repository = Container.Resolve<IUserRepository>();
            if (repository == null) return null;
            User user = null;
            if (Site.LocalAuth.Enabled && Site.LocalAuth.AutoRegister && HttpContext.Current.Request.IsLocal)
            {
                user = SignInLocal();
            }
            if (user == null)
            {
                var principal = HttpContext.Current.User;
                if (principal == null)
                {
                    return null;
                }
                var identity = principal.Identity;
                if (identity.IsAuthenticated)
                {
                    user = repository.GetByEmail(identity.Name);
                }
            }
            if(user != null)
            {
                user.Roles = Roles.GetRolesForUser(user.Email);
            }
            return user;
        }

        private static Profile LoadProfile()
        {
            var repository = Container.Resolve<IProfileRepository>();
            if (repository == null) return null;
            var key = User == null ? HttpContext.Current.Request.AnonymousID : User.Email;
            var profile = repository.GetByKey(key) ?? new Profile
            {
                Key = key,
                Theme = "Default"
            };
            return profile;
        }

        private static string GetUserScopedKeyFor(string baseKey)
        {
            if (HttpContext.Current == null) return null;
            var principal = HttpContext.Current.User;
            if (principal == null) return null;
            var key = string.Concat(baseKey, principal.Identity.Name);
            return key;
        }

        internal static User SignInLocal()
        {
            User user = null;
            var repository = Container.Resolve<IUserRepository>();
            var identity = HttpContext.Current.User.Identity;
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && windowsIdentity.IsAuthenticated)
            {
                user = ResolveUserByWindowsIdentity(repository, windowsIdentity);
                if (!identity.IsAuthenticated)
                {
                    var auth = Container.Resolve<IAuthenticationService>();
                    auth.SignIn(user.Email, false);
                }
            }
            return user;
        }

        private static User ResolveUserByWindowsIdentity(IUserRepository repository, WindowsIdentity windowsIdentity)
        {
            // 1. User is already registered by Windows identity
            var user = repository.GetByUsername(windowsIdentity.Name);
            if (user == null)
            {
                // 2. User is already registered, but not associated with a Windows identity    
                var email = UserPrincipal.Current.EmailAddress;
                user = repository.GetByEmail(email);
                if (user == null)
                {
                    // 3. User is not registered, but is authenticated
                    user = repository.CreateFromWindowsIdentity(windowsIdentity);
                }
                else
                {
                    // Associate registered user with this identity
                    user.Username = windowsIdentity.Name;
                    user.IsActivated = true;
                }
                repository.Save(user);
            }
            return user;
        }
    }
}
