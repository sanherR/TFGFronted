using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;
using Apptech.Services;
using Apptech.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;

namespace Apptech.views;

public partial class PerfilPage : ContentPage, INotifyPropertyChanged
{
    private readonly ApiService _apiService = new ApiService();
    
    private readonly string baseUrl = "https://tfgbacken-production.up.railway.app"; 

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<Producto> Productos { get; set; } = new ObservableCollection<Producto>();
    public ObservableCollection<Producto> Favoritos { get; set; } = new ObservableCollection<Producto>();
    
    
    private ObservableCollection<Producto> _itemsActivos = new ObservableCollection<Producto>();
    public ObservableCollection<Producto> ItemsActivos
    {
        get => _itemsActivos;
        set
        {
            _itemsActivos = value;
            OnPropertyChanged();
        }
    }
    
public ObservableCollection<Producto> Ventas { get; set; } = new ObservableCollection<Producto>();


    private string nombreUsuario;
    public string NombreUsuario
    {
        get => nombreUsuario;
        set { nombreUsuario = value; OnPropertyChanged(); }
    }

    private ImageSource perfil_url;
public ImageSource Perfil_url
{
    get => perfil_url;
    set 
    { 
        if (perfil_url != value)
        {
            perfil_url = value;
            OnPropertyChanged(); 
        }
    }
}
public async Task CargarVentas()
{
    try
    {
        
        var token = Preferences.Get("token", "");
        var ventasRecibidas = await _apiService.ObtenerProductosVendidos(token);
        
        if (ventasRecibidas != null)
        {
            Ventas.Clear();
            foreach (var v in ventasRecibidas)
            {
                v.ImagenUrlBase = FixUrl(v.ImagenUrl);
                Ventas.Add(v);
            }
        }
    }
    catch (Exception ex) { Debug.WriteLine(" Error al cargar ventas: " + ex.Message); }
}
    
    private bool _mostrarBotonesAccion = true;
    public bool MostrarBotonesAccion
    {
        get => _mostrarBotonesAccion;
        set { _mostrarBotonesAccion = value; OnPropertyChanged(); }
    }

    public PerfilPage()
    {
        InitializeComponent();
        BindingContext = this;

        
        this.Loaded += async (s, e) =>
        {
            await InicializarDatosAsync();
        };
        
    }

    protected override async void OnAppearing()
{
    base.OnAppearing();
    await Task.WhenAll(CargarProductos(), CargarFavoritos(), CargarVentas());
}

    private async Task InicializarDatosAsync()
{
    try 
    {
        Debug.WriteLine("--- INICIANDO CARGA DE PERFIL ---");
        
        var token = Preferences.Get("token", "");
        Debug.WriteLine($"DEBUG: Token actual: {(string.IsNullOrEmpty(token) ? "VACÍO ❌" : "OK ✅")}");

        
        await Task.WhenAll(CargarPerfil(), CargarFavoritos(), CargarProductos(), CargarVentas());
        
        Debug.WriteLine("--- CARGA FINALIZADA SIN ERRORES CRÍTICOS ---");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ ERROR GLOBAL EN PERFIL: {ex.Message}");
    }
}

    

    public ICommand MostrarProductosCommand => new Command(() => 
    {
        ItemsActivos = Productos;
        MostrarBotonesAccion = true; 
    });

    public ICommand MostrarFavoritosCommand => new Command(() => 
    {
        ItemsActivos = Favoritos;
        MostrarBotonesAccion = false; 
    });
    public ICommand MostrarVentasCommand => new Command(() => 
{
    ItemsActivos = Ventas;
    MostrarBotonesAccion = false; 
});


    public ICommand EditarProductoCommand => new Command<Producto>(async (producto) =>
    {
        if (producto != null)
            await Navigation.PushAsync(new EditarProductoPage(producto));
    });

    public ICommand EliminarProductoCommand => new Command<Producto>(async (producto) =>
    {
        if (producto != null)
            await EliminarProducto(producto);
    });

   
    public ICommand VerDetalleProductoCommand => new Command<Producto>(async (producto) =>
    {
        if (producto == null) return;

        Debug.WriteLine($"[MAUI DEBUG] Abriendo detalle de: {producto.Nombre}");
        Debug.WriteLine($"-> ID Producto: {producto.Id} | Vendedor/Usuario ID: {producto.UsuarioId}");

       
var itemPop = new ItemPop
{
    Id = producto.Id,
    Nombre = producto.Nombre,
    Precio = (decimal)producto.Precio, 
    ImagenUrlBase = producto.ImagenUrl, 
    UsuarioId = producto.UsuarioId,
    Descripcion = producto.Descripcion,
    Estado = producto.Estado,
    Caracteristicas = producto.Caracteristicas, 
    EsFavorito = true 
};

        
        await Navigation.PushAsync(new DetalleProductoPage(itemPop));
    });

    

   public async Task CargarProductos()
{
    try 
    {
        var token = Preferences.Get("token", "");
        var productosRecibidos = await _apiService.ObtenerProductosUsuario(token);

        if (productosRecibidos != null)
        {
            
            Productos.Clear();
            
            foreach (var p in productosRecibidos) 
            {
                
                if (p.Vendido != 2) 
                {
                    p.ImagenUrlBase = FixUrl(p.ImagenUrl);
                    Productos.Add(p);
                }
            }
            
           
            if (MostrarBotonesAccion)
            {
                
                ItemsActivos = new ObservableCollection<Producto>(Productos);
            }
            
            Debug.WriteLine($" Productos cargados en perfil: {Productos.Count}");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($" Error al cargar productos: {ex.Message}");
    }
}

    public async Task CargarFavoritos()
    {
        try
        {
            var token = Preferences.Get("token", "");
            var favoritos = await _apiService.ObtenerFavoritos(token);

            if (favoritos != null)
            {
                Favoritos.Clear();
                foreach (var f in favoritos)
                {
                    
                    Favoritos.Add(f);
                }

                if (!MostrarBotonesAccion)
                {
                    ItemsActivos = new ObservableCollection<Producto>(Favoritos);
                }
                Debug.WriteLine($"✅ Favoritos cargados: {Favoritos.Count}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(" Error al cargar favoritos: " + ex.Message);
        }
    }

    public async Task CargarPerfil()
{
    var token = Preferences.Get("token", "");
    var perfil = await _apiService.ObtenerPerfil(token);

    if (perfil == null) 
    {
        Debug.WriteLine("⚠️ CargarPerfil: El objeto 'perfil' llegó NULL desde la API.");
        return;
    }

    Debug.WriteLine($"✅ CargarPerfil: Datos recibidos para {perfil.Nombre}.");
    Debug.WriteLine($"   URL Perfil: {perfil.PerfilUrl} | Foto Perfil: {perfil.FotoPerfil}");

    
    NombreUsuario = perfil.Nombre;
    OnPropertyChanged(nameof(NombreUsuario));
            
    
    string urlAUsar = !string.IsNullOrEmpty(perfil.FotoPerfil) ? perfil.FotoPerfil : perfil.PerfilUrl;

    if (string.IsNullOrEmpty(urlAUsar))
    {
        Debug.WriteLine(" No hay URL de imagen disponible, usando default.");
        Perfil_url = ImageSource.FromFile("perfil_default.png");
    }
    else
    {
        try 
        {
            
            string urlFinal = FixUrl(urlAUsar);
            Debug.WriteLine($" Cargando imagen final desde: {urlFinal}");
            
            
            Perfil_url = ImageSource.FromUri(new Uri(urlFinal));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" Error al procesar la URL de la imagen: {ex.Message}");
            Perfil_url = ImageSource.FromFile("perfil_default.png");
        }
    }

    
    OnPropertyChanged(nameof(Perfil_url));
}

    private string FixUrl(string url)
{
    
    if (string.IsNullOrEmpty(url)) 
        return "perfil_default.png"; 

    
    if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) 
        return url;

    
    return $"{baseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
}

    public async void CambiarFoto(object sender, EventArgs e)
    {
        var token = Preferences.Get("token", "");
        if (string.IsNullOrEmpty(token)) return;

        var archivo = await MediaPicker.Default.PickPhotoAsync();
        if (archivo == null) return;

        using var stream = await archivo.OpenReadAsync();
        var url = await _apiService.SubirImagenPerfil(stream, archivo.FileName, token);

        if (!string.IsNullOrEmpty(url))
        {
            Preferences.Set("PerfilUrl", url);
            Perfil_url = ImageSource.FromUri(new Uri(FixUrl(url)));
        }
    }

    private async Task EliminarProducto(Producto producto)
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert("Eliminar", "¿Estás seguro?", "Sí", "No");
        if (!confirm) return;

        var success = await _apiService.EliminarProducto(producto.Id);
        if (success)
        {
            
            if (Productos.Contains(producto)) Productos.Remove(producto);
            
            
            if (ItemsActivos.Contains(producto)) ItemsActivos.Remove(producto);
            
            
            MessagingCenter.Send<App>((App)Application.Current, "ActualizarMainPage");
            
            await Application.Current.MainPage.DisplayAlert("Éxito", "Producto eliminado correctamente", "OK");
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}