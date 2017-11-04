using System.ComponentModel.DataAnnotations;

namespace CoinLib.ViewModels
{
    public class CreateTransactionViewModel
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string FromAddress { get; set; }

        [Required]
        public string ToAddress { get; set; }

        [Required]
        public long Amount { get; set; }

        public string ChangeAddress { get; set; }
    }
}