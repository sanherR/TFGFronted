namespace Apptech.views;
using System.Collections.ObjectModel;
using Apptech.Services;
public partial class ProductosPage: ContentView
{
    private readonly ApiService _apiService = new ApiService();

	public class ItemPop
    {
        public string Nombre { get; set; }

        public string descripcion { get; set; }

        public int precio { get; set; }
        public string ImagenUrl { get; set; }
    }   
    public ObservableCollection<ItemPop> MisProductos { get; set; }

	public ProductosPage()
	{
        InitializeComponent();

        MisProductos = new ObservableCollection<ItemPop>();
        BindingContext = this;
         _ = CargarProductos(); 



    }
    
    public async Task CargarProductos()
    {
        try
        {
            var productos = await _apiService.ObtenerProductos();

            MisProductos.Clear();

            foreach (var producto in productos)
                {
                    MisProductos.Add(new ItemPop
                    {
                        Nombre = producto.Nombre,
                        descripcion = producto.Descripcion,
                        precio = producto.Precio,
                        ImagenUrl = producto.ImagenUrl
                    });
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los productos: {ex.Message}", "OK");
            }
        }
}

