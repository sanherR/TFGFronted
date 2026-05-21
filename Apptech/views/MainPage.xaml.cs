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
        // Dejamos el constructor limpio de suscripciones fijas para evitar fugas de memoria
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // 1. Forzamos la carga inicial en cuanto el usuario vuelve o entra a la pantalla
        await CargarDatosIniciales();

        // 2. 💥 LA CLAVE: Nos desuscribimos primero por seguridad (para no duplicar hilos) 
        // y nos suscribimos pasando 'this' como el suscriptor activo en este ciclo de vida.
        
        // Escucha si eliminas un producto desde el Perfil
        MessagingCenter.Unsubscribe<App>(this, "ActualizarMainPage");
        MessagingCenter.Subscribe<App>(this, "ActualizarMainPage", async (sender) =>
        {
            Debug.WriteLine("📢 [MainPage] Mensaje 'ActualizarMainPage' interceptado con éxito. Recargando...");
            await CargarDatosIniciales();
        });
        
        // Escucha si editas un producto desde EditarProductoPage
        MessagingCenter.Unsubscribe<App>(this, "ActualizarPerfil");
        MessagingCenter.Subscribe<App>(this, "ActualizarPerfil", async (sender) =>
        {
            Debug.WriteLine("📢 [MainPage] Mensaje 'ActualizarPerfil' (Edición) interceptado. Recargando...");
            await CargarDatosIniciales();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // 🧼 BUENA PRÁCTICA: Cuando la MainPage pasa a segundo plano, quitamos la suscripción
        // para que no intente refrescar cosas de la interfaz si el usuario no la está viendo.
        MessagingCenter.Unsubscribe<App>(this, "ActualizarMainPage");
        MessagingCenter.Unsubscribe<App>(this, "ActualizarPerfil");
    }

    private async Task CargarDatosIniciales()
    {
        try 
        {
            Debug.WriteLine("Pidiendo productos para la MainPage...");
            var productos = await _apiService.ObtenerProductos();

            if (productos != null && productos.Count > 0)
            {
                // Nos aseguramos de modificar el Carrusel dentro del hilo principal de la UI
                MainThread.BeginInvokeOnMainThread(() => {
                    miCarrusel.ItemsSource = null; 
                    miCarrusel.ItemsSource = productos; 
                    Debug.WriteLine($"✅ Main: Datos refrescados de forma instantánea. Total: {productos.Count}");
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() => {
                    miCarrusel.ItemsSource = null; // Limpiamos por si se han borrado todos
                });
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

    // 🌟 LÍNEAS AÑADIDAS: Único método nuevo para dar soporte a la flecha de volver atrás
    private async void OnVolverAlLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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