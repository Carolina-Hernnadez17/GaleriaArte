using Microsoft.AspNetCore.Mvc;

namespace GaleriaArte.Controllers
{
    public class ObrasArteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
