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
                            Id_Locacion = Convert.ToInt32(reader["id_locacion"]),
                            Ciudad = reader["ciudad"].ToString(),
                            Direccion = reader["direccion"].ToString()
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
        public ActionResult Crear()
        {
            return View();
        }

        // Método para procesar la creación
        [HttpPost]
        public ActionResult Crear(locacion locacion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "INSERT INTO locacion (ciudad, direccion) VALUES (@ciudad, @direccion)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ciudad", locacion.Ciudad);
                    cmd.Parameters.AddWithValue("@direccion", locacion.Direccion);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Locacion_Admin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al agregar locación: " + ex.Message;
                return View();
            }
        }

        // Método para mostrar el formulario de edición
        public ActionResult Editar(int id)
        {
            locacion locacion = null;

            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "SELECT * FROM locacion WHERE id_locacion = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        locacion = new locacion
                        {
                            Id_Locacion = Convert.ToInt32(reader["id_locacion"]),
                            Ciudad = reader["ciudad"].ToString(),
                            Direccion = reader["direccion"].ToString()
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener locación: " + ex.Message;
            }

            return View(locacion);
        }

        // Método para procesar la edición
        [HttpPost]
        public ActionResult Editar(locacion locacion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "UPDATE locacion SET ciudad = @ciudad, direccion = @direccion WHERE id_locacion = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ciudad", locacion.Ciudad);
                    cmd.Parameters.AddWithValue("@direccion", locacion.Direccion);
                    cmd.Parameters.AddWithValue("@id", locacion.Id_Locacion);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Locacion_Admin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al editar locación: " + ex.Message;
                return View();
            }
        }

        // Método para eliminar
        public ActionResult Eliminar(int id)
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
