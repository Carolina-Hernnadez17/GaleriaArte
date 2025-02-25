using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Net;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace GaleriaArte.Controllers
{
    public class ExposicionController : Controller
    {
        private readonly ConexionGallery _conexionGaleria;

        public ExposicionController()
        {
            _conexionGaleria = new ConexionGallery();
        }

        // Endpoint que muestra todas las exposiciones
        public IActionResult exposicion_admin()
        {
            List<exposicion> exposiciones = new List<exposicion>();

            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                string query = "SELECT id_exposicion, id_locacion, titulo_exposicion, descripcion, fecha_inicio, fecha_cierre, estado FROM exposicion";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exposiciones.Add(new exposicion
                        {
                            id_exposicion = reader.GetInt32("id_exposicion"),
                            id_locacion = reader.GetInt32("id_locacion"),
                            titulo_exposicion = reader.GetString("titulo_exposicion"),
                            descripcion = reader["descripcion"] != DBNull.Value ? reader.GetString("descripcion") : "",
                            fecha_inicio = reader.GetDateTime("fecha_inicio"),
                            fecha_cierre = reader.GetDateTime("fecha_cierre"),
                            estado = reader.GetBoolean("estado")
                        });
                    }
                }
            }

            return View(exposiciones);
        }


        // Endpoint para agregar una nueva exposición
        [HttpGet]
        public IActionResult Agregar()
        {
            ViewBag.Locaciones = ObtenerLocaciones();
            return View();
        }

        [HttpPost]
        public IActionResult Agregar(exposicion exposicion)
        {
            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                string query = @"INSERT INTO exposicion (id_locacion, titulo_exposicion, descripcion, fecha_inicio, fecha_cierre, estado)
                         VALUES (@id_locacion, @titulo_exposicion, @descripcion, @fecha_inicio, @fecha_cierre, @estado)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_locacion", exposicion.id_locacion);
                    cmd.Parameters.AddWithValue("@titulo_exposicion", exposicion.titulo_exposicion);
                    cmd.Parameters.AddWithValue("@descripcion", exposicion.descripcion);
                    cmd.Parameters.AddWithValue("@fecha_inicio", exposicion.fecha_inicio);
                    cmd.Parameters.AddWithValue("@fecha_cierre", exposicion.fecha_cierre);
                    cmd.Parameters.AddWithValue("@estado", exposicion.estado);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("exposicion_admin");
        }

        // Endpoint de filtrado
        public IActionResult exposicion_admin_filtrada(string estadoFiltro, DateTime? fechaInicio, DateTime? fechaCierre)
        {

            List<exposicion> exposiciones = new List<exposicion>();

            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                string query = "SELECT id_exposicion, id_locacion, titulo_exposicion, descripcion, fecha_inicio, fecha_cierre, estado FROM exposicion WHERE 1=1";

                if (!string.IsNullOrEmpty(estadoFiltro))
                {
                    query += " AND estado = @estado";
                }
                if (fechaInicio.HasValue)
                {
                    query += " AND fecha_inicio >= @fechaInicio";
                }
                if (fechaCierre.HasValue)
                {
                    query += " AND fecha_cierre <= @fechaCierre";
                }

                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(estadoFiltro))
                    {
                        bool estado = estadoFiltro == "Activa";
                        cmd.Parameters.AddWithValue("@estado", estado);
                    }
                    if (fechaInicio.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.Value);
                    }
                    if (fechaCierre.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@fechaCierre", fechaCierre.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            exposiciones.Add(new exposicion
                            {
                                id_exposicion = reader.GetInt32("id_exposicion"),
                                id_locacion = reader.GetInt32("id_locacion"),
                                titulo_exposicion = reader.GetString("titulo_exposicion"),
                                descripcion = reader["descripcion"] != DBNull.Value ? reader.GetString("descripcion") : "",
                                fecha_inicio = reader.GetDateTime("fecha_inicio"),
                                fecha_cierre = reader.GetDateTime("fecha_cierre"),
                                estado = reader.GetBoolean("estado")
                            });
                        }
                    }
                }
            }

            return View("exposicion_admin", exposiciones);

        }


        // EndPoint cargar vista para editar una exposición
        [HttpGet]
        public ActionResult Editar(int id)
        {
            exposicion exposicion = null;
            // Usamos la conexión proporcionada por ConexionGallery
            using (var connection = _conexionGaleria.AbrirConexion())
            {
                string query = "SELECT * FROM exposicion WHERE id_exposicion = @id_exposicion";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_exposicion", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            exposicion = new exposicion
                            {
                                id_exposicion = reader.GetInt32("id_exposicion"),
                                titulo_exposicion = reader.GetString("titulo_exposicion"),
                                descripcion = reader.GetString("descripcion"),
                                fecha_inicio = reader.GetDateTime("fecha_inicio"),
                                fecha_cierre = reader.GetDateTime("fecha_cierre"),
                                id_locacion = reader.GetInt32("id_locacion"),
                                estado = reader.GetBoolean("estado")
                            };
                        }
                    }
                }
            }

            if (exposicion == null)
            {
                return NotFound();
            }

            // Obtener las locaciones para el desplegable
            List<locacion> locaciones = ObtenerLocaciones();
            ViewBag.Locaciones = locaciones;

            return View(exposicion);
        }

        [HttpPost]
        public IActionResult Editar(exposicion exposicion)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Locaciones = ObtenerLocaciones();
                return View(exposicion);
            }

            try
            {
                using (var conn = _conexionGaleria.AbrirConexion())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    string queryCheck = "SELECT fecha_inicio FROM exposicion WHERE id_exposicion = @id_exposicion";
                    using (var checkCmd = new MySqlCommand(queryCheck, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id_exposicion", exposicion.id_exposicion);
                        object fechaInicioObj = checkCmd.ExecuteScalar();
                        if (fechaInicioObj != null && Convert.ToDateTime(fechaInicioObj) <= DateTime.Now)
                        {
                            TempData["Error"] = "No se puede modificar una exposición que ya ha iniciado.";
                            return RedirectToAction("exposicion_admin");
                        }
                    }

                    string queryUpdate = "UPDATE exposicion SET " +
                                         "titulo_exposicion = @titulo_exposicion, " +
                                         "descripcion = @descripcion, " +
                                         "fecha_inicio = @fecha_inicio, " +
                                         "fecha_cierre = @fecha_cierre, " +
                                         "id_locacion = @id_locacion, " +
                                         "estado = @estado " +
                                         "WHERE id_exposicion = @id_exposicion";
                    using (var cmd = new MySqlCommand(queryUpdate, conn))
                    {
                        cmd.Parameters.AddWithValue("@titulo_exposicion", exposicion.titulo_exposicion);
                        cmd.Parameters.AddWithValue("@descripcion", exposicion.descripcion);
                        cmd.Parameters.AddWithValue("@fecha_inicio", exposicion.fecha_inicio);
                        cmd.Parameters.AddWithValue("@fecha_cierre", exposicion.fecha_cierre);
                        cmd.Parameters.AddWithValue("@id_locacion", exposicion.id_locacion);
                        cmd.Parameters.AddWithValue("@estado", exposicion.estado);
                        cmd.Parameters.AddWithValue("@id_exposicion", exposicion.id_exposicion);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("Rows affected: " + rowsAffected);
                        if (rowsAffected > 0)
                        {
                            string queryHistorial = "INSERT INTO historial_exposicion (id_exposicion, usuario_modificacion, fecha_modificacion, detalles) " +
                                                    "VALUES (@id_exposicion, @usuario_modificacion, NOW(), @detalles)";
                            using (var histCmd = new MySqlCommand(queryHistorial, conn))
                            {
                                histCmd.Parameters.AddWithValue("@id_exposicion", exposicion.id_exposicion);
                                histCmd.Parameters.AddWithValue("@usuario_modificacion", "admin");
                                histCmd.Parameters.AddWithValue("@detalles", "Se modificó la exposición");
                                histCmd.ExecuteNonQuery();
                            }
                            TempData["Success"] = "Exposición actualizada correctamente.";
                        }
                        else
                        {
                            TempData["Error"] = "No se encontraron cambios para actualizar.";
                        }
                    }
                    return RedirectToAction("exposicion_admin");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar la exposición: " + ex.Message;
                return RedirectToAction("exposicion_admin");
            }
        }




        //EndPoin para eliminar una exposicionaun
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID inválido.";
                return RedirectToAction("exposicion_admin");
            }

            try
            {
                using (var conn = _conexionGaleria.AbrirConexion())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    Console.WriteLine("Conectado a la base de datos. Eliminando exposición ID: " + id);

                    string queryEliminarRelacion = "DELETE FROM exposicion_obra WHERE id_exposicion = @id_exposicion";
                    using (var cmdRel = new MySqlCommand(queryEliminarRelacion, conn))
                    {
                        cmdRel.Parameters.AddWithValue("@id_exposicion", id);
                        cmdRel.ExecuteNonQuery();
                    }

                    string queryEliminar = "DELETE FROM exposicion WHERE id_exposicion = @id_exposicion";
                    using (var cmdEliminar = new MySqlCommand(queryEliminar, conn))
                    {
                        cmdEliminar.Parameters.AddWithValue("@id_exposicion", id);
                        int rowsAffected = cmdEliminar.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            TempData["Success"] = "Exposición eliminada correctamente.";
                            Console.WriteLine("Exposición eliminada correctamente.");
                        }
                        else
                        {
                            TempData["Error"] = "No se encontró la exposición o no se pudo eliminar.";
                            Console.WriteLine("No se encontró la exposición.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar la exposición: " + ex.Message;
                Console.WriteLine("Error al eliminar: " + ex.Message);
            }

            return RedirectToAction("exposicion_admin");
        }


        // EndPoint que muestra la vista para agregar obras
        [HttpGet]
        public IActionResult AgregarObra(int exposicionId)
        {
            try
            {
                var exposicion = ObtenerExposicionesPorId(exposicionId);
                if (exposicion == null || exposicion.estado == false)
                {
                    ViewBag.Error = "La exposición no existe o no está activa.";
                    return View("Error");
                }

                var obrasDisponibles = ObtenerObrasDisponibles();
                ViewBag.Obras = obrasDisponibles;

                var obrasEnExposicion = ObtenerObrasEnExposicion(exposicionId);
                ViewBag.ObrasEnExposicion = obrasEnExposicion;

                ViewBag.ExposicionId = exposicionId;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar la vista de agregar obra.";
                Console.WriteLine("Error: " + ex.Message);
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult GuardarObra(int exposicionId, int obraId)
        {
            try
            {
                using (var conexion = new ConexionGallery().AbrirConexion())
                {
                    string checkQuery = "SELECT COUNT(*) FROM exposicion_obra WHERE id_exposicion = @exposicionId AND id_obra = @obraId";
                    using (var checkCommand = new MySqlCommand(checkQuery, conexion))
                    {
                        checkCommand.Parameters.AddWithValue("@exposicionId", exposicionId);
                        checkCommand.Parameters.AddWithValue("@obraId", obraId);
                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                        if (count > 0)
                        {
                            ViewBag.Error = "Esta obra ya se ha agregado.";
                            return View("Error");
                        }
                    }

                    string query = "INSERT INTO exposicion_obra (id_exposicion, id_obra) VALUES (@exposicionId, @obraId)";
                    using (var command = new MySqlCommand(query, conexion))
                    {
                        command.Parameters.AddWithValue("@exposicionId", exposicionId);
                        command.Parameters.AddWithValue("@obraId", obraId);
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("AgregarObra", new { exposicionId });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al agregar la obra a la exposición.";
                Console.WriteLine("Error: " + ex.Message);
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult EliminarObraDeExposicion(int exposicionId, int obraId)
        {
            try
            {
                using (var conn = _conexionGaleria.AbrirConexion())
                {
                    string query = "DELETE FROM exposicion_obra WHERE id_exposicion = @exposicionId AND id_obra = @obraId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@exposicionId", exposicionId);
                        cmd.Parameters.AddWithValue("@obraId", obraId);
                        int filasAfectadas = cmd.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            TempData["MensajeExito"] = "Obra eliminada de la exposición exitosamente.";
                        }
                        else
                        {
                            TempData["MensajeError"] = "No se pudo eliminar la obra de la exposición.";
                        }
                    }
                }
                return RedirectToAction("AgregarObra", new { exposicionId = exposicionId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { mensaje = "Error al eliminar la obra de la exposición: " + ex.Message });
            }
        }



        // Métodos auxiliares
        public List<locacion> ObtenerLocaciones()
        {
            List<locacion> locaciones = new List<locacion>();

            var conexionGallery = new ConexionGallery();

            var connection = conexionGallery.AbrirConexion();

            string query = "SELECT * FROM locacion";

            try
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            locaciones.Add(new locacion
                            {
                                id_Locacion = reader.GetInt32("id_Locacion"),
                                ciudad = reader.GetString("ciudad")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener locaciones: " + ex.Message);
            }
            finally
            {
                conexionGallery.CerrarConexion();
            }

            return locaciones;
            
        }

        private exposicion ObtenerExposicionesPorId(int exposicionId)
        {
            using (var conexion = new ConexionGallery().AbrirConexion())
            {
                string query = "SELECT id_exposicion, titulo_exposicion, estado FROM exposicion WHERE id_exposicion = @exposicionId";
                using (var command = new MySqlCommand(query, conexion))
                {
                    command.Parameters.AddWithValue("@exposicionId", exposicionId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new exposicion
                            {
                                id_exposicion = reader.GetInt32("id_exposicion"),
                                titulo_exposicion = reader.GetString("titulo_exposicion"),
                                estado = reader.GetBoolean("estado")
                            };
                        }
                    }
                }
            }
            return null;
        }

        private List<obra> ObtenerObrasDisponibles()
        {
            var obras = new List<obra>();
            using (var conexion = new ConexionGallery().AbrirConexion())
            {
                string query = "SELECT id_obra, titulo FROM obra WHERE estado = 1";
                using (var command = new MySqlCommand(query, conexion))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            obras.Add(new obra
                            {
                                id_obra = reader.GetInt32("id_obra"),
                                titulo = reader.GetString("titulo")
                            });
                        }
                    }
                }
            }
            return obras;
        }

        private List<obra> ObtenerObrasEnExposicion(int exposicionId)
        {
            var obras = new List<obra>();
            using (var conexion = new ConexionGallery().AbrirConexion())
            {
                string query = @"
                SELECT o.id_obra, o.titulo 
                FROM obra o
                INNER JOIN exposicion_obra eo ON o.id_obra = eo.id_obra
                WHERE eo.id_exposicion = @exposicionId";

                using (var command = new MySqlCommand(query, conexion))
                {
                    command.Parameters.AddWithValue("@exposicionId", exposicionId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            obras.Add(new obra
                            {
                                id_obra = reader.GetInt32("id_obra"),
                                titulo = reader.GetString("titulo")
                            });
                        }
                    }
                }
            }
            return obras;
        }
    }
}
