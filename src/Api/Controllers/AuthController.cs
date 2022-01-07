using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
