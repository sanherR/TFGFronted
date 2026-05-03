namespace Apptech.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Precio { get; set; } 
        public string ImagenUrl { get; set; }
        public int CategoriaId { get; set; }
        public string? Grupo { get; set; } 

        // --- AÑADE ESTAS DOS LÍNEAS PARA QUE DEJE DE DAR ERROR ---
        public string Caracteristicas { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
