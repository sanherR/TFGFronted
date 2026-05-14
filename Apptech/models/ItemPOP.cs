using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Apptech.Models
{
    public class ItemPop : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _esFavorito;
        private int _vendido; // Ahora lo manejamos con el setter para notificar cambios

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

        // Sincronizado con la columna 'vendido' del Backend
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
                    // Notificamos a las propiedades calculadas para que la UI se actualice
                    OnPropertyChanged(nameof(EsReservado));
                    OnPropertyChanged(nameof(PuedeComprar));
                }
            }
        }

        // --- LÓGICA PARA LA INTERFAZ ---
        
        // Si Vendido es 1, mostramos el cartelito de RESERVADO
        public bool EsReservado => Vendido == 1;

        // Solo se puede comprar si Vendido es 0
        public bool PuedeComprar => Vendido == 0;

        // -------------------------------

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