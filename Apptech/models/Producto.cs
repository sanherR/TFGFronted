using System.Text.Json.Serialization;

namespace Apptech.Models
{
    public class Producto
    {
        [JsonPropertyName("id_producto")] // Coincide con tu imagen de la BD
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonPropertyName("precio")]
        public int Precio { get; set; } 

        [JsonPropertyName("imagen_url")]
        public string ImagenUrl { get; set; } = string.Empty;

    
        public int categoria_id { get; set; }

        [JsonPropertyName("usuario_id")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("estado_producto")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("caracteristicas")]
        public string Caracteristicas { get; set; } = string.Empty;

        public string Disponibilidad { get; set; } = "disponible";
    }
}