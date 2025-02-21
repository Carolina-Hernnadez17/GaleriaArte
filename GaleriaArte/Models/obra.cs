namespace GaleriaArte.Models
{
    public class obra
    {
        public int id_obra {  get; set; }
        public int id_cliente { get; set; }
        public string nombre_artista { get; set; }
        public string num_registro {  get; set; }
        public string titulo {  get; set; }
        public DateTime año_creacion { get; set; }
        public decimal precio { get; set; }
        public string descripcion { get; set; }
        public bool estado {  get; set; }
    }
}
