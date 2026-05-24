using Apptech.Services;
using Apptech.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input; 

namespace Apptech.views;

public partial class ProductosPage : ContentView
{
    private readonly ApiService _apiService = new ApiService();

    
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

    
    public ICommand ActualizarProductosCommand { get; }

    public ProductosPage()
{
    InitializeComponent();
    
    
    this.BindingContext = this;

    
    ActualizarProductosCommand = new Command(async () => await CargarProductosAsync());

    
    Loaded += async (s, e) => await CargarProductosAsync();

    
    MessagingCenter.Unsubscribe<object>(this, "ForzarRefrescoProductos");
    MessagingCenter.Subscribe<object>(this, "ForzarRefrescoProductos", async (sender) =>
    {
        Debug.WriteLine("📥 [ProductosPage] Orden de actualización recibida desde la MainPage madre. Recargando API...");
        await CargarProductosAsync();
    });

    
    MessagingCenter.Unsubscribe<VenderPage>(this, "REFRESH_PRODUCTOS");
    MessagingCenter.Subscribe<VenderPage>(this, "REFRESH_PRODUCTOS", async (s) => 
    {
        Debug.WriteLine("📥 [ProductosPage] Refresco directo solicitado desde VenderPage.");
        await CargarProductosAsync();
    });
}
    
    public async Task CargarProductosAsync()
    {
        try
        {
            Debug.WriteLine("Refrescando lista de productos...");
            
           
            var productosOriginales = await _apiService.ObtenerProductos();
            
            if (productosOriginales == null || !productosOriginales.Any()) 
            {
                PararRefresco();
                return;
            }

            
            var productosInvertidos = productosOriginales.AsEnumerable().Reverse().ToList();

            MainThread.BeginInvokeOnMainThread(() => 
            {
                Novedades.Clear();
                MasPopulares.Clear();

                foreach (var p in productosInvertidos)
{
    
    
    ItemPop itemParaAñadir = p; 

    
    MasPopulares.Add(itemParaAñadir);

    
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

    
    private void PararRefresco()
    {
        MainThread.BeginInvokeOnMainThread(() => {
            
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