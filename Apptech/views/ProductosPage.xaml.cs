namespace Apptech.views;
using System.Collections.ObjectModel;
using Apptech.Services;
using Apptech.Models;

public partial class ProductosPage: ContentView
{
    private readonly ApiService _apiService = new ApiService();

	

	public ProductosPage()
	{
        InitializeComponent();  
        BindingContext = this; 
        Loaded += async (s, e) => await CargarProductosAsync();

        


    }
     public ObservableCollection<ItemPop> Recomendados { get; set; } = new();
    public ObservableCollection<ItemPop> MasPopulares { get; set; } = new();
    public ObservableCollection<ItemPop> Novedades { get; set; } = new();

   
   public async Task CargarProductosAsync()
    {


        if (Recomendados.Count == 0)
        {
            try{
                    var productos = await _apiService.ObtenerProductos();
                    Recomendados.Clear();
                    MasPopulares.Clear();
                    Novedades.Clear();

            
            foreach (var p in productos)
            {
                var item = new ItemPop
                {
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    precio = (int)p.Precio,
                    ImagenUrl = "http://192.168.1.137:5062" + p.ImagenUrl
                };

                switch (p.Grupo)
                {
                    case "Recomendados": Recomendados.Add(item); break;
                    case "Más Populares": MasPopulares.Add(item); break;
                    case "Novedades": Novedades.Add(item); break;
                }
            }

                Lista1.ItemsSource = Recomendados;
                Lista2.ItemsSource = MasPopulares;
                Lista3.ItemsSource = Novedades;
            }

            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los productos: {ex.Message}", "OK");
            }
        
    }
        }
        
    
    
    protected override async void OnParentSet()
    {
        base.OnParentSet();
        await CargarProductosAsync(); 
    }
    
    
   
    
  
}


