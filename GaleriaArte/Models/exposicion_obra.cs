using System.ComponentModel.DataAnnotations;

namespace GaleriaArte.Models
{
    public class exposicion_obra
    {
        [Key]

        public int id_exposicion_obra { get; set; }

        [Required]
        public int id_exposicion { get; set; }
        [Required]
        public int id_obra { get; set; }
    }
}
