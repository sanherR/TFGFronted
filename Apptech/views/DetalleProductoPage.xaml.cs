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
        private async void OnComprarClicked(object sender, EventArgs e)
        {
            // Verificamos que el BindingContext sea el modelo correcto
            if (BindingContext is ItemPop producto)
            {
                bool confirmar = await DisplayAlert("Confirmar Compra", "¿Deseas reservar este artículo?", "Sí", "No");

                if (confirmar)
                {
                    // Simulamos que el producto se ha vendido
                    producto.Vendido = 1;

                    // Notificamos al XAML para que refresque la visibilidad
                    OnPropertyChanged(nameof(producto.EsVendido)); 
                    OnPropertyChanged(nameof(producto.PuedeComprar));

                    await DisplayAlert("¡Logrado!", "Has reservado el producto con éxito.", "OK");

                    // Regresamos a la lista principal
                    await Navigation.PopAsync();
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