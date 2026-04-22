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

    }
}

