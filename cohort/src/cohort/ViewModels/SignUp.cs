using System.ComponentModel.DataAnnotations;

namespace cohort.ViewModels
{
    public class SignUp
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        
        public string Username { get; set; }

        [Required, FormName("confirm_password")]
        public string PasswordAgain { get; set; }

        [FormName("landing_page")]
        public string LandingPage { get; set; }

        [FormName("referer_url")]
        public string RefererUrl { get; set; }
    }
}
