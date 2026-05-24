using Apptech.Services;
using Apptech.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input; // IMPORTANTE para ICommand

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

    // NUEVO: Comando para el RefreshView
    public ICommand ActualizarProductosCommand { get; }

    public ProductosPage()
{
    InitializeComponent();
    
    // El BindingContext debe ser la propia clase para que el XAML vea las propiedades y el Comando
    this.BindingContext = this;

    // Inicializamos el comando de actualización apuntando al método REAL de tu archivo
    ActualizarProductosCommand = new Command(async () => await CargarProductosAsync());

    // Carga inicial cuando se monta el componente en el carrusel
    Loaded += async (s, e) => await CargarProductosAsync();

    // 🔄 ESCUCHA REENVÍO DESDE MAINPAGE (Detecta flecha de atrás, borrados y ediciones globales)
    MessagingCenter.Unsubscribe<object>(this, "ForzarRefrescoProductos");
    MessagingCenter.Subscribe<object>(this, "ForzarRefrescoProductos", async (sender) =>
    {
        Debug.WriteLine("📥 [ProductosPage] Orden de actualización recibida desde la MainPage madre. Recargando API...");
        await CargarProductosAsync();
    });

    // 🛍️ ESCUCHA DIRECTA DESDE VENDERPAGE (Por si acaso tu vista de publicar sigue usando este canal)
    MessagingCenter.Unsubscribe<VenderPage>(this, "REFRESH_PRODUCTOS");
    MessagingCenter.Subscribe<VenderPage>(this, "REFRESH_PRODUCTOS", async (s) => 
    {
        Debug.WriteLine("📥 [ProductosPage] Refresco directo solicitado desde VenderPage.");
        await CargarProductosAsync();
    });
}
    // 2. CARGA DE DATOS DESDE LA API
    public async Task CargarProductosAsync()
    {
        try
        {
            Debug.WriteLine("Refrescando lista de productos...");
            
            // Obtenemos la lista de la API
            var productosOriginales = await _apiService.ObtenerProductos();
            
            if (productosOriginales == null || !productosOriginales.Any()) 
            {
                PararRefresco();
                return;
            }

            // Invertimos la lista para mostrar novedades primero
            var productosInvertidos = productosOriginales.AsEnumerable().Reverse().ToList();

            MainThread.BeginInvokeOnMainThread(() => 
            {
                Novedades.Clear();
                MasPopulares.Clear();

                foreach (var p in productosInvertidos)
{
    // YA NO USAMOS MapearAItemPop. 
    // Ahora 'p' ya tiene la propiedad ImagenUrl lista y corregida
    // gracias a la lógica que pusimos en el modelo Producto.cs (o ItemPop.cs).
    
    ItemPop itemParaAñadir = p; // Directamente asignamos el producto

    // Añadimos a MasPopulares
    MasPopulares.Add(itemParaAñadir);

    // Si estamos dentro de los primeros 8, añadimos a Novedades
    if (Novedades.Count < 8)
    {
        Novedades.Add(itemParaAñadir);
    }
}
                
                Debug.WriteLine($"✅ Listas actualizadas: {Novedades.Count} novedades.");
                PararRefresco();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error en CargarProductos: {ex.Message}");
            PararRefresco();
        }
    }

    // Método auxiliar para detener el icono de carga del RefreshView
    private void PararRefresco()
    {
        MainThread.BeginInvokeOnMainThread(() => {
            // "RefreshControl" es el x:Name que pusimos en el XAML
            RefreshControl.IsRefreshing = false;
        });
    }

    

    // 4. NAVEGACIÓN AL DETALLE
    private async void OnProductoSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ItemPop item)
            return;

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        await Navigation.PushAsync(new DetalleProductoPage(item));
    }
}