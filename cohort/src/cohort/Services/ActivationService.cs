using System;
using System.Web.Security;
using cohort.Extensions;
using cohort.Models;

namespace cohort.Services
{
    public class ActivationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IActivationRepository _activationRepository;

        public ActivationService(IUserRepository userRepository, IAuthenticationService authenticationService, IActivationRepository activationRepository)
        {
            _userRepository = userRepository;
            _authenticationService = authenticationService;
            _activationRepository = activationRepository;
        }

        public string StoreAuthenticationTicketForActivation(string email)
        {
            var expiry = TimeSpan.FromDays(Cohort.Site.Membership.ActivationDays);
            return StoreAuthenticationTicket(expiry, email, "activation");
        }

        public string StoreAuthenticationTicketForReset(string email)
        {
            var expiry = TimeSpan.FromDays(Cohort.Site.Membership.PasswordResetDays);
            return StoreAuthenticationTicket(expiry, email, "reset");
        }

        private string StoreAuthenticationTicket(TimeSpan expiry, string email, string tag)
        {
            var issueDate = DateTime.UtcNow;
            var expiryDate = issueDate.Add(expiry);

            var ticket = _authenticationService.Encrypt(
                new FormsAuthenticationTicket(1, email, issueDate, expiryDate, false, tag)
            );

            var hash = Guid.NewGuid().ToString().MD5();
            var activation = new Activation
            {
                Hash = hash,
                Ticket = ticket
            };

            _activationRepository.Save(activation);
            return hash;
        }

        public bool ActivateAccount(string hash)
        {
            return ActivateByHashAndTicketFunction(hash, ticket =>
            {
                var email = ticket.Name;
                var user = _userRepository.GetByEmail(email);
                if (!user.IsActivated)
                {
                    user.IsActivated = true;
                    _userRepository.Save(user);
                    return true;
                }
                return false;
            });
        }

        public User ResetPassword(string hash)
        {
            User user = null;

            ActivateByHashAndTicketFunction(hash, ticket =>
            {
                var email = ticket.Name;
                user = _userRepository.GetByEmail(email);
                return user != null;
            });

            return user;
        }

        private bool ActivateByHashAndTicketFunction(string hash, Func<FormsAuthenticationTicket, bool> getActivated)
        {
            var activated = false;
            var activation = _activationRepository.FindByHash(hash);
            if (activation != null)
            {
                var ticket = _authenticationService.Decrypt(activation.Ticket);
                if (ticket != null)
                {
                    if (!ticket.Expired)
                    {
                        activated = getActivated.Invoke(ticket);
                    }
                    _activationRepository.Delete(activation);
                }
            }

            return activated;
        }
    }
}