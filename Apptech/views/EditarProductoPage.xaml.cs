using Apptech.Models;
using Apptech.Services;
using System.Windows.Input;

namespace Apptech.views;

public partial class EditarProductoPage : ContentPage
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
    public int CategoriaId { get; set; }

    public ImageSource ImagenPreview { get; set; }

    public EditarProductoPage(Producto producto)
    {
        InitializeComponent();
        _producto = producto;

        Nombre = producto.Nombre;
        Descripcion = producto.Descripcion;
        Precio = producto.Precio;
        CategoriaId = producto.CategoriaId;
        ImagenPreview = producto.ImagenUrl;

        GuardarCommand = new Command(async () => await Guardar());
        CambiarImagenCommand = new Command(async () => await SeleccionarImagen());

        BindingContext = this;
    }

    private async Task Guardar()
{
    var success = await _apiService.ActualizarProducto(
        _producto.Id,
        Nombre,
        Descripcion,
        Precio,
        CategoriaId,
        _imagenStream,
        _nombreArchivo);

    if (success)
    {
        await DisplayAlert("Éxito", "Producto actualizado correctamente", "OK");
        
        // 🔥 ESTO HACE QUE EL CAMBIO SE VEA EN EL PERFIL AL VOLVER
        MessagingCenter.Send<App>((App)Application.Current, "ActualizarPerfil");
        
        await Navigation.PopAsync();
    }
    else
    {
        await DisplayAlert("Error", "El servidor rechazó la actualización. Revisa los datos.", "OK");
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
            // Solo si el usuario elige una foto, llenamos el stream
            _imagenStream = await result.OpenReadAsync();
            _nombreArchivo = result.FileName;
            
            // Actualizamos la vista previa en la pantalla
            ImagenPreview = ImageSource.FromStream(() => _imagenStream);
            OnPropertyChanged(nameof(ImagenPreview));
            
            await DisplayAlert("Imagen", "Imagen seleccionada correctamente", "OK");
        }
    }
}