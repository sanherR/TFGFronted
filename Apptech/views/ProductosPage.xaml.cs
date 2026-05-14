using Apptech.Services;
using Apptech.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Apptech.views;

public partial class ProductosPage : ContentView
{
    private readonly ApiService _apiService = new ApiService();

    // 1. DEFINICIÓN DE PROPIEDADES (Bindable)
    public static readonly BindableProperty MasPopularesProperty = 
        BindableProperty.Create(nameof(MasPopulares), typeof(ObservableCollection<ItemPop>), typeof(ProductosPage), new ObservableCollection<ItemPop>());

    public static readonly BindableProperty NovedadesProperty = 
        BindableProperty.Create(nameof(Novedades), typeof(ObservableCollection<ItemPop>), typeof(ProductosPage), new ObservableCollection<ItemPop>());

    public ObservableCollection<ItemPop> MasPopulares
    {
        get => (ObservableCollection<ItemPop>)GetValue(MasPopularesProperty);
        set => SetValue(MasPopularesProperty, value);
    }

    public ObservableCollection<ItemPop> Novedades
    {
        get => (ObservableCollection<ItemPop>)GetValue(NovedadesProperty);
        set => SetValue(NovedadesProperty, value);
    }

    public ProductosPage()
    {
        InitializeComponent();
        
        // El BindingContext debe ser la propia clase para que el XAML vea las propiedades
        this.BindingContext = this;

        // Carga inicial cuando se monta el componente
        Loaded += async (s, e) => await CargarProductosAsync();

        // Suscripción para refrescar datos cuando se venda algo
        MessagingCenter.Subscribe<VenderPage>(this, "REFRESH_PRODUCTOS", async (s) => await CargarProductosAsync());
    }

    // 2. CARGA DE DATOS DESDE LA API
    // 2. CARGA DE DATOS DESDE LA API
// 2. CARGA DE DATOS DESDE LA API
public async Task CargarProductosAsync()
{
    try
    {
        // Obtenemos la lista de la API
        var productosOriginales = await _apiService.ObtenerProductos();
        
        if (productosOriginales == null || !productosOriginales.Any()) return;

        // Invertimos la lista para mostrar novedades primero
        var productosInvertidos = productosOriginales.AsEnumerable().Reverse().ToList();

        MainThread.BeginInvokeOnMainThread(() => 
        {
            Novedades.Clear();
            MasPopulares.Clear();

            foreach (var p in productosInvertidos)
            {
                // SOLUCIÓN DEFINITIVA AL ERROR DE LA IMAGEN:
                // Creamos una variable de tipo ItemPop mapeada correctamente
                ItemPop itemParaAñadir = MapearAItemPop(p);

                // Añadimos a MasPopulares (Todos)
                MasPopulares.Add(itemParaAñadir);

                // Si estamos dentro de los primeros 8, añadimos a Novedades
                if (Novedades.Count < 8)
                {
                    Novedades.Add(itemParaAñadir);
                }
            }
            
            Debug.WriteLine($"✅ Listas actualizadas: {Novedades.Count} novedades.");
        });
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"❌ Error en CargarProductos: {ex.Message}");
    }
}

// Asegúrate de que esta función reciba "dynamic" o el tipo exacto que devuelve tu ApiService
private ItemPop MapearAItemPop(ItemPop p)
{
    // Como ya hemos arreglado ItemPop.cs, 'p' ya trae el Id y el UsuarioId correctos.
    // Solo entramos aquí para asegurarnos de que la URL de la imagen sea completa.
    if (!string.IsNullOrEmpty(p.ImagenUrl) && !p.ImagenUrl.StartsWith("http"))
    {
        p.ImagenUrl = $"http://10.0.2.2:5062{p.ImagenUrl}";
    }
    
    return p;
}

    // 4. NAVEGACIÓN AL DETALLE
    private async void OnProductoSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ItemPop item)
            return;

        // Limpiamos la selección inmediatamente para evitar doble clic
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        // Navegación (Asegúrate de que DetalleProductoPage acepte ItemPop en su constructor)
        await Navigation.PushAsync(new DetalleProductoPage(item));
    }
}