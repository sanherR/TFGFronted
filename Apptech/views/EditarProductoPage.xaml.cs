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

    // Comandos para los botones del XAML
    public ICommand GuardarCommand { get; }
    public ICommand CambiarImagenCommand { get; }

    // Propiedades vinculadas al XAML (Bindings)
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int categoriaId { get; set; }
    public string Estado_producto { get; set; } // Lo usamos para el Binding del Picker
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
        categoriaId = producto.categoria_id;
        
        // Mapeamos desde tu modelo Producto.cs
        Estado_producto = producto.Estado ?? "Nuevo"; 
        Caracteristicas = producto.Caracteristicas ?? "";
        
        // Imagen actual
        ImagenPreview = producto.ImagenUrl;

        // Inicializamos los comandos
        GuardarCommand = new Command(async () => await Guardar());
        CambiarImagenCommand = new Command(async () => await SeleccionarImagen());

        // Establecemos el contexto de datos para que el XAML funcione
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
            // 1. Cargamos las categorías del servidor para el Picker
            var listaCategorias = await _apiService.ObtenerCategorias();
            if (listaCategorias != null && CategoriaPicker != null) 
            {
                CategoriaPicker.ItemsSource = listaCategorias;
                
                // Seleccionamos automáticamente la categoría actual del producto
                var seleccionada = listaCategorias.FirstOrDefault(c => c.Id == categoriaId);
                if (seleccionada != null)
                {
                    CategoriaPicker.SelectedItem = seleccionada;
                }
            }

            // 2. Seleccionamos el estado actual en el Picker de estados
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
        // 1. EXTRAEMOS EL ID REAL (Prueba con id_producto que es como viene del JSON del Back)
        int idFinalProducto = _producto.Id; 

        // Si tu objeto en la App usa la propiedad "Id" con mayúscula pero venía vacía,
        // asegúrate de verificar cómo se llama en tu modelo 'Producto' del móvil.
        // Si tu modelo en la app usa 'id_producto', mándale '_producto.id_producto'.

        int precioFinal = (int)Math.Round(Precio);

        var catSeleccionada = (Categoria)CategoriaPicker.SelectedItem;
        int idFinalCat = catSeleccionada?.Id ?? categoriaId;
        string estadoFinal = EstadoPicker.SelectedItem?.ToString() ?? Estado_producto;

        // 3. Llamada a la API pasándole el ID que toca
        var success = await _apiService.ActualizarProducto(
    _producto.Id, // Manda la propiedad del ID directo
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
            // Modificamos el alert para que en la pantalla te diga qué ID está enviando el móvil
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
                
                // Cambiamos la vista previa en la pantalla
                ImagenPreview = ImageSource.FromStream(() => _imagenStream);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo cargar la imagen: " + ex.Message, "OK");
        }
    }

    // Lógica para notificar cambios a la interfaz (UI)
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}