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
    
    // Colecciones para la interfaz
    public ObservableCollection<ItemPop> Sugerencias { get; set; } = new();
    public ObservableCollection<Categoria> ListaCategorias { get; set; } = new();
    public ObservableCollection<ItemPop> ProductosCategoria { get; set; } = new();

    private List<ItemPop> ListaProductos { get; set; } = new();
    
    // Comandos
    public Command<ItemPop> ProductoSeleccionadoCommand { get; private set; }
    public Command<Categoria> CategoriaSeleccionadaCommand { get; private set; }

    public bool MostrarSugerencias => Sugerencias.Count > 0;

    // Propiedad para la categoría seleccionada
    private Categoria _categoriaActual = new Categoria { Id = 0, Nombre = "Todas" };
    public Categoria CategoriaActual 
    { 
        get => _categoriaActual; 
        set { _categoriaActual = value; OnPropertyChanged(); } 
    }

    // Propiedad para el texto del buscador
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
            // Mapeo manual forzando la variable con guion bajo
            ListaProductos = productosDb.Select(p => 
            {
                var nuevoItem = new ItemPop
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    // ASIGNACIÓN CRÍTICA:
                    categoria_id = p.categoria_id, 
                    ImagenUrl = p.ImagenUrl.StartsWith("http") ? p.ImagenUrl : $"http://10.0.2.2:5062{p.ImagenUrl}",
                    Estado = p.Estado,
                    Caracteristicas = p.Caracteristicas
                };

                // Esto imprimirá en consola el nombre y el ID de cada producto según llega
                Debug.WriteLine($"[DATOS API] Producto: {nuevoItem.Nombre} | ID Categoria: {nuevoItem.categoria_id}");
                
                return nuevoItem;
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
            
            // Forzamos el filtro inicial
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
        // Esto te dirá en la consola de Visual Studio qué está viendo la App realmente
Debug.WriteLine($"BUSCANDO: {CategoriaActual.Id} | DISPONIBLE EN APP: {string.Join(",", ListaProductos.Select(x => x.categoria_id))}");
        ProductosCategoria.Clear();
        
        // 1. Empezamos con la lista completa
        var filtrados = ListaProductos.AsEnumerable();

        // 2. Filtro por Categoría (Fíjate en la C MAYÚSCULA)
        if (CategoriaActual != null && CategoriaActual.Id != 0)
        {
            // Usamos CategoriaId con C mayúscula porque así está en tu ItemPop.cs
            filtrados = filtrados.Where(p => p.categoria_id == CategoriaActual.Id);
        }

        // 3. Filtro por Texto del buscador
        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            filtrados = filtrados.Where(p => p.Nombre.ToLower().Contains(TextoBusqueda.ToLower()));
        }

        // 4. Cargamos los resultados finales
        foreach (var p in filtrados.ToList())
        {
            ProductosCategoria.Add(p);
        }

        // Ahora el Debug funcionará porque añadimos el "using" arriba
        Debug.WriteLine($"✅ Filtro aplicado. Categoria: {CategoriaActual?.Nombre}, Resultados: {ProductosCategoria.Count}");
    });
}

    private void Buscador(object sender, TextChangedEventArgs e)
    {
        TextoBusqueda = e.NewTextValue ?? string.Empty;

        if (string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            Sugerencias.Clear();
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

        AplicarFiltro();
    }

    private void SeleccionarProducto(ItemPop producto)
    {
        if (producto == null) return;
        TextoBusqueda = producto.Nombre;
        Sugerencias.Clear();
        AplicarFiltro();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}