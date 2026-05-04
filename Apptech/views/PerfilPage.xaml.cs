using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Storage;
using Apptech.Services;
using Apptech.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Apptech.views;

public partial class PerfilPage : ContentView, INotifyPropertyChanged
{
    private readonly ApiService _apiService = new ApiService();
    private readonly string baseUrl = "http://192.168.1.137:5062/";

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<Producto> Productos { get; set; } = new ObservableCollection<Producto>();
    public ObservableCollection<Producto> Favoritos { get; set; } = new ObservableCollection<Producto>();
    public ObservableCollection<Producto> ItemsActivos { get; set; } = new ObservableCollection<Producto>();

    private string nombreUsuario;
    public string NombreUsuario
    {
        get => nombreUsuario;
        set
        {
            nombreUsuario = value;
            OnPropertyChanged();
        }
    }

    private ImageSource perfil_url;
    public ImageSource Perfil_url
    {
        get => perfil_url;
        set
        {
            perfil_url = value;
            OnPropertyChanged();
        }
    }

    public PerfilPage()
{
    InitializeComponent();
    BindingContext = this;
    _ = CargarFavoritos();
    _ = CargarPerfil();
    _ = CargarProductos();

    // 🔥 ESCUCHA LA SEÑAL: Cuando llegue "ActualizarPerfil", ejecuta CargarProductos de nuevo
    MessagingCenter.Subscribe<App>(this, "ActualizarPerfil", (sender) =>
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await CargarProductos(); // Esto limpia la lista y trae los nuevos del server
        });
    });
}

    public ICommand MostrarProductosCommand => new Command(() =>
    {
        ItemsActivos = Productos;
        OnPropertyChanged(nameof(ItemsActivos));
    });

    public ICommand MostrarFavoritosCommand => new Command(() =>
    {
        ItemsActivos = Favoritos;
        OnPropertyChanged(nameof(ItemsActivos));
    });

    public ICommand EditarProductoCommand => new Command<Producto>(async (producto) =>
    {
        await Navigation.PushAsync(new EditarProductoPage(producto));
    });

    public ICommand EliminarProductoCommand => new Command<Producto>(async (producto) =>
    {
        await EliminarProducto(producto);
    });

    public async Task CargarProductos()
    {
        var token = Preferences.Get("token", "");
        var productos = await _apiService.ObtenerProductosUsuario(token);

        Productos.Clear();
        foreach (var producto in productos)
        {
            producto.ImagenUrl = FixUrl(producto.ImagenUrl);
            Productos.Add(producto);
        }
        
        ItemsActivos = Productos;
        OnPropertyChanged(nameof(ItemsActivos));
    }
    public async Task CargarFavoritos()
{
    var token = Preferences.Get("token", "");

    var favoritos = await _apiService.ObtenerFavoritos(token); // TE FALTA ESTE MÉTODO

    Favoritos.Clear();

    foreach (var f in favoritos)
    {
        f.ImagenUrl = FixUrl(f.ImagenUrl);
        Favoritos.Add(f);
    }
}

    private string FixUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return "";

        if (url.StartsWith("http"))
            return url;

        return $"{baseUrl}{url.TrimStart('/')}";
    }

    public async Task CargarPerfil()
    {
        var token = Preferences.Get("token", "");
        var perfil = await _apiService.ObtenerPerfil(token);

        if (perfil == null) return;

        NombreUsuario = perfil.Nombre;
        Perfil_url = string.IsNullOrEmpty(perfil.PerfilUrl)
            ? ImageSource.FromFile("perfil_default.png")
            : ImageSource.FromUri(new Uri(perfil.PerfilUrl));
            
        System.Diagnostics.Debug.WriteLine($"URL PERFIL: {perfil.PerfilUrl}");
    }

    public async void CambiarFoto(object sender, EventArgs e)
    {
        var token = Preferences.Get("token", "");

        if (string.IsNullOrEmpty(token))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Debes iniciar sesión", "OK");
            return;
        }

        var archivo = await MediaPicker.Default.PickPhotoAsync();
        if (archivo == null) return;

        using var stream = await archivo.OpenReadAsync();
        var url = await _apiService.SubirImagenPerfil(stream, archivo.FileName, token);

        if (string.IsNullOrEmpty(url))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo subir la imagen", "OK");
            return;
        }

        Preferences.Set("PerfilUrl", url);
        Perfil_url = ImageSource.FromUri(new Uri(url));
    }

    private async Task EliminarProducto(Producto producto)
    {
        var success = await _apiService.EliminarProducto(producto.Id);

        if (success)
        {
            ItemsActivos.Remove(producto);
            Productos.Remove(producto); // Lo quitamos también de la lista base por si acaso
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo eliminar el producto", "OK");
        }
    }

    void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}