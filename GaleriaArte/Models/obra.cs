namespace GaleriaArte.Models
{
    public class obra
    {
        public int id_obra {  get; set; }
        public int id_cliente { get; set; }
        public string nombre_artista { get; set; }
        public string titulo {  get; set; }
        public string estilo_arte { get; set; }
        public int ano_creacio { get; set; }
        public decimal precio { get; set; }
        public string num_registro {  get; set; }
        
        public string descripcion { get; set; }
        public string imagen_url { get; set; }
        public int estado {  get; set; }
        
    }
}
