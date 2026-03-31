namespace Apptech.views;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
public partial class ProductosPage: ContentView
{
	public class ItemPop
    {
        public string Nombre { get; set; }
        public string ImagenUrl { get; set; }
    }
    public ObservableCollection<ItemPop> MisProductos { get; set; }

	public ProductosPage()
	{
        InitializeComponent();

        MessagingCenter.Subscribe<VenderPage, ItemPop>(this, "NuevoProducto", (sender, producto) =>
{
        MisProductos.Add(producto);
        });
       
        Lista1.ItemsSource = MisProductos;
        Lista2.ItemsSource = MisProductos;
		Lista3.ItemsSource = MisProductos;

       
    }
}
