using Microsoft.AspNetCore.Mvc;

namespace ReferenceBot.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return RedirectPermanent("/swagger/index.html");
        }
    }
}
