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

    // Comandos
    public ICommand GuardarCommand { get; }
    public ICommand CambiarImagenCommand { get; }

    // Propiedades vinculadas al XAML
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

    Nombre = producto.Nombre;
    Descripcion = producto.Descripcion;
    Precio = producto.Precio;
    
    // Si da rojo, prueba a poner producto.categoria_id (en minúsculas)
    categoriaId = producto.categoria_id; 
    
    // Si estos dan rojo, es que NO existen en tu clase Producto.cs todavía
    Estado_producto = producto.Estado ?? "Nuevo"; 
    Caracteristicas = producto.Caracteristicas ?? "";
    
    ImagenPreview = producto.ImagenUrl;

    GuardarCommand = new Command(async () => await Guardar());
    CambiarImagenCommand = new Command(async () => await SeleccionarImagen());

    BindingContext = this;
}

    // Este método se ejecuta al entrar para rellenar los Pickers
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
            
            // Esto selecciona la categoría que ya tenía el producto
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
        System.Diagnostics.Debug.WriteLine("Error cargando pickers: " + ex.Message);
    }
}

    private async Task Guardar()
    {
        // Recogemos la categoría seleccionada del Picker
        var catSeleccionada = (Categoria)CategoriaPicker.SelectedItem;
        int idFinalCat = catSeleccionada?.Id ?? categoriaId;

        // Recogemos el estado seleccionado del Picker
        string estadoFinal = EstadoPicker.SelectedItem?.ToString() ?? Estado_producto;

        var success = await _apiService.ActualizarProducto(
            _producto.Id,
            Nombre,
            Descripcion,
            Precio,
            idFinalCat,
            _imagenStream,
            _nombreArchivo,
            estadoFinal,
            Caracteristicas);

        if (success)
        {
            await DisplayAlert("Éxito", "Producto actualizado correctamente", "OK");
            MessagingCenter.Send<App>((App)Application.Current, "ActualizarPerfil");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo actualizar el producto. Revisa los datos.", "OK");
        }
    }

    private async Task SeleccionarImagen()
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona una imagen",
            FileTypes = FilePickerFileType.Images
        });

        if (result != null)
        {
            _imagenStream = await result.OpenReadAsync();
            _nombreArchivo = result.FileName;
            
            ImagenPreview = ImageSource.FromStream(() => _imagenStream);
            await DisplayAlert("Imagen", "Imagen seleccionada correctamente", "OK");
        }
    }

    // Para que la imagen se refresque en la pantalla
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}