using System.ComponentModel.DataAnnotations;

namespace cohort.ViewModels
{
    public class ResetPassword
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}