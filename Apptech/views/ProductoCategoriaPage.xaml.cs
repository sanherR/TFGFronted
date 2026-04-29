using Apptech.Services;
using Apptech.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Apptech.views;

public partial class ProductoCategoriaPage: ContentView, INotifyPropertyChanged
{

	private readonly ApiService _apiService = new();
	public ObservableCollection<Producto> Sugerencias { get; set; } = new();

	public ObservableCollection<Categoria> ListaCategorias { get; set; } = new();

	public ObservableCollection<Producto> ProductosCategoria { get; set; } = new();

	private List<Producto> ListaProductos { get; set; } = new();
	public Command<Producto> ProductoSeleccionadoCommand { get; private set; }
	
	public bool MostrarSugerencias => Sugerencias.Any();


	private Categoria CategoriaActual { get; set; } = new Categoria { Id = 0, Nombre = "Todas" };


	 public string TextoBusqueda { get; set; } = string.Empty;

	public Command<Categoria> CategoriaSeleccionadaCommand { get; private set; }

	public ProductoCategoriaPage()
	{
        InitializeComponent();
		 
		CategoriaSeleccionadaCommand = new Command<Categoria>(FiltrarProductosPorCategoria);
		ProductoSeleccionadoCommand = new Command<Producto>(SeleccionarProducto);

		this.BindingContext = this;
	}

	protected override async void OnParentSet()
	{
		base.OnParentSet();
		if (Parent != null)
		{
			await CargarProductosCategorias();
		}
	}
	

	public async Task CargarProductosCategorias()
	{
		try
			{
				var listaCategorias =  await _apiService.ObtenerCategorias();
				var listaProductos = await _apiService.ObtenerProductos();

				foreach (var p in listaProductos)
{
				if (!string.IsNullOrWhiteSpace(p.ImagenUrl) &&
					!p.ImagenUrl.StartsWith("http"))
				{
					p.ImagenUrl = $"http://192.168.1.137:5062{p.ImagenUrl}";
				}
			}


				if(listaCategorias != null)
				{
					ListaCategorias.Clear();

				ListaCategorias.Add(new Categoria { Id = 0, Nombre = "Todas" });

				foreach (var cat in listaCategorias)
				{
						ListaCategorias.Add(cat);
				}
				ListaProductos = listaProductos;

				FiltrarProductosPorCategoria(new Categoria { Id = 0, Nombre = "Todas" }); 
				}
			}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar las categorías y productos: {ex.Message}", "OK");
			System.Diagnostics.Debug.WriteLine($"Error al cargar categorías y productos: {ex}");
			ListaCategorias.Clear();
			ProductosCategoria.Clear();
		
		

		
		}
	}

	private void FiltrarProductosPorCategoria(Categoria categoria)
	{

		if (categoria == null)
		{	
			return;
		}
		CategoriaActual = categoria;
		
		Aplicarfiltro();
		
	}

	private void Aplicarfiltro()
	{
	
		ProductosCategoria.Clear();

		IEnumerable<Producto> productos = ListaProductos;

		if(CategoriaActual.Id !=0)
		{
			productos = productos.Where(p => p.CategoriaId == CategoriaActual.Id);
		}	

		if(!string.IsNullOrWhiteSpace(TextoBusqueda))
		{
			productos = productos.Where(p => p.Nombre.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase));
		}	

		foreach(var p in productos)
		{
			ProductosCategoria.Add(p);
		}

	}

	private void Buscador(object sender, TextChangedEventArgs e)
	{
		TextoBusqueda = e.NewTextValue ?? string.Empty;

		if (string.IsNullOrWhiteSpace(TextoBusqueda))
		{
			Sugerencias.Clear();
			
			OnPropertyChanged(nameof(MostrarSugerencias));

			Aplicarfiltro();

			return;
		}
		var sugerencias = ListaProductos.Where(p => p.Nombre.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase)).Take(5).ToList();
		Sugerencias.Clear();
		foreach(var s in sugerencias){
			Sugerencias.Add(s);
		}

		OnPropertyChanged(nameof(MostrarSugerencias));

		Aplicarfiltro();
	}
	private async void SeleccionarProducto(Producto producto)
	{
		if (producto == null)
			return;

		TextoBusqueda = producto.Nombre;
		Sugerencias.Clear();
		OnPropertyChanged(nameof(MostrarSugerencias));
		Aplicarfiltro();
	}

	public event PropertyChangedEventHandler PropertyChanged;	

	void OnPropertyChanged(string nombre)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
	}	


}		
