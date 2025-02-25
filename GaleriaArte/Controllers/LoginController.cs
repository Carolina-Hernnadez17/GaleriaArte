using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GaleriaArte.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

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
            usuario.Tipo_Usuario = false;
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

        


        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
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
                    reader.Close();

                    // Actualizamos la contraseña
                    string queryEdit = "UPDATE usuario SET contrasena = @Contrasena WHERE correo = @Correo";
                    MySqlCommand cm = new MySqlCommand(queryEdit, conn);
                    cm.Parameters.AddWithValue("@Correo", usuario.Correo);
                    cm.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);

                    cm.ExecuteNonQuery();

                    // Si la actualización fue exitosa y el tipo de usuario es 'false', permitimos el acceso
                    if (tipoUsuario == false)
                    {
                        return RedirectToAction("Index", "Home");
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
        public IActionResult registro_user()
        {
            return View();
        }
        [HttpPost]
        public IActionResult registro_user(usuario usuario)
        {
            return View();
        }


    }
}
