using cohort.Models;

namespace cohort.Services
{
    public class RoleService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public RoleService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        private string _superUserEmail;
        public string SuperUserEmail
        {
            get { return _superUserEmail; }
            set
            {
                _superUserEmail = value;
                EnsureRoleExists(SuperUserRole);
                EnsureSuperUserExists();
            }
        }

        private string _adminRole;
        public string AdminRole
        {
            get { return _adminRole; }
            set
            {
                _adminRole = value;
                EnsureRoleExists(AdminRole);
            }
        }

        private void EnsureRoleExists(string role)
        {
            if (_roleRepository == null)
            {
                return;
            }
            if (!(_roleRepository.RoleExists(role)))
            {
                _roleRepository.CreateRole(role);
            }
        }

        public string SuperUserRole { get; set; }
        public string SuperUserPassword { get; set; }

        private void EnsureSuperUserExists()
        {
            if (_roleRepository == null || _userRepository == null || string.IsNullOrWhiteSpace(SuperUserRole) || string.IsNullOrWhiteSpace(SuperUserEmail))
            {
                return;
            }
            var user = _userRepository.GetByEmail(SuperUserEmail);
            if (user == null)
            {
                user = _userRepository.CreateFromEmail(SuperUserEmail, SuperUserPassword);
                _userRepository.Save(user);
            }
            if (_roleRepository.IsUserInRole(user.Email, SuperUserRole))
            {
                return;
            }
            _roleRepository.AddUsersToRoles(new[] { user.Email }, new[] { SuperUserRole });
        }
    }
}