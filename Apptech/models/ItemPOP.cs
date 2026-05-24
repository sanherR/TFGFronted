using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Apptech.Models
{
    public class ItemPop : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _esFavorito;
        private int _vendido;
        private string _imagenUrlBase; 

        [JsonPropertyName("id_producto")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonPropertyName("caracteristicas")]
        public string Caracteristicas { get; set; } = string.Empty;

        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

        public int categoria_id { get; set; }

        [JsonPropertyName("usuario_id")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("estado_producto")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("vendido")]
        public int Vendido 
        { 
            get => _vendido;
            set
            {
                if (_vendido != value)
                {
                    _vendido = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EsReservado));
                    OnPropertyChanged(nameof(PuedeComprar));
                }
            }
        }

        
        
        [JsonPropertyName("imagen_url")] 
        public string ImagenUrlBase 
        { 
            get => _imagenUrlBase;
            set { _imagenUrlBase = value; OnPropertyChanged(nameof(ImagenUrl)); }
        }

        [JsonIgnore]
        public string ImagenUrl 
        { 
            get 
            {
                if (string.IsNullOrEmpty(ImagenUrlBase)) return "placeholder.png";
                if (ImagenUrlBase.StartsWith("http")) return ImagenUrlBase;
                return $"https://tfgbacken-production.up.railway.app{ImagenUrlBase}";
            }
        }
        

        public bool EsReservado => Vendido == 1;
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

        public string IconoFavorito => EsFavorito ? "heart_filled.png" : "heart_empty.png";

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}