using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Apptech.Models
{
    public class ItemPop : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _esFavorito;

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

        public bool EsVendido => Vendido == 1;
        public bool PuedeComprar => Vendido == 0;

        public bool EsFavorito
        {
            get => _esFavorito;
            set
            {
                if (_esFavorito != value)
                {
                    _esFavorito = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IconoFavorito));
                }
            }
        }

        public string IconoFavorito =>
            EsFavorito ? "heart_filled.png" : "heart_empty.png";

        [JsonPropertyName("imagenUrl")]
        public string ImagenUrl { get; set; } = string.Empty;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}