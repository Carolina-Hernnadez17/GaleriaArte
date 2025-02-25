using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using static Mysqlx.Crud.Order.Types;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace GaleriaArte.Controllers
{
    public class ObrasArteController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        private ConexionGallery conexion = new ConexionGallery();
        public IActionResult ObrasUSer()
        {
            List<obra> lista = new List<obra>();

            try
            {


                using (var conn = conexion.AbrirConexion())
                {
                    string query = "SELECT * FROM obra";
                    var command = new MySqlCommand(query, conn);
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {


                        obra vista_obra = new obra
                        {
                            id_obra = reader.GetInt32("id_obra"),
                            id_cliente = reader.GetInt32("id_cliente"),
                            nombre_artista = reader.GetString("nombre_artista"),
                            titulo = reader.GetString("titulo"),
                            estilo_arte = reader.GetString("estilo_arte"),
                            precio = reader.GetDecimal("precio"),
                            num_registro = reader.GetString("num_registro"),
                            descripcion = reader.GetString("descripcion"),
                            imagen_url = reader["imagen_url"] as string ?? "",
                            estado = reader.GetInt32("estado")
                        };
                        lista.Add(vista_obra);

                    }
                    return View(lista);
                }

            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener obras: " + ex.Message;
                return View();
            }


        }

        //public IActionResult ObrasUSer()
        //{
        //    List<obra> lista = new List<obra>();

        //    try
        //    {


        //        using (var conn = conexion.AbrirConexion())
        //        {
        //            string query = "SELECT * FROM obra";
        //            var command = new MySqlCommand(query, conn);
        //            var reader = command.ExecuteReader();

        //            while (reader.Read())
        //            {


        //                obra vista_obra = new obra
        //                {
        //                    id_obra = reader.GetInt32("id_obra"),
        //                    id_cliente = reader.GetInt32("id_cliente"),
        //                    nombre_artista = reader.GetString("nombre_artista"),
        //                    titulo = reader.GetString("titulo"),
        //                    estilo_arte = reader.GetString("estilo_arte"),
        //                    precio = reader.GetDecimal("precio"),
        //                    num_registro = reader.GetString("num_registro"),
        //                    descripcion = reader.GetString("descripcion"),
        //                    imagen_url = reader["imagen_url"] as string ?? "",
        //                    estado = reader.GetInt32("estado")
        //                };
        //                lista.Add(vista_obra);

        //            }
        //           return View(lista);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Error al obtener obras: " + ex.Message;
        //        return View();
        //    }


        //}


        [HttpGet]
        public ActionResult Agregar_Obra()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Agregar_Obra(obra obra, IFormFile file)
        {
            try
            {
                string rutaImagen = null;
                if (file != null && file.Length > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                   
                    string fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(folderPath, fileName);

                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    rutaImagen = $"/images/{fileName}";
                }
                obra.imagen_url = rutaImagen;
                obra.id_cliente = 2;
                using (var conn = conexion.AbrirConexion())
                {
                    string query = "INSERT INTO obra (id_cliente, nombre_artista, titulo, estilo_arte, precio,num_registro, descripcion, imagen_url,estado)" +
                                    " VALUES (@id_cliente, @nombre_artista, @titulo, @estilo_arte, @precio,@num_registro, @descripcion, @imagen_url,@estado)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_cliente", obra.id_cliente);
                    cmd.Parameters.AddWithValue("@nombre_artista", obra.nombre_artista);
                    cmd.Parameters.AddWithValue("@titulo", obra.titulo);
                    cmd.Parameters.AddWithValue("@estilo_arte", obra.estilo_arte);
                    cmd.Parameters.AddWithValue("@precio", obra.precio);
                    cmd.Parameters.AddWithValue("@num_registro", obra.num_registro);
                    cmd.Parameters.AddWithValue("@descripcion", obra.descripcion);
                    cmd.Parameters.AddWithValue("@imagen_url", obra.imagen_url);
                    cmd.Parameters.AddWithValue("@estado", obra.estado);
                    

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Obra agregada exitosamente";
                        return RedirectToAction("Agregar_Obra");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "No se pudo agregar ";
                    }

                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al agregar Obra: " + ex.Message;
                return View();
            }

            return View(obra);
        }



    }
}
