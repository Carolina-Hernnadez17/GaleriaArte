using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.Diagnostics;

namespace GaleriaArte.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public ActionResult Index_usuario()
        {
            return View();
        }


        public ActionResult Index()
        {
            ConexionGallery conexion = new ConexionGallery();
            ViewBag.Mensaje = conexion.ProbarConexion();
            return View();
        }

        public ActionResult Privacy() 
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
