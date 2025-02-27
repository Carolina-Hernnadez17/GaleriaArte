using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using GaleriaArte.Models;

namespace GaleriaArte.Controllers
{
    public class HistorialController : Controller
    {
        private readonly ConexionGallery _conexionGaleria;

        // Usando el constructor normal para la inyección de dependencias
        public HistorialController(ConexionGallery conexionGaleria)
        {
            _conexionGaleria = conexionGaleria;
        }

        [HttpGet]
        public IActionResult Historial()
        {
            var historial = ObtenerHistorialExposiciones();
            return View(historial);
        }

        private List<historial_exposicion> ObtenerHistorialExposiciones()
        {
            var historial = new List<historial_exposicion>();

            // Intentar instanciar manualmente ConexionGallery
            var conexionManual = new ConexionGallery();
            using (var conn = conexionManual.AbrirConexion())
            {
                string query = "SELECT id_historial, id_exposicion, usuario_modificacion, fecha_modificacion, detalles FROM historial_exposicion ORDER BY fecha_modificacion DESC";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        historial.Add(new historial_exposicion
                        {
                            id_historial = reader.GetInt32("id_historial"),
                            id_exposicion = reader.GetInt32("id_exposicion"),
                            usuario_modificacion = reader.GetString("usuario_modificacion"),
                            fecha_modificacion = reader.GetDateTime("fecha_modificacion"),
                            detalles = reader.GetString("detalles")
                        });
                    }
                }
            }

            return historial;
        }
    }
}
