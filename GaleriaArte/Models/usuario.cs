using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Models
{
    public class usuario
    {
        [Key]
        public int Id_CUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El nombre no debe superar los 50 caracteres.")]
        public string Nombre_Cliente { get; set; }

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [MaxLength(50)]
        public string Documento_Identidad { get; set; }

        [Required(ErrorMessage = "La direccion es obligatorio.")]
        [MaxLength(50)]
        public string Direccion_Cliente { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [MaxLength(50)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [RegularExpression(@"^\+(502|503|504|505|506|507)\s?[0-9]{4}\s?[0-9]{4}$", ErrorMessage = "Número de teléfono no válido.")]
        public string Telefono { get; set; }


        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Contrasena { get; set; }

        public bool Tipo_Usuario { get; set; } = false;
        public bool Estado { get; set; } = true;
    }
}
