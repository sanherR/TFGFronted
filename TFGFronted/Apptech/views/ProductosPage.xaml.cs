namespace Apptech.views;

using System.Collections.ObjectModel;
using Apptech.Services;
using Apptech.Models;

public partial class ProductosPage : ContentView
{
    private readonly ApiService _apiService = new ApiService();

    public ObservableCollection<ItemPop> Recomendados { get; set; } = new();
    public ObservableCollection<ItemPop> MasPopulares { get; set; } = new();
    public ObservableCollection<ItemPop> Novedades { get; set; } = new();

    public ProductosPage()
    {
        InitializeComponent();
        this.BindingContext = this;

        Loaded += async (s, e) =>
        {
            await CargarProductosAsync();
        };

        MessagingCenter.Subscribe<VenderPage>(this, "REFRESH_PRODUCTOS", async (s) =>
        {
            await CargarProductosAsync();
        });
    }

   private async void OnComprarClicked(object sender, EventArgs e)
{
    var button = sender as Button;
    var producto = button?.CommandParameter as ItemPop;

    if (producto == null) return;

    // AÑADE ESTA LÍNEA PARA DEPURAR:
    Console.WriteLine($"DEBUG: Intentando comprar Producto ID: {producto.Id}, Vendedor ID: {producto.VendedorId}");

    bool answer = await Application.Current.MainPage.DisplayAlert(
        "Confirmar", 
        $"¿Quieres comprar {producto.Nombre}? (ID: {producto.Id})", // Mostramos el ID en el mensaje
        "Sí", 
        "No");

        if (answer)
        {
            try
            {
                var client = new HttpClient();
                var pedidoService = new PedidoService(client);
                bool exito = await pedidoService.RealizarCompraAsync(producto.Id, producto.VendedorId, (decimal)producto.precio);

                if (exito)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "¡Pedido realizado correctamente!", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El servidor rechazó la compra. Revisa tu sesión.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
            }
        }
    }

    public async Task CargarProductosAsync()
    {
        try
        {
            var productos = await _apiService.ObtenerProductos();

            Recomendados.Clear();
            MasPopulares.Clear();
            Novedades.Clear();

            foreach (var p in productos)
            {
                var item = new ItemPop
                {
                    Id = p.Id,
                    VendedorId = p.UsuarioId,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion ?? "",
                    precio = (int)p.Precio,
                    ImagenUrl = "http://192.168.1.36:5062" + p.ImagenUrl 
                };

                switch (p.Grupo)
                {
                    case "Recomendados":
                        Recomendados.Add(item);
                        break;
                    case "Más Populares":
                        MasPopulares.Add(item);
                        break;
                    case "Novedades":
                        Novedades.Add(item);
                        break;
                    default:
                        Recomendados.Add(item);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los productos: {ex.Message}", "OK");
        }
    } // <-- Esta cierra el método CargarProductosAsync
} // <-- ESTA ES LA QUE TE FALTABA (Cierra la clase)