namespace GaleriaArte.Models
{
    public class AgregarObraViewModel
    {
        public int ExposicionId { get; set; }
        public string ExposicionTitulo { get; set; }
        public List<obra> ObrasDisponibles { get; set; } = new List<obra>();
        public string ErrorMessage { get; set; }
    }
}
