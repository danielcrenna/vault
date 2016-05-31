using System.Collections.Generic;
using System.Security.Principal;

namespace cohort.Models
{
    public interface IUserRepository
    {
        void Save(User user);
        User GetByEmail(string email, bool activatedOnly = false);
        User GetByUsername(string username, bool activatedOnly = false);
        User GetByIdentity(string usernameOrEmail, bool activatedOnly = false);
        User CreateFromEmail(string email, string password = null, bool activate = true);
        User CreateFromUsername(string email, string password = null, bool activate = true);
        User CreateFromWindowsIdentity(WindowsIdentity identity);
        IEnumerable<User> GetAll();
    }


}