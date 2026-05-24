using Apptech.Models;
using Apptech.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Apptech.views;

public partial class ListaChatsPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    // Definimos la base aquí para reutilizarla
    private readonly string baseUrl = "https://tfgbacken-production.up.railway.app";
    
    public ObservableCollection<ChatBandeja> MisChats { get; set; } = new ObservableCollection<ChatBandeja>();

    public ListaChatsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarConversaciones();
    }

    private async Task CargarConversaciones()
    {
        try
        {
            int userId = Preferences.Get("userId", 0);
            if (userId == 0) return;

            var chats = await _apiService.ObtenerBandejaEntrada(userId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MisChats.Clear();
                foreach (var chat in chats)
                {
                    // 1. Arreglamos la URL de la imagen
                    if (!string.IsNullOrEmpty(chat.ImagenProductoUrl) && !chat.ImagenProductoUrl.StartsWith("http"))
                    {
                        chat.ImagenProductoUrl = $"{baseUrl}/{chat.ImagenProductoUrl.TrimStart('/')}";
                    }

                    // Nota: Si el backend envía NombreProducto y NombreUsuario, 
                    // esta línea ya los incluye al añadir el objeto a la colección.
                    MisChats.Add(chat);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error al cargar bandeja: {ex.Message}");
        }
    }

    private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
    {
        var chatSeleccionado = e.CurrentSelection.FirstOrDefault() as ChatBandeja;
        if (chatSeleccionado == null) return;

        // Deseleccionamos para que se pueda volver a clicar el mismo chat
        ((CollectionView)sender).SelectedItem = null;

        try
        {
            // Pasamos los datos al ChatPage
            await Navigation.PushAsync(new ChatPage(
                chatSeleccionado.Id, 
                chatSeleccionado.TituloChat, 
                chatSeleccionado.ProductoId
            ));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error al abrir el chat: {ex.Message}");
        }
    }
}