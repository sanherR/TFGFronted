using Apptech.Models;
using Apptech.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Apptech.views;

public partial class ListaChatsPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
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
            // Obtenemos el ID del usuario logueado
            int userId = Preferences.Get("userId", 0);
            if (userId == 0) return;

            // Llamada al endpoint: api/mensajes/mis-chats/{userId}
            var chats = await _apiService.ObtenerBandejaEntrada(userId);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MisChats.Clear();
                foreach (var chat in chats)
                {
                    // Ajustamos la URL de la imagen si es necesario
                    if (chat is { } c)
                    {
                        // Aquí podrías añadir lógica para formatear la URL de la imagen si el backend no la da completa
                        MisChats.Add(chat);
                    }
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
    System.Diagnostics.Debug.WriteLine($"Probando Chat: {chatSeleccionado.Id} - Producto: {chatSeleccionado.ProductoId}");
    // Deseleccionamos
    ((CollectionView)sender).SelectedItem = null;

    try
    {
        // IMPORTANTE: Pasamos los 3 datos. 
        // Si 'chatSeleccionado.ProductoId' es el ID real, el botón verde funcionará.
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