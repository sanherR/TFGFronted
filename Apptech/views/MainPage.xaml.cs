namespace Apptech.views;

using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        
        
        MessagingCenter.Unsubscribe<object>(this, "ActualizarMainPage");
        MessagingCenter.Subscribe<object>(this, "ActualizarMainPage", (sender) =>
        {
            Debug.WriteLine("📢 [MainPage] Mensaje 'ActualizarMainPage' interceptado. Reenviando pulso a las subvistas...");
            MessagingCenter.Send<object>(this, "ForzarRefrescoProductos");
        });

        
        MessagingCenter.Unsubscribe<object>(this, "ActualizarPerfil");
        MessagingCenter.Subscribe<object>(this, "ActualizarPerfil", (sender) =>
        {
            Debug.WriteLine("📢 [MainPage] Mensaje 'ActualizarPerfil' interceptado. Reenviando pulso a las subvistas...");
            MessagingCenter.Send<object>(this, "ForzarRefrescoProductos");
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("🔄 [MainPage] Pasando por OnAppearing (Flecha de atrás pulsada). Forzando actualización interna.");
        MessagingCenter.Send<object>(this, "ForzarRefrescoProductos");
    }

    private void IrAProductos(object sender, EventArgs e)
    {
        miCarrusel.ScrollTo(0, animate: true);
    }

    private async void IrABusqueda(object sender, EventArgs e)
    {
        try { await Navigation.PushAsync(new ProductoCategoriaPage()); }
        catch (Exception ex) { Debug.WriteLine($"❌ Error al abrir Búsqueda: {ex.Message}"); }
    }

    private async void IrAVender(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new VenderPage());
    }

    private async void IrAChat(object sender, EventArgs e)
    {
        try { await Navigation.PushAsync(new ListaChatsPage()); }
        catch (Exception ex) { Debug.WriteLine($"❌ Error al abrir Lista de Chats: {ex.Message}"); }
    }

    private async void IrAPerfil(object sender, EventArgs e)
    {
        try { await Navigation.PushAsync(new PerfilPage()); }
        catch (Exception ex) { Debug.WriteLine($"❌ Error al navegar al Perfil: {ex.Message}"); }
    }

    private async void OnVolverAlLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnPositionChanged(object sender, PositionChangedEventArgs e)
    {
        switch (e.CurrentPosition)
        {
            case 0: Title = "Productos"; break;
            case 1: Title = "Categorías"; break;
            case 2: Title = "Chat"; break;
            case 3: Title = "Perfil"; break;
        }
    }
}