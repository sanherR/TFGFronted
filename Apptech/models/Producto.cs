using System.Text.Json.Serialization;

namespace Apptech.Models
{
    public class Producto
    {
        [JsonPropertyName("id_producto")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonPropertyName("vendido")]
        public int Vendido { get; set; } // 0=Libre, 1=Reservado, 2=Vendido

        [JsonPropertyName("precio")]
        public int Precio { get; set; } 

        [JsonPropertyName("imagen_url")]
        public string ImagenUrlBase { get; set; } = string.Empty;

        
        [JsonIgnore] 
public string ImagenUrl 
{
    get
    {
        // 1. Si no hay nada, devolvemos el placeholder
        if (string.IsNullOrEmpty(ImagenUrlBase))
            return "placeholder.png"; 

        // 2. Si ya es una URL completa (http...), la devolvemos tal cual
        if (ImagenUrlBase.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return ImagenUrlBase;

        // 3. Aseguramos que la base termine en / y la ruta relativa NO empiece por /
        // para evitar que queden dos barras // o ninguna.
        string baseUrl = "https://tfgbacken-production.up.railway.app/";
        string cleanPath = ImagenUrlBase.TrimStart('/'); 
        
        return $"{baseUrl}{cleanPath}";
    }
}
    
        [JsonPropertyName("categoria_id")]
        public int CategoriaId { get; set; }

        [JsonPropertyName("usuario_id")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("estado_producto")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("caracteristicas")]
        public string Caracteristicas { get; set; } = string.Empty;

        public string Disponibilidad { get; set; } = "disponible";
    }
}