using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Controllers
{
    public class ExposicionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
