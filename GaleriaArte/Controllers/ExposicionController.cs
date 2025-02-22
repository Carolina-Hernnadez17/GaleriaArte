using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Net;

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

                string query = "SELECT * FROM exposicion";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exposiciones.Add(new exposicion
                        {
                            id_exposicion = reader.GetInt32("id_exposicion"),
                            id_locacion = reader.GetInt32("id_locacion"),
                            id_obra = reader.GetInt32("id_obra"),
                            titulo_exposicion = reader.GetString("titulo_exposicion"),
                            descripcion = reader["descripcion"] != DBNull.Value ? reader.GetString("descripcion") : "",
                            fecha_inicio = reader.GetDateTime("fecha_inicio"),
                            fecha_cierre = reader.GetDateTime("fecha_cierre")
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
            ViewBag.Obras = ObtenerObras();
            return View();
        }

        [HttpPost]
        public IActionResult Agregar(int id_locacion, int id_obra, string titulo_exposicion, string descripcion, DateTime fecha_inicio, DateTime fecha_cierre)
        {
            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                string query = @"INSERT INTO exposicion (id_locacion, id_obra, titulo_exposicion, descripcion, fecha_inicio, fecha_cierre) 
                                 VALUES (@id_locacion, @id_obra, @titulo_exposicion, @descripcion, @fecha_inicio, @fecha_cierre)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_locacion", id_locacion);
                    cmd.Parameters.AddWithValue("@id_obra", id_obra);
                    cmd.Parameters.AddWithValue("@titulo_exposicion", titulo_exposicion);
                    cmd.Parameters.AddWithValue("@descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@fecha_inicio", fecha_inicio);
                    cmd.Parameters.AddWithValue("@fecha_cierre", fecha_cierre);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("exposicion_admin");
        }

        // Endpoint para editar una exposición
        [HttpGet]
        public IActionResult Editar(int id)
        {
            exposicion exposicion = null;

            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                string query = "SELECT * FROM exposicion WHERE id_exposicion = @id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            exposicion = new exposicion
                            {
                                id_exposicion = reader.GetInt32("id_exposicion"),
                                id_locacion = reader.GetInt32("id_locacion"),
                                id_obra = reader.GetInt32("id_obra"),
                                titulo_exposicion = reader.GetString("titulo_exposicion"),
                                descripcion = reader["descripcion"] != DBNull.Value ? reader.GetString("descripcion") : "",
                                fecha_inicio = reader.GetDateTime("fecha_inicio"),
                                fecha_cierre = reader.GetDateTime("fecha_cierre")
                            };
                        }
                    }
                }
            }

            if (exposicion == null)
                return NotFound();

            return View(exposicion);
        }

        [HttpPost]
        public IActionResult Editar(exposicion exposicion)
        {
            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                string query = @"UPDATE exposicion 
                                 SET id_locacion = @id_locacion, id_obra = @id_obra, titulo_exposicion = @titulo_exposicion, 
                                     descripcion = @descripcion, fecha_inicio = @fecha_inicio, fecha_cierre = @fecha_cierre 
                                 WHERE id_exposicion = @id_exposicion";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_exposicion", exposicion.id_exposicion);
                    cmd.Parameters.AddWithValue("@id_locacion", exposicion.id_locacion);
                    cmd.Parameters.AddWithValue("@id_obra", exposicion.id_obra);
                    cmd.Parameters.AddWithValue("@titulo_exposicion", exposicion.titulo_exposicion);
                    cmd.Parameters.AddWithValue("@descripcion", exposicion.descripcion);
                    cmd.Parameters.AddWithValue("@fecha_inicio", exposicion.fecha_inicio);
                    cmd.Parameters.AddWithValue("@fecha_cierre", exposicion.fecha_cierre);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("exposicion_admin");
        }

        // Endpoint para eliminar una exposición
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                string query = "DELETE FROM exposicion WHERE id_exposicion = @id";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("exposicion_admin");
        }

        private List<locacion> ObtenerLocaciones()
        {
            List<locacion> lista = new List<locacion>();

            using (var conn = _conexionGaleria.AbrirConexion())
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                string query = "SELECT id_locacion, ciudad FROM locacion";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new locacion
                        {
                            id_Locacion = Convert.ToInt32(reader["id_locacion"]),
                            ciudad = reader["ciudad"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        private List<obra> ObtenerObras()
        {
            var lista = new List<obra>();
            var conn = _conexionGaleria.AbrirConexion();

            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                string query = "SELECT id_obra, titulo FROM obra";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var obraTemp = new obra
                        {
                            id_obra = Convert.ToInt32(reader["id_obra"]),
                            titulo = reader["titulo"].ToString()
                        };

                        Console.WriteLine($"Obra encontrada: {obraTemp.id_obra} - {obraTemp.titulo}");

                        lista.Add(obraTemp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerObras: {ex.Message}");
            }

            return lista;
        }




    }
}
