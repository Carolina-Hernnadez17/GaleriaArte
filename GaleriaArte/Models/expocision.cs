using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Models
{
    public class expocision
    {
        [Key]
        public int id_exposicion { get; set; }
        public int id_locacion { get; set; }
        public int id_obra {  get; set; }

        [Required]
        public string titulo_exposicion { get; set; }

        public string descripcion { get; set; }

        [Required]
        public DateTime fecha_inicio { get; set; }

        [Required]
        public DateTime fecha_cierre { get; set; }

    }
}
