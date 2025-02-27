namespace GaleriaArte.Models
{
    public class historial_exposicion
    {
        public int id_historial { get; set; }
        public int id_exposicion { get; set; }
        public string usuario_modificacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public string detalles { get; set; }
    }
}
