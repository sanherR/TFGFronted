using Apptech.Services;
using Apptech.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics; 

namespace Apptech.views;

public partial class ProductoCategoriaPage : ContentPage, INotifyPropertyChanged
{
    private readonly ApiService _apiService = new();
    
    public ObservableCollection<ItemPop> Sugerencias { get; set; } = new();
    public ObservableCollection<Categoria> ListaCategorias { get; set; } = new();
    public ObservableCollection<ItemPop> ProductosCategoria { get; set; } = new();

    private List<ItemPop> ListaProductos { get; set; } = new();
    
    public Command<ItemPop> ProductoSeleccionadoCommand { get; private set; }
    public Command<Categoria> CategoriaSeleccionadaCommand { get; private set; }

    public bool MostrarSugerencias => Sugerencias.Count > 0;

    private Categoria _categoriaActual = new Categoria { Id = 0, Nombre = "Todas" };
    public Categoria CategoriaActual 
    { 
        get => _categoriaActual; 
        set { _categoriaActual = value; OnPropertyChanged(); } 
    }

    private string _textoBusqueda = string.Empty;
    public string TextoBusqueda 
    { 
        get => _textoBusqueda; 
        set { _textoBusqueda = value; OnPropertyChanged(); } 
    }

    public ProductoCategoriaPage()
    {
        InitializeComponent();
        
        CategoriaSeleccionadaCommand = new Command<Categoria>(FiltrarProductosPorCategoria);
        ProductoSeleccionadoCommand = new Command<ItemPop>(SeleccionarProducto);

        this.BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarProductosCategorias();
    }

    public async Task CargarProductosCategorias()
    {
        try
        {
            var listaCategorias = await _apiService.ObtenerCategorias();
            var productosDb = await _apiService.ObtenerProductos(); 

            if (productosDb != null)
            {
                ListaProductos = productosDb.Select(p => new ItemPop
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = (decimal)p.Precio, 
                    categoria_id = p.categoria_id,
                    UsuarioId = p.UsuarioId,
                    Estado = p.Estado,
                    Caracteristicas = p.Caracteristicas,
                    ImagenUrlBase = p.ImagenUrl.StartsWith("http") ? p.ImagenUrl : $"https://tfgbacken-production.up.railway.app{p.ImagenUrl}",
                    
                    
                    Vendido = p.Vendido 
                }).ToList();
            }

            if (listaCategorias != null)
            {
                ListaCategorias.Clear();
                ListaCategorias.Add(new Categoria { Id = 0, Nombre = "Todas" });
                foreach (var cat in listaCategorias) 
                {
                    ListaCategorias.Add(cat);
                }
                FiltrarProductosPorCategoria(ListaCategorias[0]); 
            }
        }
        catch (Exception ex) 
        { 
            Debug.WriteLine($"Error crítico en Carga: {ex.Message}"); 
        }
    }

    private void FiltrarProductosPorCategoria(Categoria categoria)
    {
        if (categoria == null) return;
        CategoriaActual = categoria;
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        MainThread.BeginInvokeOnMainThread(() => {
            ProductosCategoria.Clear();
            var filtrados = ListaProductos.AsEnumerable();

            if (CategoriaActual != null && CategoriaActual.Id != 0)
            {
                filtrados = filtrados.Where(p => p.categoria_id == CategoriaActual.Id);
            }

            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                filtrados = filtrados.Where(p => p.Nombre.ToLower().Contains(TextoBusqueda.ToLower()));
            }

            foreach (var p in filtrados)
            {
                ProductosCategoria.Add(p);
            }
            OnPropertyChanged(nameof(MostrarSugerencias));
        });
    }

    private void Buscador(object sender, TextChangedEventArgs e)
    {
        TextoBusqueda = e.NewTextValue ?? string.Empty;

        if (string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            Sugerencias.Clear();
            OnPropertyChanged(nameof(MostrarSugerencias));
            AplicarFiltro();
            return;
        }

        var sugerenciasList = ListaProductos
            .Where(p => p.Nombre.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .ToList();

        Sugerencias.Clear();
        foreach (var s in sugerenciasList)
        {
            Sugerencias.Add(s);
        }

        OnPropertyChanged(nameof(MostrarSugerencias));
        AplicarFiltro();
    }

    private async void SeleccionarProducto(ItemPop item)
    {
        if (item == null) return;

        try
        {
            Sugerencias.Clear();
            OnPropertyChanged(nameof(MostrarSugerencias));

            
            await Navigation.PushAsync(new DetalleProductoPage(item));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al navegar: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}