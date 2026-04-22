namespace Apptech.Models
{
    public class Producto
    {
        public int Id { get; set; } // Mayúscula para coincidir con el backend
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string? ImagenUrl { get; set; }
        public string? Grupo { get; set; }
        public int UsuarioId { get; set; } // EXACTAMENTE ASÍ
    }
}