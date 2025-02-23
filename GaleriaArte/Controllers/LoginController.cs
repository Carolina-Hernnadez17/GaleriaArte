using System.ComponentModel.DataAnnotations;
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
                    //holi
                    //holi

                    // Si el correo existe, validamos la contraseña
                    reader.Read(); // Leer el primer resultado del correo
                    string contrasenaHash = reader["contrasena"].ToString(); // Asumimos que las contraseñas están hashadas
                    bool tipoUsuario = Convert.ToBoolean(reader["tipo_usuario"]);

                    // Verificar si la contraseña coincide (aquí deberías usar un método de hash en vez de comparar directamente)
                    if (usuario.Contrasena != contrasenaHash)
                    {
                        TempData["Mensaje"] = "Correo o contraseña incorrectos.";
                        return RedirectToAction("Login");
                    }

                    // Si la contraseña es correcta y el tipo de usuario es 'false', permitimos el acceso
                    if (tipoUsuario == false)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["Mensaje"] = "Acceso denegado.";
                        return RedirectToAction("Login");
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

        // POST: Registro
        [HttpPost]
        public IActionResult Registro(usuario usuario)
        {
            if (string.IsNullOrEmpty(usuario.Nombre_Cliente) || string.IsNullOrEmpty(usuario.Documento_Identidad) ||
                string.IsNullOrEmpty(usuario.Correo) || string.IsNullOrEmpty(usuario.Contrasena))
            {
                ViewBag.Error = "Por favor, complete todos los campos requeridos.";
                return View(usuario);
            }

            try
            {
                // Consulta para insertar los datos en la base de datos
                string query = "INSERT INTO usuario (nombre_cliente, documento_identidad, direccion_cliente, correo, telefono, contrasena, tipo_usuario, estado) " +
                               "VALUES (@NombreCliente, @DocumentoIdentidad, @DireccionCliente, @Correo, @Telefono, @Contrasena, @TipoUsuario, @Estado)";

                using (var conn = _conexion.AbrirConexion())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@NombreCliente", usuario.Nombre_Cliente);
                    cmd.Parameters.AddWithValue("@DocumentoIdentidad", usuario.Documento_Identidad);
                    cmd.Parameters.AddWithValue("@DireccionCliente", usuario.Direccion_Cliente ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                    cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);
                    cmd.Parameters.AddWithValue("@TipoUsuario", usuario.Tipo_Usuario);
                    cmd.Parameters.AddWithValue("@Estado", usuario.Estado);

                    cmd.ExecuteNonQuery(); // Ejecuta la inserción de datos en la base de datos
                }

                return RedirectToAction("Login", "Login"); // Redirige al login después del registro
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al registrar el usuario: " + ex.Message;
                return View(usuario);
            }
        }
        //[HttpPost]
        //public IActionResult Registro(usuario usuario)
        //{
        //    // Validación de los campos obligatorios
        //    if (string.IsNullOrEmpty(usuario.Nombre_Cliente) || string.IsNullOrEmpty(usuario.Correo) ||
        //        string.IsNullOrEmpty(usuario.Contrasena) || string.IsNullOrEmpty(usuario.Telefono))
        //    {
        //        ViewBag.Error = "Por favor, complete todos los campos requeridos.";
        //        return View(usuario);
        //    }

        //    // Validación del formato de correo electrónico
        //    if (!new EmailAddressAttribute().IsValid(usuario.Correo))
        //    {
        //        ViewBag.Error = "Por favor, ingrese un correo electrónico válido.";
        //        return View(usuario);
        //    }

        //    // Validación del formato de teléfono para El Salvador
        //    if (!System.Text.RegularExpressions.Regex.IsMatch(usuario.Telefono, @"^\+503\d{8}$"))
        //    {
        //        ViewBag.Error = "El teléfono debe tener el formato +503XXXXXXXX.";
        //        return View(usuario);
        //    }

        //    // Validación de la contraseña (mínimo 8 caracteres)
        //    if (usuario.Contrasena.Length < 8)
        //    {
        //        ViewBag.Error = "La contraseña debe tener al menos 8 caracteres.";
        //        return View(usuario);
        //    }

        //    // Verificar si el correo ya está registrado
        //    string checkQuery = "SELECT COUNT(*) FROM usuario WHERE correo = @Correo";
        //    using (var conn = _conexion.AbrirConexion())
        //    {
        //        MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
        //        checkCmd.Parameters.AddWithValue("@Correo", usuario.Correo);
        //        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

        //        if (count > 0)
        //        {
        //            ViewBag.Error = "El correo electrónico ya está registrado.";
        //            return View(usuario);
        //        }
        //    }

        //    try
        //    {
        //        string query = "INSERT INTO usuario (nombre_cliente, documento_identidad, direccion_cliente, correo, telefono, contrasena, tipo_usuario, estado) " +
        //                       "VALUES (@NombreCliente, @DocumentoIdentidad, @DireccionCliente, @Correo, @Telefono, @Contrasena, @TipoUsuario, @Estado)";

        //        using (var conn = _conexion.AbrirConexion())
        //        {
        //            MySqlCommand cmd = new MySqlCommand(query, conn);
        //            cmd.Parameters.AddWithValue("@NombreCliente", usuario.Nombre_Cliente);
        //            cmd.Parameters.AddWithValue("@DocumentoIdentidad", usuario.Documento_Identidad);
        //            cmd.Parameters.AddWithValue("@DireccionCliente", usuario.Direccion_Cliente ?? (object)DBNull.Value);
        //            cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
        //            cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono ?? (object)DBNull.Value);
        //            cmd.Parameters.AddWithValue("@Contrasena", usuario.Contrasena);
        //            cmd.Parameters.AddWithValue("@TipoUsuario", usuario.Tipo_Usuario);
        //            cmd.Parameters.AddWithValue("@Estado", usuario.Estado);

        //            cmd.ExecuteNonQuery(); 
        //        }

        //        return RedirectToAction("Login", "Login"); // Redirige al login después del registro
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Error al registrar el usuario: " + ex.Message;
        //        return View(usuario);
        //    }
        //}

    }
}
