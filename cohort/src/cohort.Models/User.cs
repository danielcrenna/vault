using System;
using TableDescriptor;
using cohort.Configuration;

namespace cohort.Models
{
    public class User
    {
        private string[] _roles;
        
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ReferrerUrl { get; set; }
        public string LandingPageUrl { get; set; }
        public string Culture { get; set; }
        public string IPAddress { get; set; }
        public bool IsActivated { get; set; }
        public DateTime? JoinedOn { get; set; }
        public DateTime? SignedInOn { get; set; }
        public DateTime? SignedOutOn { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Transient]
        public string Identity
        {
            get { return Username ?? Email; }
        }

        [Transient]
        public string[] Roles
        {
            get
            {
                if (_roles == null && !string.IsNullOrWhiteSpace(Email))
                {
                    // Better would just be an inner join in a repo query with a multi-map...
                    _roles = Config.Container.Resolve<IRoleRepository>().GetRolesForUser(Email);
                }
                return _roles;
            }
            set { _roles = value; }
        }

        [Transient]
        public bool IsOnline
        {
            get
            {
                return SignedInOn.HasValue && !SignedOutOn.HasValue;
            }
        }
    }
}