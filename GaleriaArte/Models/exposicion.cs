using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Models
{
    public class exposicion
    {
        [Key]
        public int id_exposicion { get; set; }

        [Required]
        public int id_locacion { get; set; }

        [Required]
        [MaxLength(50)]
        public string titulo_exposicion { get; set; }

        [Required]
        [MaxLength(50)]
        public string descripcion { get; set; }

        [Required]
        public DateTime fecha_inicio { get; set; }

        [Required]
        public DateTime fecha_cierre { get; set; }

        public Boolean estado {  get; set; }

    }
}
