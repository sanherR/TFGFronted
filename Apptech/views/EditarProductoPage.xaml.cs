using System.Windows.Input;
using Apptech.Models;
using Apptech.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Apptech.views;

public partial class EditarProductoPage : ContentPage, INotifyPropertyChanged
{
    private Producto _producto;
    private ApiService _apiService = new ApiService();
    private Stream _imagenStream;
    private string _nombreArchivo;

    
    public ICommand GuardarCommand { get; }
    public ICommand CambiarImagenCommand { get; }

    
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int categoriaId { get; set; }
    public string Estado_producto { get; set; } 
    public string Caracteristicas { get; set; }

    private ImageSource _imagenPreview;
    public ImageSource ImagenPreview
    {
        get => _imagenPreview;
        set { _imagenPreview = value; OnPropertyChanged(); }
    }

    public EditarProductoPage(Producto producto)
    {
        InitializeComponent();
        _producto = producto;

        // Cargamos los datos del objeto producto en las propiedades de la página
        Nombre = producto.Nombre;
        Descripcion = producto.Descripcion;
        Precio = producto.Precio;
        categoriaId = producto.CategoriaId;
        
        // Mapeamos
        Estado_producto = producto.Estado ?? "Nuevo"; 
        Caracteristicas = producto.Caracteristicas ?? "";
        
        // Imagen actual
        ImagenPreview = producto.ImagenUrl;

        // Inicializamos los comandos
        GuardarCommand = new Command(async () => await Guardar());
        CambiarImagenCommand = new Command(async () => await SeleccionarImagen());

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDatosIniciales();
    }

    private async Task CargarDatosIniciales()
    {
        try 
        {
            
            var listaCategorias = await _apiService.ObtenerCategorias();
            if (listaCategorias != null && CategoriaPicker != null) 
            {
                CategoriaPicker.ItemsSource = listaCategorias;
                
                
                var seleccionada = listaCategorias.FirstOrDefault(c => c.Id == categoriaId);
                if (seleccionada != null)
                {
                    CategoriaPicker.SelectedItem = seleccionada;
                }
            }

            
            if (EstadoPicker != null && !string.IsNullOrEmpty(Estado_producto))
            {
                EstadoPicker.SelectedItem = Estado_producto;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error cargando datos: " + ex.Message);
        }
    }

   private async Task Guardar()
{
    try 
    {
        // 1. EXTRAEMOS EL ID REAL 
        int idFinalProducto = _producto.Id; 

        

        int precioFinal = (int)Math.Round(Precio);

        var catSeleccionada = (Categoria)CategoriaPicker.SelectedItem;
        int idFinalCat = catSeleccionada?.Id ?? categoriaId;
        string estadoFinal = EstadoPicker.SelectedItem?.ToString() ?? Estado_producto;

        
        var success = await _apiService.ActualizarProducto(
    _producto.Id, 
    Nombre,
    Descripcion,
    precioFinal,
    idFinalCat,
    _imagenStream,
    _nombreArchivo,
    estadoFinal,
    Caracteristicas);

        if (success)
        {
            await DisplayAlert("Éxito", "Producto actualizado", "OK");
            MessagingCenter.Send<App>((App)Application.Current, "ActualizarPerfil");
            await Navigation.PopAsync();
        }
        else
        {
            
            await DisplayAlert("Error", $"Servidor: Producto no encontrado. El móvil envió el ID: {idFinalProducto}", "OK");
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", "Fallo al guardar: " + ex.Message, "OK");
    }
}

    private async Task SeleccionarImagen()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona una nueva imagen",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                _imagenStream = await result.OpenReadAsync();
                _nombreArchivo = result.FileName;
                
                
                ImagenPreview = ImageSource.FromStream(() => _imagenStream);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo cargar la imagen: " + ex.Message, "OK");
        }
    }

    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}