using System.ComponentModel.DataAnnotations;

namespace cohort.ViewModels
{
    public class SignIn
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool RememberMe { get; set; }
    }
}