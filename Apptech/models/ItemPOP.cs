using System.Text.Json.Serialization;

namespace Apptech.Models
{
    public class ItemPop
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonPropertyName("caracteristicas")]
        public string Caracteristicas { get; set; } = string.Empty;

        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

        [JsonPropertyName("categoriaId")] 
        public int CategoriaId { get; set; }

        [JsonPropertyName("estado_producto")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("vendido")]
        public int Vendido { get; set; }

        // Propiedades calculadas para el XAML
        public bool EsVendido => Vendido == 1;
        public bool PuedeComprar => Vendido == 0;

        [JsonPropertyName("imagenUrl")]
        public string ImagenUrl { get; set; } = string.Empty;
    }
}