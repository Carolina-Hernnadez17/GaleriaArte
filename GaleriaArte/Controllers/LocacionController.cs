using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace GaleriaArte.Controllers
{
    public class locacionController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        private ConexionGallery conexion = new ConexionGallery();

        // Método para listar las locaciones
        public ActionResult Locacion_Admin()
        {
            List<locacion> lista = new List<locacion>();

            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "SELECT * FROM locacion";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new locacion
                        {
                            id_Locacion = Convert.ToInt32(reader["id_locacion"]),
                            ciudad = reader["ciudad"].ToString(),
                            direccion = reader["direccion"].ToString(),
                            latitud = reader["latitud"].ToString(),
                            longitud = reader["longitud"].ToString()
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener locaciones: " + ex.Message;
            }

            return View(lista);
        }

        // Método para mostrar el formulario de agregar
        public ActionResult agregar()
        {
            return View("locacion");
        }

        // Método para procesar la creación
        [HttpPost]
        public ActionResult AgregarLocacion(locacion locacion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "INSERT INTO locacion (ciudad, direccion, latitud, longitud) VALUES (@ciudad, @direccion, @latitud, @longitud)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ciudad", locacion.ciudad);
                    cmd.Parameters.AddWithValue("@direccion", locacion.direccion);
                    cmd.Parameters.AddWithValue("@latitud", locacion.latitud);
                    cmd.Parameters.AddWithValue("@longitud", locacion.longitud);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("locacion_Admin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al agregar locación: " + ex.Message;
                return View();
            }


        }

        // Método para mostrar el formulario de edición

        public ActionResult editar()
        {
            return View("locacion");
        }

        // Método para procesar la creación
        [HttpPost]
        public ActionResult EditarLocacion(locacion locacion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "UPDATE locacion SET ciudad = @ciudad, direccion = @direccion, latitud = @latitud, longitud = @longitud " +
                                    "WHERE id_locacion = @id\r\n";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    
                    cmd.Parameters.AddWithValue("@ciudad", locacion.ciudad);
                    cmd.Parameters.AddWithValue("@direccion", locacion.direccion);
                    cmd.Parameters.AddWithValue("@latitud", locacion.latitud);
                    cmd.Parameters.AddWithValue("@longitud", locacion.longitud);
                    cmd.Parameters.AddWithValue("@id", locacion.id_Locacion);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("locacion_Admin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al editar locación: " + ex.Message;
                return RedirectToAction("locacion_Admin");
            }

            
        }


        // Método para eliminar
        public ActionResult EliminarLocacion(int id)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "DELETE FROM locacion WHERE id_locacion = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Locacion_Admin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al eliminar locación: " + ex.Message;
                return RedirectToAction("Locacion_Admin");
            }
        }
    }
}