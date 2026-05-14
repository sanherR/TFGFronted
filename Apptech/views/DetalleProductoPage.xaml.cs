using Apptech.Models;
using System;
using Apptech.Services;

namespace Apptech.views
{
    public partial class DetalleProductoPage : ContentPage
    {
        public DetalleProductoPage(ItemPop producto)
        {
            InitializeComponent();
            Console.WriteLine("🔥 DEBUG DETALLE - ID ENTRADA: " + producto?.Id);
            BindingContext = producto;
        }
        private readonly ApiService _apiService = new ApiService();
        private async void OnChatClicked(object sender, EventArgs e)
{
    if (BindingContext is ItemPop producto)
    {
        // 1. Obtener mi ID (quién está usando la app)
        int miId = Preferences.Get("userId", 0);
        
        // 2. Validación: No chatear conmigo mismo
        if (miId == producto.UsuarioId) 
        {
            await DisplayAlert("Aviso", "Este producto es tuyo.", "OK");
            return;
        }

        // 3. LA CLAVE: Llamamos a la API para crear el chat en la base de datos
        // Esto hará que en phpMyAdmin aparezca la fila en la tabla conversaciones
        int chatIdReal = await _apiService.ObtenerOCrearChat(producto.UsuarioId, producto.Id);

        if (chatIdReal > 0)
        {
            // 4. Si el servidor nos da un ID (ej: chat número 5), vamos a la pantalla de burbujas
            // Usamos el constructor que recibe el ID y el nombre del producto
            await Navigation.PushAsync(new ChatPage(chatIdReal, producto.Nombre));
        }
        else 
        {
            // Si llega aquí, es que la API falló (revisa que el servidor esté encendido)
            await DisplayAlert("Error", "No se pudo conectar con el vendedor.", "OK");
        }
    }
}
       private async void OnFavoritoClicked(object sender, EventArgs e)
        {
            var image = (Image)sender;

            if (image.BindingContext is ItemPop producto)
            {
                Console.WriteLine("ID PRODUCTO: " + producto.Id);

                producto.EsFavorito = !producto.EsFavorito;

                var success = await _apiService.AñadirFavorito(producto.Id);

                if (!success)
                {
                    producto.EsFavorito = !producto.EsFavorito;
                    
                }
            }
        }
    }
}