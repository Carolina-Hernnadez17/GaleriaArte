using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Models
{
    public class usuario
    {
        [Key]
        public int Id_CUsuario { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre_Cliente { get; set; }

        [Required]
        [MaxLength(50)]
        public string Documento_Identidad { get; set; }

        [MaxLength(50)]
        public string Direccion_Cliente { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public string Correo { get; set; }

        [MaxLength(50)]
        public string Telefono { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }

        public bool Tipo_Usuario { get; set; } = false;

        public bool Estado { get; set; } = true;
        //jiji
    }
}
