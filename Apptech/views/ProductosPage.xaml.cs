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
        this.BindingContext = this; 
         Loaded += async (s, e) =>
    {
        await CargarProductosAsync();
        System.Diagnostics.Debug.WriteLine($"UI READY → BindingContext OK: {BindingContext != null}");

    };

        


    }
     public ObservableCollection<ItemPop> Recomendados { get; set; } = new();
    public ObservableCollection<ItemPop> MasPopulares { get; set; } = new();
    public ObservableCollection<ItemPop> Novedades { get; set; } = new();

   
   public async Task CargarProductosAsync()
    {



            try{
                    var productos = await _apiService.ObtenerProductos();
                    Recomendados.Clear();
                    MasPopulares.Clear();
                    Novedades.Clear();

            
            foreach (var p in productos)
            {
                    System.Diagnostics.Debug.WriteLine($"GRUPO: '{p.Grupo}' LEN={p.Grupo?.Length}");

                var item = new ItemPop
                {
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    precio = (int)p.Precio,
                    ImagenUrl = "http://192.168.1.137:5062" + p.ImagenUrl
                };
                System.Diagnostics.Debug.WriteLine(
                System.Text.Json.JsonSerializer.Serialize(p)
);
                System.Diagnostics.Debug.WriteLine("UI TEST → entrando a foreach");

                switch (p.Grupo)
                {
                    case "Recomendados": 
                    
                        System.Diagnostics.Debug.WriteLine("ADD ITEM A RECOMENDADOS");
                        Recomendados.Add(item); break;
                    case "Más Populares": 
                        System.Diagnostics.Debug.WriteLine("ADD ITEM A MÁS POPULARES");
                        MasPopulares.Add(item); break;
                    case "Novedades": 
                        System.Diagnostics.Debug.WriteLine("ADD ITEM A NOVEDADES");
                        Novedades.Add(item); break;
                }
            }
            System.Diagnostics.Debug.WriteLine($"Recomendados: {Recomendados.Count}");
            System.Diagnostics.Debug.WriteLine($"MasPopulares: {MasPopulares.Count}");
            System.Diagnostics.Debug.WriteLine($"Novedades: {Novedades.Count}");
            System.Diagnostics.Debug.WriteLine($"TOTAL API: {productos.Count}");
          

              
            }

            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los productos: {ex.Message}", "OK");
            }
        
    }
}
       
       
   
        
    
    
  
    
   
    
  



