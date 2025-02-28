using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var exposiciones = ObtenerExposicionesPublicadas();
        }

        public IActionResult exposicion_admin()
        {
            List<exposicion> exposiciones = new List<exposicion>();

            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                DateTime fechaActual = DateTime.Now;

                // Actualizar estados de exposiciones antes de mostrarlas
                string updateQuery = @"
            UPDATE exposicion 
            SET estado = 
                CASE 
                    WHEN fecha_inicio <= @fechaActual AND fecha_cierre >= @fechaActual THEN 'activo'
                    WHEN fecha_cierre < @fechaActual THEN 'finalizada'
                    ELSE estado
                END";

                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@fechaActual", fechaActual);
                    updateCmd.ExecuteNonQuery();
                }

                // Consulta para obtener las exposiciones con la cantidad de obras asociadas
                string query = @"
            SELECT e.id_exposicion, e.id_locacion, e.titulo_exposicion, e.descripcion, 
                   e.fecha_inicio, e.fecha_cierre, e.estado,
                   (SELECT COUNT(*) FROM exposicion_obra eo WHERE eo.id_exposicion = e.id_exposicion) AS cantidad_obras
            FROM exposicion e";

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
                            estado = reader.GetString("estado"),
                            CantidadObras = reader.GetInt32("cantidad_obras") // Agregar cantidad de obras a la instancia
                        });
                    }
                }
            }

            return View(exposiciones);
        }

        public IActionResult VerExposicion_user(int id)
        {
            exposicion exposicion = null;
            List<obra> obras = new List<obra>();

            using (MySqlConnection conexion = new ConexionGallery().AbrirConexion())
            {
                // Obtener la exposición por ID
                string queryExposicion = "SELECT * FROM exposicion WHERE id_exposicion = @id";
                using (MySqlCommand cmd = new MySqlCommand(queryExposicion, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            exposicion = new exposicion
                            {
                                id_exposicion = Convert.ToInt32(reader["id_exposicion"]),
                                titulo_exposicion = reader["titulo_exposicion"].ToString(),
                                descripcion = reader["descripcion"].ToString(),
                                fecha_inicio = Convert.ToDateTime(reader["fecha_inicio"]),
                                fecha_cierre = Convert.ToDateTime(reader["fecha_cierre"]),
                                estado = reader["estado"].ToString()
                            };
                        }
                    }
                }

                if (exposicion != null)
                {
                    // Obtener las obras asociadas a la exposición
                    string queryObras = @"
                SELECT o.* FROM obra o
                INNER JOIN exposicion_obra eo ON o.id_obra = eo.id_obra
                WHERE eo.id_exposicion = @id";

                    using (MySqlCommand cmdObras = new MySqlCommand(queryObras, conexion))
                    {
                        cmdObras.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader readerObras = cmdObras.ExecuteReader())
                        {
                            while (readerObras.Read())
                            {
                                obras.Add(new obra
                                {
                                    id_obra = Convert.ToInt32(readerObras["id_obra"]),
                                    titulo = readerObras["titulo"].ToString(),
                                    nombre_artista = readerObras["nombre_artista"].ToString()
                                });
                            }
                        }
                    }

                    exposicion.Obras = obras;
                }
            }

            if (exposicion == null)
            {
                return NotFound();
            }

            return View(exposicion);
        }



        public IActionResult VerExposicion(int id)
        {
            exposicion exposicion = null;
            List<obra> obras = new List<obra>();

            using (MySqlConnection conexion = new ConexionGallery().AbrirConexion())
            {
                // Obtener la exposición por ID
                string queryExposicion = "SELECT * FROM exposicion WHERE id_exposicion = @id";
                using (MySqlCommand cmd = new MySqlCommand(queryExposicion, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            exposicion = new exposicion
                            {
                                id_exposicion = Convert.ToInt32(reader["id_exposicion"]),
                                titulo_exposicion = reader["titulo_exposicion"].ToString(),
                                descripcion = reader["descripcion"].ToString(),
                                fecha_inicio = Convert.ToDateTime(reader["fecha_inicio"]),
                                fecha_cierre = Convert.ToDateTime(reader["fecha_cierre"]),
                                estado = reader["estado"].ToString()
                            };
                        }
                    }
                }

                if (exposicion != null)
                {
                    // Obtener las obras asociadas a la exposición
                    string queryObras = @"
                SELECT o.* FROM obra o
                INNER JOIN exposicion_obra eo ON o.id_obra = eo.id_obra
                WHERE eo.id_exposicion = @id";

                    using (MySqlCommand cmdObras = new MySqlCommand(queryObras, conexion))
                    {
                        cmdObras.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader readerObras = cmdObras.ExecuteReader())
                        {
                            while (readerObras.Read())
                            {
                                obras.Add(new obra
                                {
                                    id_obra = Convert.ToInt32(readerObras["id_obra"]),
                                    titulo = readerObras["titulo"].ToString(),
                                    nombre_artista = readerObras["nombre_artista"].ToString()
                                });
                            }
                        }
                    }

                    exposicion.Obras = obras;
                }
            }

            if (exposicion == null)
            {
                return NotFound();
            }

            return View(exposicion);
        }



        // Método para cambiar el estado de una exposición
        [HttpPost]
        public IActionResult CambiarEstado(int id_exposicion, string nuevoEstado)
        {
            using (var conexion = _conexionGaleria.AbrirConexion())
            {
                try
                {
                    if (conexion.State != System.Data.ConnectionState.Open) conexion.Open();

                    // Verificar si la exposición tiene al menos una obra
                    string consultaObras = "SELECT COUNT(*) FROM exposicion_obra WHERE id_exposicion = @id";
                    using (var cmd = new MySqlCommand(consultaObras, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", id_exposicion);
                        int cantidadObras = Convert.ToInt32(cmd.ExecuteScalar());

                        if (nuevoEstado.ToLower() == "activo" && cantidadObras == 0)
                        {
                            TempData["Error"] = "No puedes activar la exposición sin agregar al menos una obra.";
                            return RedirectToAction("exposicion_admin");
                        }
                    }

                    // Actualizar el estado de la exposición si la validación se pasó
                    string actualizarEstado = "UPDATE exposicion SET estado = @estado WHERE id_exposicion = @id";
                    using (var cmd = new MySqlCommand(actualizarEstado, conexion))
                    {
                        cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                        cmd.Parameters.AddWithValue("@id", id_exposicion);
                        cmd.ExecuteNonQuery();
                    }

                    TempData["Mensaje"] = "Estado actualizado correctamente.";
                    return RedirectToAction("exposicion_admin");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al cambiar el estado de la exposición.";
                    return RedirectToAction("exposicion_admin");
                }
            }
        }

        // Endpoint que muestra todas las exposiciones
        //public IActionResult exposicion_admin()
        //{
        //    List<exposicion> exposiciones = new List<exposicion>();

        //    using (var conn = _conexionGaleria.AbrirConexion())
        //    {
        //        if (conn.State != System.Data.ConnectionState.Open) conn.Open();

        //        // Obtener la fecha actual
        //        DateTime fechaActual = DateTime.Now;

        //        // Actualizar estados de exposiciones antes de mostrarlas
        //        string updateQuery = @"
        //            UPDATE exposicion 
        //            SET estado = 
        //                CASE 
        //                    WHEN fecha_inicio <= @fechaActual AND fecha_cierre >= @fechaActual THEN 'activo'
        //                    WHEN fecha_cierre < @fechaActual THEN 'finalizada'
        //                    ELSE estado
        //                END";

        //        using (var updateCmd = new MySqlCommand(updateQuery, conn))
        //        {
        //            updateCmd.Parameters.AddWithValue("@fechaActual", fechaActual);
        //            updateCmd.ExecuteNonQuery();
        //        }

        //        // Consultar las exposiciones actualizadas
        //        string query = "SELECT id_exposicion, id_locacion, titulo_exposicion, descripcion, fecha_inicio, fecha_cierre, estado FROM exposicion";

        //        using (var cmd = new MySqlCommand(query, conn))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                exposiciones.Add(new exposicion
        //                {
        //                    id_exposicion = reader.GetInt32("id_exposicion"),
        //                    id_locacion = reader.GetInt32("id_locacion"),
        //                    titulo_exposicion = reader.GetString("titulo_exposicion"),
        //                    descripcion = reader["descripcion"] != DBNull.Value ? reader.GetString("descripcion") : "",
        //                    fecha_inicio = reader.GetDateTime("fecha_inicio"),
        //                    fecha_cierre = reader.GetDateTime("fecha_cierre"),
        //                    estado = reader.GetString("estado")
        //                });
        //            }
        //        }
        //    }

        //    return View(exposiciones);
        //}


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

        //EndPoint de editar 
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var exposicion = ObtenerExposicionPorId(id);
            if (exposicion == null)
            {
                TempData["Error"] = "La exposición no existe.";
                return RedirectToAction("exposicion_admin");
            }

            // Cargar lista de locaciones
            ViewBag.Locaciones = new SelectList(ObtenerLocaciones(), "id_Locacion", "ciudad", exposicion.id_locacion);

            // Configurar lista de estados según reglas de negocio
            bool tieneObras = VerificarObrasAsociadas(id);
            bool puedeFinalizar = exposicion.fecha_cierre <= DateTime.Now;
            var estados = new List<SelectListItem>
            {
                new SelectListItem { Value = "programada", Text = "Programada", Selected = exposicion.estado.ToLower() == "programada" },
                new SelectListItem { Value = "activo", Text = "Activo", Disabled = !tieneObras, Selected = exposicion.estado.ToLower() == "activo" },
                new SelectListItem { Value = "finalizado", Text = "Finalizado", Disabled = !puedeFinalizar, Selected = exposicion.estado.ToLower() == "finalizado" }
            };
            ViewBag.Estados = estados;

            return View(exposicion);
        }

        // POST: Procesa la edición y actualiza la exposición en la base de datos
        [HttpPost]
        public IActionResult Editar(exposicion exposicion)
        {
            if (!ModelState.IsValid)
            {
                // Recargar listas en caso de error en el modelo
                ViewBag.Locaciones = new SelectList(ObtenerLocaciones(), "id_Locacion", "ciudad", exposicion.id_locacion);
                bool tieneObras = VerificarObrasAsociadas(exposicion.id_exposicion);
                bool puedeFinalizar = exposicion.fecha_cierre <= DateTime.Now;
                ViewBag.Estados = new List<SelectListItem>
                {
                    new SelectListItem { Value = "programada", Text = "Programada", Selected = exposicion.estado.ToLower() == "programada" },
                    new SelectListItem { Value = "activo", Text = "Activo", Disabled = !tieneObras, Selected = exposicion.estado.ToLower() == "activo" },
                    new SelectListItem { Value = "finalizado", Text = "Finalizado", Disabled = !puedeFinalizar, Selected = exposicion.estado.ToLower() == "finalizado" }
                };
                return View(exposicion);
            }

            try
            {
                using (var conn = _conexionGaleria.AbrirConexion())
                {
                    // Verificar que la exposición exista y no haya iniciado
                    string queryCheck = "SELECT fecha_inicio FROM exposicion WHERE id_exposicion = @id_exposicion";
                    using (var checkCmd = new MySqlCommand(queryCheck, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id_exposicion", exposicion.id_exposicion);
                        object result = checkCmd.ExecuteScalar();
                        if (result == null)
                        {
                            TempData["Error"] = "La exposición no existe.";
                            return RedirectToAction("exposicion_admin");
                        }
                        DateTime fechaInicioActual = Convert.ToDateTime(result);
                        if (fechaInicioActual <= DateTime.Now)
                        {
                            TempData["Error"] = "No se puede modificar una exposición que ya ha iniciado.";
                            return RedirectToAction("exposicion_admin");
                        }
                    }

                    // Validar que la fecha de cierre sea posterior a la de inicio
                    if (exposicion.fecha_cierre <= exposicion.fecha_inicio)
                    {
                        TempData["Error"] = "La fecha de cierre debe ser posterior a la fecha de inicio.";
                        ViewBag.Locaciones = new SelectList(ObtenerLocaciones(), "id_Locacion", "ciudad", exposicion.id_locacion);
                        bool tieneObras = VerificarObrasAsociadas(exposicion.id_exposicion);
                        bool puedeFinalizar = exposicion.fecha_cierre <= DateTime.Now;
                        ViewBag.Estados = new List<SelectListItem>
                        {
                            new SelectListItem { Value = "programada", Text = "Programada", Selected = exposicion.estado.ToLower() == "programada" },
                            new SelectListItem { Value = "activo", Text = "Activo", Disabled = !tieneObras, Selected = exposicion.estado.ToLower() == "activo" },
                            new SelectListItem { Value = "finalizado", Text = "Finalizado", Disabled = !puedeFinalizar, Selected = exposicion.estado.ToLower() == "finalizado" }
                        };
                        return View(exposicion);
                    }

                    // Validar que al cambiar el estado a "activo" existan obras asignadas
                    if (exposicion.estado.ToLower() == "activo" && !VerificarObrasAsociadas(exposicion.id_exposicion))
                    {
                        TempData["Error"] = "No se puede cambiar el estado a 'activo' sin tener obras asignadas.";
                        ViewBag.Locaciones = new SelectList(ObtenerLocaciones(), "id_Locacion", "ciudad", exposicion.id_locacion);
                        bool tieneObras = VerificarObrasAsociadas(exposicion.id_exposicion);
                        bool puedeFinalizar = exposicion.fecha_cierre <= DateTime.Now;
                        ViewBag.Estados = new List<SelectListItem>
                        {
                            new SelectListItem { Value = "programada", Text = "Programada", Selected = exposicion.estado.ToLower() == "programada" },
                            new SelectListItem { Value = "activo", Text = "Activo", Disabled = !tieneObras, Selected = exposicion.estado.ToLower() == "activo" },
                            new SelectListItem { Value = "finalizado", Text = "Finalizado", Disabled = !puedeFinalizar, Selected = exposicion.estado.ToLower() == "finalizado" }
                        };
                        return View(exposicion);
                    }
                    string usuarioModificacion = User.Identity.IsAuthenticated ? User.Identity.Name : "Administardor";

                    // Realizar el UPDATE en la base de datos
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
                        Console.WriteLine("Rows affected: " + rowsAffected); // Log para depuración

                        if (rowsAffected > 0)
                        {
                            // Registrar la actualización en historial
                            string queryHistorial = @"
                            INSERT INTO historial_exposicion (id_exposicion, usuario_modificacion, fecha_modificacion, detalles) 
                            VALUES (@id_exposicion, @usuario_modificacion, NOW(), @detalles)";

                            using (var histCmd = new MySqlCommand(queryHistorial, conn))
                            {
                                histCmd.Parameters.AddWithValue("@id_exposicion", exposicion.id_exposicion);
                                histCmd.Parameters.AddWithValue("@usuario_modificacion", usuarioModificacion);
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
                }
                return RedirectToAction("exposicion_admin");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar la exposición: " + ex.Message;
                return RedirectToAction("exposicion_admin");
            }
        }

        // EndPoint que muestra la vista para agregar obras
        [HttpGet]
        public IActionResult AgregarObra(int exposicionId)
        {
            try
            {
                var exposicion = ObtenerExposicionesPorId(exposicionId);
                // Se compara usando Trim() y ToLower() para asegurarse de que coincida con "programada"
                if (exposicion == null || exposicion.estado.Trim().ToLower() != "programada")
                {
                    ViewBag.Error = "La exposición no existe o no está en estado programada.";
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
                    string estadoQuery = "SELECT estado FROM exposicion WHERE id_exposicion = @exposicionId";
                    using (var estadoCommand = new MySqlCommand(estadoQuery, conexion))
                    {
                        estadoCommand.Parameters.AddWithValue("@exposicionId", exposicionId);
                        string estadoExposicion = estadoCommand.ExecuteScalar()?.ToString();
                        // Compara robustamente
                        if (estadoExposicion == null || estadoExposicion.Trim().ToLower() != "programada")
                        {
                            ViewBag.Error = "Solo se pueden agregar obras a exposiciones programadas.";
                            return View("Error");
                        }
                    }

                    string checkQuery = "SELECT COUNT(*) FROM exposicion_obra WHERE id_exposicion = @exposicionId AND id_obra = @obraId";
                    using (var checkCommand = new MySqlCommand(checkQuery, conexion))
                    {
                        checkCommand.Parameters.AddWithValue("@exposicionId", exposicionId);
                        checkCommand.Parameters.AddWithValue("@obraId", obraId);
                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            ViewBag.Error = "Esta obra ya se ha agregado a la exposición.";
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

        //EndPoin para eliminar una exposicion
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

                    string usuarioModificacion = User.Identity.IsAuthenticated ? User.Identity.Name : "Administardor";

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
                            // Registrar la eliminación en historial
                            string queryHistorial = @"
                        INSERT INTO historial_exposicion (id_exposicion, usuario_modificacion, fecha_modificacion, detalles) 
                        VALUES (@id_exposicion, @usuario_modificacion, NOW(), @detalles)";

                            using (var histCmd = new MySqlCommand(queryHistorial, conn))
                            {
                                histCmd.Parameters.AddWithValue("@id_exposicion", id);
                                histCmd.Parameters.AddWithValue("@usuario_modificacion", usuarioModificacion);
                                histCmd.Parameters.AddWithValue("@detalles", "Se eliminó la exposición");
                                histCmd.ExecuteNonQuery();
                            }

                            TempData["Success"] = "Exposición eliminada correctamente.";
                        }
                        else
                        {
                            TempData["Error"] = "No se encontró la exposición o no se pudo eliminar.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar la exposición: " + ex.Message;
            }

            return RedirectToAction("exposicion_admin");
        }

        [HttpPost]
        public IActionResult EliminarObraDeExposicion(int exposicionId, int obraId)
        {
            try
            {
                using (var conn = _conexionGaleria.AbrirConexion())
                {
                    string query = "DELETE FROM exposicion_obra WHERE id_exposicion = @exposicionId AND id_obra = @obraId";
                    using (var cmd = new MySqlCommand(query, conn))
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
                            TempData["MensajeError"] = "No se encontró la obra en la exposición.";
                        }
                    }
                }
                return RedirectToAction("AgregarObra", new { exposicionId = exposicionId });
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar la obra de la exposición: " + ex.Message;
                return RedirectToAction("AgregarObra", new { exposicionId = exposicionId });
            }
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
                                estado = reader.GetString("estado")
                            });
                        }
                    }
                }
            }

            return View("exposicion_admin", exposiciones);

        }


        //EndPoints para usuarios
        [HttpGet]
        public IActionResult exposicion_user()
        {
            List<exposicion> exposicionesActivas = new List<exposicion>();

            try
            {
                using (var conn = _conexionGaleria.AbrirConexion())
                {
                    string query = "SELECT id_exposicion, id_locacion, titulo_exposicion, descripcion, fecha_inicio, fecha_cierre, estado FROM exposicion WHERE estado = 'activo'";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                exposicionesActivas.Add(new exposicion
                                {
                                    id_exposicion = reader.GetInt32("id_exposicion"),
                                    id_locacion = reader.GetInt32("id_locacion"),
                                    titulo_exposicion = reader.GetString("titulo_exposicion"),
                                    descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? "" : reader.GetString("descripcion"),
                                    fecha_inicio = reader.GetDateTime("fecha_inicio"),
                                    fecha_cierre = reader.GetDateTime("fecha_cierre"),
                                    estado = reader.GetString("estado")
                                });
                            }
                        }
                    }
                }

                Dictionary<int, List<obra>> obrasPorExposicion = new Dictionary<int, List<obra>>();
                foreach (var expo in exposicionesActivas)
                {
                    obrasPorExposicion[expo.id_exposicion] = ObtenerObrasEnExposicion(expo.id_exposicion);
                }

                ViewBag.ExposicionesActivas = exposicionesActivas;
                ViewBag.ObrasPorExposicion = obrasPorExposicion;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al obtener las exposiciones: " + ex.Message;
            }

            return View();
        }



        // Métodos auxiliares



        private bool VerificarObrasAsociadas(int id_exposicion)
        {
            try
            {
                using (var conn = _conexionGaleria.AbrirConexion())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    string query = "SELECT COUNT(*) FROM exposicion_obra WHERE id_exposicion = @id_exposicion";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_exposicion", id_exposicion);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al verificar obras: " + ex.Message);
                return false;
            }
        }

        private exposicion ObtenerExposicionPorId(int id)
        {
            exposicion expo = null;
            using (var conexion = _conexionGaleria.AbrirConexion())
            {
                string query = "SELECT * FROM exposicion WHERE id_exposicion = @id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            expo = new exposicion
                            {
                                id_exposicion = reader.GetInt32("id_exposicion"),
                                titulo_exposicion = reader.GetString("titulo_exposicion"),
                                descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? "" : reader.GetString("descripcion"),
                                fecha_inicio = reader.GetDateTime("fecha_inicio"),
                                fecha_cierre = reader.GetDateTime("fecha_cierre"),
                                id_locacion = reader.GetInt32("id_locacion"),
                                estado = reader.GetString("estado")
                            };
                        }
                    }
                }
            }
            return expo;
        }



        private List<exposicion> ObtenerExposicionesPublicadas()
        {
            List<exposicion> exposiciones = new List<exposicion>();
            using (var conexion = new ConexionGallery().AbrirConexion())
            {
                string query = @"SELECT id_exposicion, titulo_exposicion, descripcion, fecha_inicio, fecha_cierre, estado 
                                 FROM exposicion 
                                 WHERE estado = 1
                                 ORDER BY fecha_inicio";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            exposiciones.Add(new exposicion
                            {
                                id_exposicion = reader.GetInt32("id_exposicion"),
                                titulo_exposicion = reader.GetString("titulo_exposicion"),
                                descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString("descripcion"),
                                fecha_inicio = reader.GetDateTime("fecha_inicio"),
                                fecha_cierre = reader.GetDateTime("fecha_cierre"),
                                estado = reader.GetString("estado")
                            });
                        }
                    }
                }
            }
            return exposiciones;
        }


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
                                estado = reader.GetString("estado")
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
