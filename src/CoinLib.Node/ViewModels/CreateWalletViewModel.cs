using System.ComponentModel.DataAnnotations;

namespace CoinLib.ViewModels
{
    public class CreateWalletViewModel
    {
        [Required]
        public string Password { get; set; }
    }
}
