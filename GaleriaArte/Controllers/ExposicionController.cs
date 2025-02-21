using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Net;

namespace GaleriaArte.Controllers
{
    public class ExposicionController : Controller
    {
        private ConexionGallery conexion = new ConexionGallery();
        
        public ActionResult exposiciones_admin()
        {
            List<exposicion> lista = new List<exposicion>();

            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = @"SELECT e.id_exposicion, e.titulo_exposicion, e.descripcion, e.fecha_inicio, e.fecha_cierre,
                                            l.ciudad, l.direccion,
                                            o.titulo AS titulo_obra, o.nombre_artista
                                     FROM expocision e
                                     JOIN locacion l ON e.id_locacion = l.id_locacion
                                     JOIN obra o ON e.id_obra = o.id_obra";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new exposicion
                        {
                            id_exposicion = Convert.ToInt32(reader["id_exposicion"]),
                            titulo_exposicion = reader["titulo_exposicion"].ToString(),
                            descripcion = reader["descripcion"].ToString(),
                            fecha_inicio = Convert.ToDateTime(reader["fecha_inicio"]),
                            fecha_cierre = Convert.ToDateTime(reader["fecha_cierre"])
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener exposiciones: " + ex.Message;
            }

            return View(lista);
        }

        // Método para mostrar el formulario de agregar
        public ActionResult Agregar()
        {
            CargarDatosRelacionados();
            return View("Exposicion");
        }

        // Procesar la creación de una exposición
        [HttpPost]
        public ActionResult AgregarExposicion(exposicion exposicion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = @"INSERT INTO expocision (id_locacion, id_obra, titulo_exposicion, descripcion, fecha_inicio, fecha_cierre)
                                     VALUES (@id_locacion, @id_obra, @titulo_exposicion, @descripcion, @fecha_inicio, @fecha_cierre)";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_locacion", exposicion.id_locacion);
                    cmd.Parameters.AddWithValue("@id_obra", exposicion.id_obra);
                    cmd.Parameters.AddWithValue("@titulo_exposicion", exposicion.titulo_exposicion);
                    cmd.Parameters.AddWithValue("@descripcion", exposicion.descripcion);
                    cmd.Parameters.AddWithValue("@fecha_inicio", exposicion.fecha_inicio);
                    cmd.Parameters.AddWithValue("@fecha_cierre", exposicion.fecha_cierre);

                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Exposiciones_Admin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al agregar exposición: " + ex.Message;
                CargarDatosRelacionados();
                return View("Exposicion", exposicion);
            }
        }

        // Método para mostrar el formulario de edición
        public ActionResult Editar(int id)
        {
            exposicion exposicion = null;
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "SELECT * FROM expocision WHERE id_exposicion = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        exposicion = new exposicion
                        {
                            id_exposicion = Convert.ToInt32(reader["id_exposicion"]),
                            id_locacion = Convert.ToInt32(reader["id_locacion"]),
                            id_obra = Convert.ToInt32(reader["id_obra"]),
                            titulo_exposicion = reader["titulo_exposicion"].ToString(),
                            descripcion = reader["descripcion"].ToString(),
                            fecha_inicio = Convert.ToDateTime(reader["fecha_inicio"]),
                            fecha_cierre = Convert.ToDateTime(reader["fecha_cierre"])
                        };
                    }
                    reader.Close();
                }
                CargarDatosRelacionados();
                return View("Exposicion", exposicion);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener exposición: " + ex.Message;
                return RedirectToAction("Exposiciones_Admin");
            }
        }

        // Procesar la edición de una exposición
        [HttpPost]
        public ActionResult EditarExposicion(exposicion exposicion)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = @"UPDATE expocision SET id_locacion = @id_locacion, id_obra = @id_obra, 
                                     titulo_exposicion = @titulo_exposicion, descripcion = @descripcion, 
                                     fecha_inicio = @fecha_inicio, fecha_cierre = @fecha_cierre 
                                     WHERE id_exposicion = @id_exposicion";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_locacion", exposicion.id_locacion);
                    cmd.Parameters.AddWithValue("@id_obra", exposicion.id_obra);
                    cmd.Parameters.AddWithValue("@titulo_exposicion", exposicion.titulo_exposicion);
                    cmd.Parameters.AddWithValue("@descripcion", exposicion.descripcion);
                    cmd.Parameters.AddWithValue("@fecha_inicio", exposicion.fecha_inicio);
                    cmd.Parameters.AddWithValue("@fecha_cierre", exposicion.fecha_cierre);
                    cmd.Parameters.AddWithValue("@id_exposicion", exposicion.id_exposicion);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Exposiciones_Admin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al editar exposición: " + ex.Message;
                CargarDatosRelacionados();
                return View("Exposicion", exposicion);
            }
        }

        // Método para eliminar exposición
        public ActionResult EliminarExposicion(int id)
        {
            try
            {
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "DELETE FROM expocision WHERE id_exposicion = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Exposiciones_Admin");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al eliminar exposición: " + ex.Message;
                return RedirectToAction("Exposiciones_Admin");
            }
        }

        // Método para cargar locaciones y obras
        private void CargarDatosRelacionados()
        {
            using (var conn = conexion.AbrirConexion())
            {
                var locaciones = new List<locacion>();
                var obras = new List<obra>();

                MySqlCommand cmdLocacion = new MySqlCommand("SELECT * FROM locacion", conn);
                var readerLoc = cmdLocacion.ExecuteReader();
                while (readerLoc.Read())
                {
                    locaciones.Add(new locacion
                    {
                        id_Locacion = Convert.ToInt32(readerLoc["id_locacion"]),
                        ciudad = readerLoc["ciudad"].ToString(),
                        direccion = readerLoc["direccion"].ToString()
                    });
                }
                readerLoc.Close();

                MySqlCommand cmdObra = new MySqlCommand("SELECT * FROM obra", conn);
                var readerObra = cmdObra.ExecuteReader();
                while (readerObra.Read())
                {
                    obras.Add(new obra
                    {
                        id_obra = Convert.ToInt32(readerObra["id_obra"]),
                        titulo = readerObra["titulo"].ToString(),
                        nombre_artista = readerObra["nombre_artista"].ToString()
                    });
                }
                readerObra.Close();

                ViewBag.Locaciones = locaciones;
                ViewBag.Obras = obras;
            }
        }

    }
}
