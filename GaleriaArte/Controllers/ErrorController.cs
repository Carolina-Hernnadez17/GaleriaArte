using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GaleriaArte.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Index(string mensaje = null)
        {
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            ViewBag.Error = mensaje;
            return View(errorModel);
        }
    }
}
