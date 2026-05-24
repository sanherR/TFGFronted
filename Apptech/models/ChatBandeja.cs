using System.Text.Json.Serialization;

namespace Apptech.Models
{
    public class ChatBandeja
    {
        public int Id { get; set; }
        public string TituloChat { get; set; } 
        public string UltimoMensaje { get; set; } 
        public string ImagenProductoUrl { get; set; } 

        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        // NUEVOS CAMPOS
        public string NombreProducto { get; set; }
        public string NombreUsuario { get; set; }
    }
}