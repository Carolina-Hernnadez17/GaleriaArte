using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GaleriaArte.Controllers
{
    public class LoginController : Controller
    {

        private readonly ConexionGallery _conexion;

        public LoginController()
        {
            _conexion = new ConexionGallery();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(usuario usuario)
        {
            try
            {
                string query = "SELECT * FROM usuario WHERE correo = @correo AND estado = 1"; // Solo buscamos usuarios activos
                using (var conn = _conexion.AbrirConexion())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@correo", usuario.Correo);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Si el correo no existe
                    if (!reader.HasRows)
                    {
                        TempData["Mensaje"] = "Cuenta inexistente.";
                        return RedirectToAction("Login");
                    }

                    // Si el correo existe, validamos la contraseña
                    reader.Read(); // Leer el primer resultado del correo
                    string contrasena = reader["contrasena"].ToString(); // Obtener la contraseña almacenada directamente
                    bool tipoUsuario = Convert.ToBoolean(reader["tipo_usuario"]);

                    // Verificar si la contraseña coincide directamente (sin hash)
                    if (usuario.Contrasena != contrasena)
                    {
                        TempData["Mensaje"] = "Correo o contraseña incorrectos.";
                        return RedirectToAction("Login");
                    }

                    // Si la contraseña es correcta y el tipo de usuario es 'false', permitimos el acceso
                    if (tipoUsuario == false)
                    {
                        return RedirectToAction("Index_usuario", "Home");
                    }
                    else
                    {
                        //TempData["Mensaje"] = "Acceso denegado.";
                        return RedirectToAction("Index", "Home");
                    }
                }

            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error al realizar el login: " + ex.Message;
                return View();
            }
        }






        // GET: Registro
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(usuario usuario)
        {
            usuario.Tipo_Usuario = true;
            usuario.Estado = true;

            string emailPattern = @"^[a-zA-Z0-9._%+-]+@(gmail\.com|outlook\.com|yahoo\.com|icloud\.com|protonmail\.com|zoho\.com|gmx\.com)$";
            if (!Regex.IsMatch(usuario.Correo, emailPattern))
            {
                ModelState.AddModelError("Correo", "El correo debe pertenecer a un dominio válido como Gmail, Outlook, Yahoo, iCloud, ProtonMail, Zoho o GMX.");
            }
            string telPattern = @"^\+(502|503|504|505|506|507)\s?[0-9]{4}\s?[0-9]{4}$";
            if (!Regex.IsMatch(usuario.Telefono, telPattern))
            {
                ViewBag.Error = "Formato invalido, ejemplo: +503 78787878.";
                ModelState.AddModelError("Telefono", "Formato invalido, ejemplo: +503 78787878");
                return View(usuario);


            }
            if (usuario.Contrasena.Length < 8)
            {
                ViewBag.Error = "La contraseña debe tener minimo 8 caracteres";
                return View(usuario);
            }


            if (ModelState.IsValid)
            {
                try
                {
                    string checkDocumentoQuery = "SELECT COUNT(*) FROM usuario WHERE documento_identidad = @DocumentoIdentidad";
                    // Comprobar si ya existe el correo
                    string checkQuery = "SELECT COUNT(*) FROM usuario WHERE correo = @Correo";

                    using (var conn = _conexion.AbrirConexion())
                    {
                        MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                        checkCmd.Parameters.AddWithValue("@Correo", usuario.Correo);

                        int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        MySqlCommand checkdoc = new MySqlCommand(checkDocumentoQuery, conn);
                        checkdoc.Parameters.AddWithValue("@DocumentoIdentidad", usuario.Documento_Identidad);

                        int existingdoc = Convert.ToInt32(checkdoc.ExecuteScalar());

                        if (existingCount > 0)
                        {
                            ViewBag.Error = "Ya existe un usuario con este correo electrónico.";
                            return View(usuario);
                        }
                        else if (existingdoc > 0) 
                        {
                            ViewBag.Error = "Ya existe un usuario con este documento de identidad";
                            return View(usuario);
                        }
                        string query = "INSERT INTO usuario (nombre_cliente, documento_identidad, direccion_cliente, correo, telefono, contrasena, tipo_usuario, estado) " +
                                   "VALUES (@NombreCliente, @DocumentoIdentidad, @DireccionCliente, @Correo, @Telefono, @Contrasena, @TipoUsuario, @Estado)";

                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@NombreCliente", usuario.Nombre_Cliente);
                        cmd.Parameters.AddWithValue("@DocumentoIdentidad", usuario.Documento_Identidad);
                        cmd.Parameters.AddWithValue("@DireccionCliente", usuario.Direccion_Cliente ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                        cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);
                        cmd.Parameters.AddWithValue("@TipoUsuario", usuario.Tipo_Usuario);
                        cmd.Parameters.AddWithValue("@Estado", usuario.Estado);

                        cmd.ExecuteNonQuery();


                        return RedirectToAction("Login", "Login");
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al registrar el usuario: " + ex.Message;
                }
            }

            return View(usuario);
        }

        

        public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarContrasena(usuario usuario)
        {
            try
            {
                string query = "SELECT * FROM usuario WHERE correo = @correo AND estado = 1"; // Solo buscamos usuarios activos
                using (var conn = _conexion.AbrirConexion())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@correo", usuario.Correo);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Si el correo no existe
                    if (!reader.HasRows)
                    {
                        TempData["Mensaje"] = "Cuenta inexistente.";
                        return RedirectToAction("RecuperarContrasena");
                    }

                    // Guardamos el tipo de usuario antes de cerrar el reader
                    reader.Read();
                    bool tipoUsuario = Convert.ToBoolean(reader["tipo_usuario"]);
                    int userId = Convert.ToInt32(reader["id_cusuario"]);
                    reader.Close();

                    //// Actualizamos la contraseña
                    //string queryEdit = "UPDATE usuario SET contrasena = @Contrasena WHERE correo = @Correo";
                    //MySqlCommand cm = new MySqlCommand(queryEdit, conn);
                    //cm.Parameters.AddWithValue("@Correo", usuario.Correo);
                    //cm.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);

                    //cm.ExecuteNonQuery();

                    // Si la actualización fue exitosa y el tipo de usuario es 'false', permitimos el acceso
                    if (tipoUsuario == false)
                    {
                        return RedirectToAction("BuscarPreguntas", new { userId = userId });
                    }
                    else
                    {
                        TempData["Mensaje"] = "Acceso denegado.";
                        return RedirectToAction("RecuperarContrasena");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error al realizar la recuperación: " + ex.Message;
                return View();
            }
        }

        public IActionResult NuevaContrasena(int userId)
        {
            if (userId <= 0)
            {
                TempData["Mensaje"] = "ID inválido.";
                return RedirectToAction("Index", "Home");
            }

            TempData["UserId"] = userId;  // Guardamos temporalmente el ID para la siguiente request
            return View();
        }


        [HttpPost]
        public IActionResult MostrarNuevaContrasena(int userId, string nuevaContra)
        {
            if (userId <= 0 || string.IsNullOrEmpty(nuevaContra) || nuevaContra.Length < 8)
            {
                TempData["Mensaje"] = "Datos inválidos para cambiar la contraseña. La contraseña debe tener minimo 8 caracteres";
                return RedirectToAction("NuevaContrasena", new { userId });
            }

            try
            {
                using (var conn = _conexion.AbrirConexion())
                {
                    string queryEdit = "UPDATE usuario SET contrasena = @Contrasena WHERE id_cusuario = @idUser";

                    using (var cm = new MySqlCommand(queryEdit, conn))
                    {
                        cm.Parameters.AddWithValue("@idUser", userId);
                        cm.Parameters.AddWithValue("@Contrasena", nuevaContra);

                        cm.ExecuteNonQuery();
                    }
                }

                TempData["MensajeExito"] = "Contraseña actualizada con éxito. Ahora puedes iniciar sesión.";
                return RedirectToAction("Index_usuario", "Home");
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al cambiar la contraseña: " + ex.Message;
                return RedirectToAction("NuevaContrasena", new { userId });
            }
        }
        public IActionResult BuscarPreguntas(int userId)
        {
            try
            {
                string query = "SELECT question, answer FROM PreguntasSeguridad WHERE user_id = @userId LIMIT 2"; // Solo tomamos 2 preguntas

                preguntasModel preguntas = new preguntasModel
                {
                    UserId = userId
                };

                using (var conn = _conexion.AbrirConexion())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    int index = 0;
                    while (reader.Read())
                    {
                        if (index == 0)
                        {
                            preguntas.Question1 = reader["question"].ToString();
                            preguntas.Answer1 = reader["answer"].ToString();
                        }
                        else if (index == 1)
                        {
                            preguntas.Question2 = reader["question"].ToString();
                            preguntas.Answer2 = reader["answer"].ToString();
                        }
                        index++;
                    }
                }

                if (string.IsNullOrEmpty(preguntas.Question1))
                {
                    TempData["Mensaje"] = "No se han establecido preguntas de recuperación.";
                    return RedirectToAction("RecuperarContrasena");
                }

                return View(preguntas);
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error al recuperar preguntas: " + ex.Message;
                return View();
            }
        }
        [HttpPost]
        public IActionResult ValidarRespuestas(preguntasModel model)
        {
            try
            {
                string query = "SELECT question, answer FROM PreguntasSeguridad WHERE user_id = @UserId LIMIT 2";

                Dictionary<string, string> respuestasBD = new Dictionary<string, string>();

                using (var conn = _conexion.AbrirConexion())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    int index = 0;
                    while (reader.Read())
                    {
                        if (index == 0)
                        {
                            respuestasBD[reader["question"].ToString()] = reader["answer"].ToString().Trim().ToLower();
                        }
                        else if (index == 1)
                        {
                            respuestasBD[reader["question"].ToString()] = reader["answer"].ToString().Trim().ToLower();
                        }
                        index++;
                    }
                }

                bool respuesta1Correcta = respuestasBD.ContainsKey(model.Question1) &&
                                          respuestasBD[model.Question1] == model.Answer1.Trim().ToLower();

                bool respuesta2Correcta = respuestasBD.ContainsKey(model.Question2) &&
                                          respuestasBD[model.Question2] == model.Answer2.Trim().ToLower();

                if (!respuesta1Correcta || !respuesta2Correcta)
                {
                    ViewBag.Msm = "Las respuestas no coinciden. Inténtalo de nuevo.";
                    return RedirectToAction("BuscarPreguntas", new { userId = model.UserId });

                }

                return RedirectToAction("NuevaContrasena", new { userId = model.UserId });
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error al validar respuestas: " + ex.Message;
                return RedirectToAction("BuscarPreguntas", new { userId = model.UserId });
            }
        }






        public IActionResult registro_user()
        {
            return View();
        }
        [HttpPost]
        public IActionResult registro_user(usuario usuario)
        {
            usuario.Tipo_Usuario = false;
            usuario.Estado = true;

            string emailPattern = @"^[a-zA-Z0-9._%+-]+@(gmail\.com|outlook\.com|yahoo\.com|icloud\.com|protonmail\.com|zoho\.com|gmx\.com)$";
            if (!Regex.IsMatch(usuario.Correo, emailPattern))
            {
                ModelState.AddModelError("Correo", "El correo debe pertenecer a un dominio válido como Gmail, Outlook, Yahoo, iCloud, ProtonMail, Zoho o GMX.");
                ViewBag.Error = "El correo debe pertenecer a un dominio válido como Gmail, Outlook, Yahoo, iCloud, ProtonMail, Zoho o GMX y debe tener una extension valida como .com. Ejemplo : galeria@gmail.com";
                return View(usuario);
            }
            string telPattern = @"^\+(502|503|504|505|506|507)\s?[0-9]{4}\s?[0-9]{4}$";
            if (!Regex.IsMatch(usuario.Telefono, telPattern))
            {
                ViewBag.Error = "Formato invalido de telefono, ejemplo: +503 78787878.";
                ModelState.AddModelError("Telefono", "Formato invalido, ejemplo: +503 78787878");
                return View(usuario);


            }
            if (usuario.Contrasena.Length < 8)
            {
                ViewBag.Error = "La contraseña debe tener minimo 8 caracteres";
                return View(usuario);
            }


            if (ModelState.IsValid)
            {
                try
                {
                    string checkDocumentoQuery = "SELECT COUNT(*) FROM usuario WHERE documento_identidad = @DocumentoIdentidad";
                    // Comprobar si ya existe el correo
                    string checkQuery = "SELECT COUNT(*) FROM usuario WHERE correo = @Correo";

                    using (var conn = _conexion.AbrirConexion())
                    {
                        MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                        checkCmd.Parameters.AddWithValue("@Correo", usuario.Correo);

                        int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        MySqlCommand checkdoc = new MySqlCommand(checkDocumentoQuery, conn);
                        checkdoc.Parameters.AddWithValue("@DocumentoIdentidad", usuario.Documento_Identidad);

                        int existingdoc = Convert.ToInt32(checkdoc.ExecuteScalar());

                        if (existingCount > 0)
                        {
                            ViewBag.Error = "Ya existe un usuario con este correo electrónico.";
                            return View(usuario);
                        }
                        else if (existingdoc > 0)
                        {
                            ViewBag.Error = "Ya existe un usuario con este documento de identidad";
                            return View(usuario);
                        }
                        string query = "INSERT INTO usuario (nombre_cliente, documento_identidad, direccion_cliente, correo, telefono, contrasena, tipo_usuario, estado) " +
                                   "VALUES (@NombreCliente, @DocumentoIdentidad, @DireccionCliente, @Correo, @Telefono, @Contrasena, @TipoUsuario, @Estado)";

                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@NombreCliente", usuario.Nombre_Cliente);
                        cmd.Parameters.AddWithValue("@DocumentoIdentidad", usuario.Documento_Identidad);
                        cmd.Parameters.AddWithValue("@DireccionCliente", usuario.Direccion_Cliente ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                        cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);
                        cmd.Parameters.AddWithValue("@TipoUsuario", usuario.Tipo_Usuario);
                        cmd.Parameters.AddWithValue("@Estado", usuario.Estado);

                        cmd.ExecuteNonQuery();


                        //return RedirectToAction("Login", "Login");
                        // Obtener el ID del usuario recién insertado
                        string getUserIdQuery = "SELECT LAST_INSERT_ID()";
                        MySqlCommand getUserIdCmd = new MySqlCommand(getUserIdQuery, conn);
                        int userId = Convert.ToInt32(getUserIdCmd.ExecuteScalar());

                        // Redirigir al formulario de seguridad pasando el userId como parámetro
                        // Devolver el ID de usuario al cliente como parte de la respuesta
                        return Json(new { success = true, userId = userId });
                        //return RedirectToAction("PreguntasSeguridad", new { userId = userId });
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al registrar el usuario: " + ex.Message;
                }
            }

            return View(usuario);
        }

        // Acción para mostrar el formulario de pregunta de seguridad
        public IActionResult PreguntasSeguridad(int userId)
        {
            var preguntas = new List<string>
            {
                "¿Cuál es tu color favorito?",
                "¿En qué ciudad naciste?",
                "¿Cuál es el nombre de tu primera mascota?",
                "¿Cuál es tu comida favorita?"
            };
            var preguntas2 = new List<string>
            {
                "¿Cuál es el nombre de tu abuela materna?",
                "¿Cuál fue tu primer trabajo?",
                "¿Cuál es tu película favorita?",
                "¿Cuál era el nombre de tu peluche favorito?"
            };

            var viewModel = new preguntasModel
            {
                UserId = userId
            };

            ViewBag.Preguntas = preguntas;
            ViewBag.Preguntas2 = preguntas2;


            return View(viewModel);
        }


        // Acción para guardar la respuesta de seguridad
        [HttpPost]
        public IActionResult GuardarPreguntasSeguridad(preguntasModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var conn = _conexion.AbrirConexion())
                    {
                        string insertQuery = "INSERT INTO PreguntasSeguridad (user_id, question, answer) VALUES (@UserId, @Question1, @Answer1), (@UserId, @Question2, @Answer2)";
                        MySqlCommand cmd = new MySqlCommand(insertQuery, conn);
                        cmd.Parameters.AddWithValue("@UserId", model.UserId);
                        cmd.Parameters.AddWithValue("@Question1", model.Question1);
                        cmd.Parameters.AddWithValue("@Answer1", model.Answer1);
                        cmd.Parameters.AddWithValue("@Question2", model.Question2);
                        cmd.Parameters.AddWithValue("@Answer2", model.Answer2);

                        cmd.ExecuteNonQuery();

                        return RedirectToAction("Login", "Login");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al guardar las respuestas: " + ex.Message;
                    return View("PreguntasSeguridad", model);
                }
            }

            ViewBag.Preguntas = new List<string>
            {
                "¿Cuál es tu color favorito?",
                "¿En qué ciudad naciste?",
                "¿Cuál es el nombre de tu primera mascota?",
                "¿Cuál es tu comida favorita?"
            };
            ViewBag.Preguntas2 = new List<string>
            {
                "¿Cuál es el nombre de tu abuela materna?",
                "¿Cuál fue tu primer trabajo?",
                "¿Cuál es tu película favorita?",
                "¿Cuál era el nombre de tu peluche favorito?"
            };

            return View("PreguntasSeguridad", model);
        }


    }
}
