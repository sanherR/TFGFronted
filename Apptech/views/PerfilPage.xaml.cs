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
    // 💡 IMPORTANTE: Asegúrate de que esta IP sea la misma que usas en el ApiService
    private readonly string baseUrl = "http://10.0.2.2:5062/"; 

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<Producto> Productos { get; set; } = new ObservableCollection<Producto>();
    public ObservableCollection<Producto> Favoritos { get; set; } = new ObservableCollection<Producto>();
    
    // Cambiamos a una propiedad con campo privado para que OnPropertyChanged funcione bien
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
        set { perfil_url = value; OnPropertyChanged(); }
    }

    public PerfilPage()
    {
        InitializeComponent();
        BindingContext = this;

        // ✅ SOLUCIÓN AL CONGELAMIENTO: 
        // No cargamos nada hasta que el componente esté visualmente listo
        this.Loaded += async (s, e) =>
        {
            await InicializarDatosAsync();
        };
        /*
        MessagingCenter.Subscribe<App>(this, "ActualizarPerfil", (sender) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await CargarProductos(); 
            });
        });
        */
    }
    protected override async void OnAppearing()
{
    base.OnAppearing();
    // Forzamos la limpieza y recarga cada vez que entramos a la pestaña
    await CargarProductos();
}
    private async Task InicializarDatosAsync()
{
    try 
    {
        Debug.WriteLine("--- INICIANDO CARGA DE PERFIL ---");
        
        // Verificamos si tenemos el token antes de disparar
        var token = Preferences.Get("token", "");
        Debug.WriteLine($"DEBUG: Token actual: {(string.IsNullOrEmpty(token) ? "VACÍO ❌" : "OK ✅")}");

        // Ejecuta las 3 cargas
        await Task.WhenAll(CargarPerfil(), CargarFavoritos(), CargarProductos());
        
        Debug.WriteLine("--- CARGA FINALIZADA SIN ERRORES CRÍTICOS ---");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ ERROR GLOBAL EN PERFIL: {ex.Message}");
    }
}

    // --- COMANDOS ---

    public ICommand MostrarProductosCommand => new Command(() => ItemsActivos = Productos);

    public ICommand MostrarFavoritosCommand => new Command(() => ItemsActivos = Favoritos);

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

    // --- MÉTODOS DE CARGA ---

    public async Task CargarProductos()
{
    try 
    {
        var token = Preferences.Get("token", "");
        var productosRecibidos = await _apiService.ObtenerProductosUsuario(token);

        if (productosRecibidos != null)
        {
            // 1. Limpiamos las listas
            Productos.Clear();
            
            foreach (var p in productosRecibidos) 
            {
                // 2. Filtro: Si Vendido es 2, es que ya se completó la venta.
                // Los que son 0 (disponibles) o 1 (reservados) deben salir en tu perfil.
                if (p.Vendido != 2) 
                {
                    p.ImagenUrl = FixUrl(p.ImagenUrl);
                    Productos.Add(p);
                }
            }
            
            // 3. LA CLAVE: Asignamos a ItemsActivos para que la UI se entere
            ItemsActivos = new ObservableCollection<Producto>(Productos);
            
            Debug.WriteLine($"✅ Productos cargados en perfil: {ItemsActivos.Count}");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine("❌ Error al cargar perfil: " + ex.Message);
    }
}

    public async Task CargarFavoritos()
    {
        var token = Preferences.Get("token", "");
        var favoritos = await _apiService.ObtenerFavoritos(token);

        if (favoritos != null)
        {
            Favoritos.Clear();
            foreach (var f in favoritos)
            {
                f.ImagenUrl = FixUrl(f.ImagenUrl);
                Favoritos.Add(f);
            }
        }
    }

    public async Task CargarPerfil()
{
    var token = Preferences.Get("token", "");
    var perfil = await _apiService.ObtenerPerfil(token);

    if (perfil == null) 
    {
        System.Diagnostics.Debug.WriteLine("⚠️ CargarPerfil: El objeto 'perfil' llegó NULL desde la API.");
        return;
    }

    System.Diagnostics.Debug.WriteLine($"✅ CargarPerfil: Datos recibidos para {perfil.Nombre}");

    NombreUsuario = perfil.Nombre;
    OnPropertyChanged(nameof(NombreUsuario));
        
        if (string.IsNullOrEmpty(perfil.PerfilUrl))
        {
            Perfil_url = ImageSource.FromFile("perfil_default.png");
        }
        else
        {
            // FixUrl también para la imagen de perfil si viene relativa
            Perfil_url = ImageSource.FromUri(new Uri(FixUrl(perfil.PerfilUrl)));
        }
    }

    private string FixUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        if (url.StartsWith("http")) return url;
        return $"{baseUrl}{url.TrimStart('/')}";
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
        // 1. Borramos de la lista general
        if (Productos.Contains(producto)) Productos.Remove(producto);
        
        // 2. ¡CLAVE!: Borramos de la lista que la pantalla está renderizando realmente
        if (ItemsActivos.Contains(producto)) ItemsActivos.Remove(producto);
        
        // 3. Avisamos a la MainPage de que este producto ya no existe
        MessagingCenter.Send<App>((App)Application.Current, "ActualizarMainPage");
        
        await Application.Current.MainPage.DisplayAlert("Éxito", "Producto eliminado correctamente", "OK");
    }
}

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}