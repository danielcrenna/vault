using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinLib.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        [HttpGet, Route("")]
        public IActionResult Index()
        {
            return Redirect("/swagger");
        }
    }
}