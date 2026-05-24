namespace Apptech.Models
{
    public class ChatDTO
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public int CompradorId { get; set; }
        public int ProductoId { get; set; }
        public string? TituloChat { get; set; }
        public string? UltimoMensaje { get; set; }

        // Esta propiedad es la que viene de la API
        public string? ImagenProductoUrl { get; set; }

        // --- LÓGICA DE IMAGEN CENTRALIZADA ---
        [System.Text.Json.Serialization.JsonIgnore]
        public string ImagenUrl 
        { 
            get 
            {
                if (string.IsNullOrEmpty(ImagenProductoUrl)) return "placeholder.png";
                if (ImagenProductoUrl.StartsWith("http")) return ImagenProductoUrl;
                return $"https://tfgbacken-production.up.railway.app{ImagenProductoUrl}";
            }
        }
        // ------------------------------------

        public ItemPop? Producto { get; set; } 
    }
}