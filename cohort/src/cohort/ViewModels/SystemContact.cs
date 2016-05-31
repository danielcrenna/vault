using System.ComponentModel.DataAnnotations;

namespace cohort.ViewModels
{
    public class SystemContact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [FormName("subject")]
        public string ContactSubject { get; set; }
        
        [Required, DataType(DataType.EmailAddress)]
        [FormName("email")]
        public string ContactEmail { get; set; }

        [Required]
        [FormName("message")]
        public string ContactMessage { get; set; }
    }
}