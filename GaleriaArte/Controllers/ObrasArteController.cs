using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace GaleriaArte.Controllers
{
    public class ObrasArteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private ConexionGallery conexion = new ConexionGallery();
        public IActionResult GetObras()
        {
            List<obra> lista = new List<obra>();

            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "SELECT * FROM locacion";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                       
                        lista.Add(new obra
                        {
                            nombre_artista = reader["nombre_artista"].ToString(),
                            num_registro = reader["num_registro"].ToString(),
                            titulo = reader["titulo"].ToString(),
                            año_creacion = Convert.ToDateTime(reader["año_creacion"]),
                            precio = Convert.ToDecimal(reader["precio"]),
                            descripcion = reader["descripcion"].ToString(),
                            estado = Convert.ToBoolean(reader["estado"]) 
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
        public ActionResult agregarObra()
        {
            return View("locacion");
        }

        // Método para procesar la creación
        [HttpPost]
        public ActionResult AgregarObra(locacion locacion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "INSERT INTO obra (id_cliente, nombre_artista, num_registro, titulo, año_creacion, precio, descripcion, estado)" +
                                    " VALUES (@id_cliente, @nombre_artista, @num_registro, @titulo, @año_creacion, @precio, @descripcion, @estado)";
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

    }
}
