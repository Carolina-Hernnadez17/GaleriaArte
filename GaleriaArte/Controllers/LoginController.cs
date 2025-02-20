using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
