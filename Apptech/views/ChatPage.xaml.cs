using Apptech.Models;
using Apptech.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Apptech.views;

public partial class ChatPage : ContentPage, INotifyPropertyChanged
{
    private readonly ApiService _apiService = new ApiService();
    private int _chatId;
    private int _productoId;
    private int _vendedorId;
    private bool _isTimerActive;

    // --- PROPIEDADES BINDING (PONLAS AQUÍ) ---
    public ObservableCollection<Mensaje> Mensajes { get; set; } = new ObservableCollection<Mensaje>();
    
    private string _tituloChat;
    public string TituloChat { get => _tituloChat; set { _tituloChat = value; OnPropertyChanged(nameof(TituloChat)); } }

    // Estas son las nuevas para que no salgan los errores de "property not found"
    private string _nombreProducto;
    public string NombreProducto { get => _nombreProducto; set { _nombreProducto = value; OnPropertyChanged(nameof(NombreProducto)); } }

    private string _precioProducto;
    public string PrecioProducto { get => _precioProducto; set { _precioProducto = value; OnPropertyChanged(nameof(PrecioProducto)); } }

    private string _imagenProducto;
    public string ImagenProducto { get => _imagenProducto; set { _imagenProducto = value; OnPropertyChanged(nameof(ImagenProducto)); } }

    public bool MostrarInfoProducto => _chatId > 0;

    // --- CONSTRUCTORES ---

    public ChatPage()
    {
        InitializeComponent();
    }

    // Constructor para cuando vienes desde la LISTA DE CHATS (Bandeja de Entrada)
    public ChatPage(int chatId, string titulo, int productoId = 0)
{
    InitializeComponent();
    _chatId = chatId;
    _productoId = productoId;
    TituloChat = titulo;
    BindingContext = this;

    if (_productoId > 0)
    {
        // Ejecutamos la carga en segundo plano
        Task.Run(async () => await CargarDatosYConfigurar(_productoId));
    }
}

    // Constructor para cuando vienes de un producto (Botón Contactar)
    public ChatPage(ItemPop producto)
    {
        InitializeComponent();
        _productoId = producto.Id;
        _vendedorId = producto.UsuarioId;
        BindingContext = this;
        _ = IniciarChatDesdeProducto(producto);
    }

    // --- LÓGICA DE INICIO ---

    private async Task IniciarChatDesdeProducto(ItemPop producto)
    {
        TituloChat = producto.Nombre;
        _chatId = await _apiService.ObtenerOCrearChat(producto.UsuarioId, producto.Id);
        
        ConfigurarBotonReserva(); 
        await ActualizarListaMensajes();
    }

    private async Task CargarDatosYConfigurar(int idProducto)
{
    Debug.WriteLine($"DEBUG: Iniciando carga para producto ID: {idProducto}");
    
    var producto = await _apiService.GetProductoById(idProducto);
    
    if (producto != null)
{
    _vendedorId = producto.UsuarioId;
    int estadoVendido = producto.Vendido; 

    NombreProducto = producto.Nombre;
    PrecioProducto = $"{producto.Precio}€";

    // --- AQUÍ ESTÁ EL CAMBIO ---
    // Usamos la lógica de tu modelo Producto para asegurar que la URL sea completa
    if (!string.IsNullOrEmpty(producto.ImagenUrl) && !producto.ImagenUrl.StartsWith("http"))
    {
        ImagenProducto = $"https://tfgbacken-production.up.railway.app{producto.ImagenUrl}";
    }
    else
    {
        ImagenProducto = producto.ImagenUrl; // Ya es completa o está vacía
    }
    // ----------------------------

    Debug.WriteLine($"DEBUG: URL final de la imagen: {ImagenProducto}");

    MainThread.BeginInvokeOnMainThread(() =>
    {
        ConfigurarBotonReserva(estadoVendido);
    });
}
}

   private void ConfigurarBotonReserva(int estadoVendido = 0)
{
    int miId = Preferences.Get("userId", 0);
    bool soyVendedor = miId == _vendedorId;

    // Reset de visibilidad
    BotonReservar.IsVisible = false;
    GridAccionesVendedor.IsVisible = false;
    LabelVendido.IsVisible = false;

    if (soyVendedor)
    {
        switch (estadoVendido)
        {
            case 0: // Disponible
                BotonReservar.IsVisible = true;
                BotonReservar.Text = "ACEPTAR RESERVA";
                BotonReservar.BackgroundColor = Colors.Green;
                break;
            case 1: // Reservado
                GridAccionesVendedor.IsVisible = true; // Mostramos los dos botones nuevos
                break;
            case 2: // Vendido
                LabelVendido.IsVisible = true;
                break;
        }
    }
    else // Lógica para el Comprador
    {
        if (estadoVendido == 2)
        {
            LabelVendido.IsVisible = true;
            LabelVendido.Text = "PRODUCTO VENDIDO";
        }
        else if (estadoVendido == 1)
        {
            BotonReservar.IsVisible = true;
            BotonReservar.Text = "PRODUCTO RESERVADO";
            BotonReservar.IsEnabled = false;
            BotonReservar.BackgroundColor = Colors.Gray;
        }
        else
        {
            BotonReservar.IsVisible = true;
            BotonReservar.Text = "SOLICITAR RESERVA";
            BotonReservar.BackgroundColor = Colors.DarkBlue;
        }
    }
}
private async void OnConfirmarVentaClicked(object sender, EventArgs e)
{
    bool confirmar = await DisplayAlert("Confirmar Venta", 
        "¿Confirmas que has vendido el producto? Ya no aparecerá en la tienda.", 
        "Sí, vendido", "Cancelar");

    if (confirmar)
    {
        var exito = await _apiService.ConfirmarVentaProducto(_productoId);
        if (exito)
        {
            await _apiService.EnviarMensaje(_chatId, "SISTEMA: ¡El vendedor ha confirmado la venta final!");
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConfigurarBotonReserva(2); // Cambia la interfaz a modo "Vendido"
            });

            await ActualizarListaMensajes();
            await DisplayAlert("¡Enhorabuena!", "Venta finalizada con éxito", "OK");
        }
        else
        {
            await DisplayAlert("Error", "No se pudo marcar como vendido", "OK");
        }
    }
}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isTimerActive = false; 
    }
    protected override async void OnAppearing()
{
    base.OnAppearing();
    _isTimerActive = true;
    await ActualizarListaMensajes();

    // Timer para refrescar el chat automáticamente
    Device.StartTimer(TimeSpan.FromSeconds(3), () => {
        if (_isTimerActive) {
            _ = ActualizarListaMensajes();
            return true; 
        }
        return false;
    });
}

    private async Task ActualizarListaMensajes()
    {
        if (_chatId <= 0) return;

        var nuevosMensajes = await _apiService.ObtenerMensajes(_chatId);
        
        if (nuevosMensajes != null && nuevosMensajes.Count != Mensajes.Count)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                Mensajes.Clear();
                foreach (var m in nuevosMensajes) Mensajes.Add(m);
                
                if (Mensajes.Count > 0)
                    MessagesList.ScrollTo(Mensajes.Count - 1);
            });
        }
    }

    // --- ACCIONES DE USUARIO ---

    private async void OnEnviarClicked(object sender, EventArgs e)
    {
        string texto = TxtMensaje.Text;
        if (string.IsNullOrWhiteSpace(texto) || _chatId <= 0) return;

        TxtMensaje.Text = string.Empty;
        bool enviado = await _apiService.EnviarMensaje(_chatId, texto);
        
        if (enviado) await ActualizarListaMensajes();
        else await DisplayAlert("Error", "No se pudo enviar el mensaje", "OK");
    }

    private async void OnReservarClicked(object sender, EventArgs e)
{
    int miId = Preferences.Get("userId", 0);

    // --- CASO VENDEDOR ---
    if (miId == _vendedorId)
    {
        // 1. LÓGICA DE ANULAR (Si el Grid de los dos botones está visible)
        if (GridAccionesVendedor.IsVisible) 
        {
            bool confirmar = await DisplayAlert("Anular Reserva", "¿Quieres volver a poner el producto a la venta?", "Sí, anular", "No");
            if (!confirmar) return;

            var exito = await _apiService.CancelarReservaProducto(_productoId);
            if (exito)
            {
                await _apiService.EnviarMensaje(_chatId, "SISTEMA: El vendedor ha anulado la reserva.");
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ConfigurarBotonReserva(0); // 0 lo vuelve a poner en "Aceptar Reserva"
                });

                await ActualizarListaMensajes();
                await DisplayAlert("Éxito", "Reserva anulada correctamente", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo anular la reserva en el servidor", "OK");
            }
            return; 
        }

        // 2. LÓGICA DE ACEPTAR (Si solo está el botón de "Aceptar Reserva")
        bool confirmarReserva = await DisplayAlert("Confirmar", "¿Aceptar la reserva para este usuario?", "Sí, aceptar", "No");
        if (!confirmarReserva) return;

        int compradorId = await ObtenerIdDelOtroUsuario();
        if (compradorId == 0) return;

        bool ok = await _apiService.AceptarReservaProducto(_productoId, compradorId);
        if (ok)
        {
            await _apiService.EnviarMensaje(_chatId, "SISTEMA: ¡Reserva aceptada!");
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConfigurarBotonReserva(1); // 1 muestra los botones de "Vender" y "Anular"
            });

            await ActualizarListaMensajes();
            await DisplayAlert("Éxito", "Has aceptado la reserva", "OK");
        }
    }
    // --- CASO COMPRADOR ---
    else
    {
        bool solicitar = await DisplayAlert("Solicitar", "¿Enviar solicitud de reserva al vendedor?", "Sí", "No");
        if (!solicitar) return;

        bool enviado = await _apiService.EnviarMensaje(_chatId, "SOLICITUD: Hola, me gustaría reservar este producto. ¿Es posible?");
        if (enviado)
        {
            await ActualizarListaMensajes();
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BotonReservar.Text = "Solicitud Enviada";
                BotonReservar.IsEnabled = false;
                BotonReservar.BackgroundColor = Colors.Gray;
            });
        }
    }
}



    private async Task<int> ObtenerIdDelOtroUsuario()
    {
        var mensajes = await _apiService.ObtenerMensajes(_chatId);
        if (mensajes == null || mensajes.Count == 0) return 0;

        int miId = Preferences.Get("userId", 0);
        
        // El otro usuario es el que tiene un EmisorId distinto al mío
        var otro = mensajes.FirstOrDefault(m => m.EmisorId != miId);
        return otro?.EmisorId ?? 0;
    }

    // --- NOTIFICACIÓN DE CAMBIOS ---
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}