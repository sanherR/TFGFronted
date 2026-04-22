namespace Apptech.views;
using Apptech.Models;
using Apptech.Services;
using System.Collections.ObjectModel;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }
   

    private void IrAProductos(object sender, EventArgs e)
    {
        miCarrusel.ScrollTo(0, animate: true);
    }

    private void IrACategorias(object sender, EventArgs e)
    {
        miCarrusel.ScrollTo(1, animate: true);
    }
    private async void IrAVender(object sender, EventArgs e)

    {
        await Navigation.PushAsync(new VenderPage());
    }

    private void IrAChat(object sender, EventArgs e)
    {
        miCarrusel.ScrollTo(2, animate: true);
    }

    private void IrAPerfil(object sender, EventArgs e)
    {

        miCarrusel.ScrollTo(3, animate: true);
    }

    private void OnPositionChanged(object sender, PositionChangedEventArgs e)
    {
        switch (e.CurrentPosition)
        {
            case 0:

                Title = "Productos";
                break;

            case 1:

                Title = "Categorías";
                break;

            case 2:

                Title = "Chat";
                break;

            case 3:

                Title = "Perfil";

                break;
        }
    }

    private void RecargarProductos()
    {
        var items = miCarrusel.ItemsSource;

        miCarrusel.ItemsSource = null;
        miCarrusel.ItemsSource = items;
    }

    

    


}