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
    private readonly string baseUrl = "https://tfgbacken-production.up.railway.app"; 

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
    // Añade esta línea debajo de tus colecciones de Productos y Favoritos:
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
            OnPropertyChanged(); // Esto avisa a la interfaz XAML que debe repintar la imagen
        }
    }
}
public async Task CargarVentas()
{
    try
    {
        // Usamos el token en lugar del userId
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
    catch (Exception ex) { Debug.WriteLine("❌ Error al cargar ventas: " + ex.Message); }
}
    // CONTROLADORES DE INTERFAZ: Para gestionar dinámicamente los botones de Editar/Eliminar
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
    await Task.WhenAll(CargarProductos(), CargarFavoritos(), CargarVentas());
}

    private async Task InicializarDatosAsync()
{
    try 
    {
        Debug.WriteLine("--- INICIANDO CARGA DE PERFIL ---");
        
        var token = Preferences.Get("token", "");
        Debug.WriteLine($"DEBUG: Token actual: {(string.IsNullOrEmpty(token) ? "VACÍO ❌" : "OK ✅")}");

        // ✅ AÑADE CargarVentas() AQUÍ dentro del Task.WhenAll
        await Task.WhenAll(CargarPerfil(), CargarFavoritos(), CargarProductos(), CargarVentas());
        
        Debug.WriteLine("--- CARGA FINALIZADA SIN ERRORES CRÍTICOS ---");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ ERROR GLOBAL EN PERFIL: {ex.Message}");
    }
}

    // --- COMANDOS ---

    public ICommand MostrarProductosCommand => new Command(() => 
    {
        ItemsActivos = Productos;
        MostrarBotonesAccion = true; // Activa los botones cuando miras tus productos
    });

    public ICommand MostrarFavoritosCommand => new Command(() => 
    {
        ItemsActivos = Favoritos;
        MostrarBotonesAccion = false; // ¡Oculta los botones cuando miras favoritos!
    });
    public ICommand MostrarVentasCommand => new Command(() => 
{
    ItemsActivos = Ventas;
    MostrarBotonesAccion = false; // No debe haber edición en el historial de ventas
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

    // COMANDO ACTUALIZADO CON TODOS LOS CAMPOS PARA EL DETALLE SANO Y SALVO
    public ICommand VerDetalleProductoCommand => new Command<Producto>(async (producto) =>
    {
        if (producto == null) return;

        Debug.WriteLine($"[MAUI DEBUG] Abriendo detalle de: {producto.Nombre}");
        Debug.WriteLine($"-> ID Producto: {producto.Id} | Vendedor/Usuario ID: {producto.UsuarioId}");

        // Transformamos el modelo pasando absolutamente todas las propiedades
        // Transformamos el modelo pasando absolutamente todas las propiedades
var itemPop = new ItemPop
{
    Id = producto.Id,
    Nombre = producto.Nombre,
    Precio = (decimal)producto.Precio, // Asegúrate de castear si el tipo difiere
    // AQUÍ ESTÁ EL CAMBIO: Asignamos a ImagenUrlBase, no a ImagenUrl
    ImagenUrlBase = producto.ImagenUrl, 
    UsuarioId = producto.UsuarioId,
    Descripcion = producto.Descripcion,
    Estado = producto.Estado,
    Caracteristicas = producto.Caracteristicas, 
    EsFavorito = true 
};

        // Redirección directa pasándole el objeto completo mapeado sin ceros intermedios
        await Navigation.PushAsync(new DetalleProductoPage(itemPop));
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
            // 1. Limpiamos la colección principal
            Productos.Clear();
            
            foreach (var p in productosRecibidos) 
            {
                // 2. Filtro: Solo mostramos disponibles (0) o reservados (1)
                // Los vendidos (2) no deben aparecer aquí, ya están en el historial de ventas
                if (p.Vendido != 2) 
                {
                    p.ImagenUrlBase = FixUrl(p.ImagenUrl);
                    Productos.Add(p);
                }
            }
            
            // 3. ACTUALIZACIÓN SEGURA DE LA VISTA:
            // Si el usuario está viendo "Mis Productos" (MostrarBotonesAccion == true),
            // refrescamos la lista que se está renderizando en pantalla.
            if (MostrarBotonesAccion)
            {
                // Creamos una nueva referencia para asegurar que el Binding se dispare
                ItemsActivos = new ObservableCollection<Producto>(Productos);
            }
            
            Debug.WriteLine($"✅ Productos cargados en perfil: {Productos.Count}");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error al cargar productos: {ex.Message}");
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
                    // ❌ ELIMINA ESTA LÍNEA: f.ImagenUrlBase = FixUrl(f.ImagenUrl);
                    // ✅ AÑADE EL PRODUCTO TAL CUAL VIENE DE LA API
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
            Debug.WriteLine("❌ Error al cargar favoritos: " + ex.Message);
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

    // 1. Actualizamos el nombre
    NombreUsuario = perfil.Nombre;
    OnPropertyChanged(nameof(NombreUsuario));
            
    // 2. Gestión de la imagen con prioridad: foto_perfil (FotoPerfil) es la columna correcta
    string urlAUsar = !string.IsNullOrEmpty(perfil.FotoPerfil) ? perfil.FotoPerfil : perfil.PerfilUrl;

    if (string.IsNullOrEmpty(urlAUsar))
    {
        Debug.WriteLine("⚠️ No hay URL de imagen disponible, usando default.");
        Perfil_url = ImageSource.FromFile("perfil_default.png");
    }
    else
    {
        try 
        {
            // Usamos FixUrl para asegurar dominio + formato correcto
            // Nota: Asegúrate de que tu FixUrl use .TrimStart('/') para evitar dobles barras
            string urlFinal = FixUrl(urlAUsar);
            Debug.WriteLine($"🌐 Cargando imagen final desde: {urlFinal}");
            
            // Asignamos la imagen (Forzamos la creación del objeto Uri)
            Perfil_url = ImageSource.FromUri(new Uri(urlFinal));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error al procesar la URL de la imagen: {ex.Message}");
            Perfil_url = ImageSource.FromFile("perfil_default.png");
        }
    }

    // 3. Avisamos explícitamente a la interfaz del cambio
    OnPropertyChanged(nameof(Perfil_url));
}

    private string FixUrl(string url)
{
    // 1. Si está vacío o es nulo, devuelve un placeholder o vacío
    if (string.IsNullOrEmpty(url)) 
        return "perfil_default.png"; 

    // 2. Si ya es una URL completa (http...), devuélvela tal cual
    if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) 
        return url;

    // 3. Garantizamos: BASE_URL + "/" + RUTA_LIMPIA
    // .TrimEnd('/') quita la barra del final de la base (si existe)
    // .TrimStart('/') quita la barra del inicio de la ruta (si existe)
    // Esto asegura que siempre haya exactamente una barra separadora.
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