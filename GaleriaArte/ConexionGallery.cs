using MySql.Data.MySqlClient;

namespace GaleriaArte
{
    public class ConexionGallery
    {
        private string cadenaConexion = "server=localhost; database=galeria_arte; user=galeria; password=12345;";
        private MySqlConnection conexion;

        public ConexionGallery()
        {
            conexion = new MySqlConnection(cadenaConexion);
        }

        public MySqlConnection AbrirConexion()
        {
            try
            {
                if (conexion.State == System.Data.ConnectionState.Closed)
                {
                    conexion.Open();
                    Console.WriteLine("Conexión abierta.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al abrir la conexión: " + ex.Message);
            }
            return conexion;
        }

        public void CerrarConexion()
        {
            try
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    conexion.Close();
                    Console.WriteLine("Conexión cerrada.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cerrar la conexión: " + ex.Message);
            }
        }

        public string ProbarConexion()
        {
            try
            {
                AbrirConexion();
                return "Conexión exitosa con la base de datos.";
            }
            catch (Exception ex)
            {
                return "Error al conectar con la base de datos: " + ex.Message;
            }
            finally
            {
                CerrarConexion();
            }
        }

    }
}
