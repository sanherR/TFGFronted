using System.Text.Json.Serialization; // Necesitas este using arriba

namespace Apptech.Models
{
    public class ChatBandeja
    {
        public int Id { get; set; }
        
        public string TituloChat { get; set; } 
        
        public string UltimoMensaje { get; set; } 
        
        public string ImagenProductoUrl { get; set; } 

        // Con esto le decimos: "Busca en el JSON algo que se llame productoId o id_producto"
        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }
    }
}