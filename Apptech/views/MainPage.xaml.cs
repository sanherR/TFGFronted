namespace Apptech.views;

using Apptech.Models;
using Apptech.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
{
    base.OnAppearing();
    // ¡Aquí estaba el error! Estaba comentado con //
    // Quitamos el Task.Run y llamamos directamente al método async
    await CargarDatosIniciales();
}

private async Task CargarDatosIniciales()
{
    try 
    {
        Debug.WriteLine("Pidiendo productos para la MainPage...");
        var productos = await _apiService.ObtenerProductos();

        if (productos != null && productos.Count > 0)
        {
            // Importante: No necesitas Task.Run si usas await arriba.
            // Solo asegúrate de tocar la UI en el hilo principal
            MainThread.BeginInvokeOnMainThread(() => {
                miCarrusel.ItemsSource = null; 
                miCarrusel.ItemsSource = productos; 
                Debug.WriteLine($"✅ Main: Datos refrescados. Total: {productos.Count}");
            });
        }
        else
        {
            Debug.WriteLine("⚠️ Main: La API no ha devuelto productos o la lista está vacía.");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error en CargarDatosIniciales: {ex.Message}");
    }
}
    // --- MÉTODOS DE NAVEGACIÓN ---

    private void IrAProductos(object sender, EventArgs e)
    {
        // Posición 0 del carrusel (Inicio)
        miCarrusel.ScrollTo(0, animate: true);
    }

    private async void IrABusqueda(object sender, EventArgs e)
{
    try 
    {
        // Esto abre la página de búsqueda como una pantalla completa
        await Navigation.PushAsync(new ProductoCategoriaPage()); 
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error al abrir Búsqueda: {ex.Message}");
    }
}

    private async void IrAVender(object sender, EventArgs e)
    {
        // Navegación real a página independiente
        await Navigation.PushAsync(new VenderPage());
    }

    private async void IrAChat(object sender, EventArgs e)
{
    try 
    {
        // IMPORTANTE: Ahora navegamos a ListaChatsPage, no a ChatPage directo
        await Navigation.PushAsync(new ListaChatsPage());
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error al abrir Lista de Chats: {ex.Message}");
    }
}

    private async void IrAPerfil(object sender, EventArgs e)
    {
        try 
        {
            // Navegación real (Ya comprobado que funciona)
            await Navigation.PushAsync(new PerfilPage());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error al navegar al Perfil: {ex.Message}");
        }
    }

    // --- EVENTOS DEL SISTEMA ---

    private void OnPositionChanged(object sender, PositionChangedEventArgs e)
    {
        // Actualizamos el título según la posición del carrusel principal
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