using System.ComponentModel.DataAnnotations;

namespace NaiveCoin.ViewModels
{
    public class CreateWalletViewModel
    {
        [Required]
        public string Password { get; set; }
    }
}
