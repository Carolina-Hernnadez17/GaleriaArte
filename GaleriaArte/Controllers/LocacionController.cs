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
                            longitud = reader["longitud"].ToString(),
                            descripcion = reader["descripcion"].ToString()
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

        public ActionResult agregar()
        {
            return View("locacion");
        }

        [HttpPost]
        public ActionResult AgregarLocacion(locacion locacion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    

                    string queryInsertar = "INSERT INTO locacion (ciudad, direccion, latitud, longitud, descripcion) VALUES (@ciudad, @direccion, @latitud, @longitud, @descripcion)";
                    MySqlCommand cmdInsertar = new MySqlCommand(queryInsertar, conn);
                    cmdInsertar.Parameters.AddWithValue("@ciudad", locacion.ciudad);
                    cmdInsertar.Parameters.AddWithValue("@direccion", locacion.direccion);
                    cmdInsertar.Parameters.AddWithValue("@latitud", locacion.latitud);
                    cmdInsertar.Parameters.AddWithValue("@longitud", locacion.longitud);
                    cmdInsertar.Parameters.AddWithValue("@descripcion", locacion.descripcion);
                    cmdInsertar.ExecuteNonQuery();
                }

                TempData["Exito"] = "La ubicacion se ha agregado correctamente.";
                return RedirectToAction("Locacion_Admin");
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) 
                {
                    TempData["Error"] = "Ya existe una ubicacion con la misma ciudad y direccion.";
                }
                else
                {
                    TempData["Error"] = "Error al agregar la ubicacion: " + ex.Message;
                }
                return RedirectToAction("Locacion_Admin");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al agregar la ubicacion: " + ex.Message;
                return RedirectToAction("Locacion_Admin");
            }
        }

        public ActionResult editar()
        {
            return View("locacion");
        }


        [HttpPost]
        public ActionResult EditarLocacion(locacion locacion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "UPDATE locacion SET ciudad = @ciudad, direccion = @direccion," +
                        "           latitud = @latitud, longitud = @longitud , descripcion = @descripcion " +
                                    "WHERE id_locacion = @id\r\n";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    
                    cmd.Parameters.AddWithValue("@ciudad", locacion.ciudad);
                    cmd.Parameters.AddWithValue("@direccion", locacion.direccion);
                    cmd.Parameters.AddWithValue("@latitud", locacion.latitud);
                    cmd.Parameters.AddWithValue("@longitud", locacion.longitud);
                    cmd.Parameters.AddWithValue("@id", locacion.id_Locacion);
                    cmd.Parameters.AddWithValue("@descripcion", locacion.descripcion);
                    cmd.ExecuteNonQuery();
                }
                TempData["Exito"] = "La ubicacion se actualizo correctamente!";
                return RedirectToAction("Locacion_Admin");
                
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al editar locacion: " + ex.Message;
                return RedirectToAction("Locacion_Admin");
            }

            
        }

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
                TempData["Exito"] = "La ubicación se elimino correctamente";
                return RedirectToAction("Locacion_Admin");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminara locacion: " + ex.Message;
                return RedirectToAction("Locacion_Admin");
            }
        }
    }
}