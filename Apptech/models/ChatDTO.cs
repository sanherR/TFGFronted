namespace Apptech.Models
{
    public class ChatDTO
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public int CompradorId { get; set; }
        public int ProductoId { get; set; }
        public string? TituloChat { get; set; } // Nombre del vendedor o del producto
        public string? UltimoMensaje { get; set; }
        public string? ImagenProductoUrl { get; set; }
        public ItemPop? Producto { get; set; } // El objeto completo para poder abrir el chat
    }
}