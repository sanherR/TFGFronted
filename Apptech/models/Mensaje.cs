using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Apptech.Models
{
    public class Mensaje
    {
        public int Id { get; set; }
        public int Conversacion { get; set; } 
        public int EmisorId { get; set; }
        public string Contenido { get; set; } 
        public DateTime Fecha { get; set; }
        public bool Leido { get; set; } 

        // Comparamos el EmisorId con el ID del usuario logueado en Preferences
        public bool EsMio => EmisorId == Preferences.Get("userId", 0);

        // Estas propiedades las usaremos en el Binding del XAML
        public LayoutOptions Alineacion => EsMio ? LayoutOptions.End : LayoutOptions.Start;
        
        // Colores: Verde clarito para ti, gris claro para el otro
        public Color ColorBurbuja => EsMio ? Color.FromArgb("#DCF8C6") : Color.FromArgb("#E9E9EB");
        
        // Alineación del texto dentro de la burbuja
        public TextAlignment AlineacionTexto => EsMio ? TextAlignment.End : TextAlignment.Start;
    }
}