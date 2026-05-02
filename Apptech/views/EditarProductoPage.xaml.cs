using Apptech.Models;
using Apptech.Services;
using System.Windows.Input;

namespace Apptech.views;

public partial  class EditarProductoPage : ContentPage
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
            await DisplayAlert("OK", "Producto actualizado", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo actualizar", "OK");
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

            OnPropertyChanged(nameof(ImagenPreview));
        }
    }
}