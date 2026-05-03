using Apptech.Models;
using Apptech.Services;
using System.Diagnostics;

namespace Apptech.views;

public partial class VenderPage : ContentPage
{
    private ApiService _apiService = new ApiService();
    private Stream _imagenStream;
    private string _nombreArchivo;

    public VenderPage()
    {
        InitializeComponent();
    }

    private async void BtnSeleccionarImagen_Clicked(object sender, EventArgs e)
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

        // ESTA ES LA LÍNEA QUE AÑADE EL MENSAJE
        await DisplayAlert("Imagen", "Imagen seleccionada correctamente: " + _nombreArchivo, "OK");
    }
}

    private async void BtnAgregarProducto_Clicked(object sender, EventArgs e)
    {
        // Validación básica
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) || string.IsNullOrWhiteSpace(PrecioEntry.Text))
        {
            await DisplayAlert("Error", "El nombre y el precio son obligatorios", "OK");
            return;
        }

        // Datos del usuario (Asegúrate de tener el ID guardado)
        int usuarioId = Preferences.Get("userId", 0);
        int categoriaId = 1; // Valor por defecto o del selector si lo mantuviste
        decimal precio = decimal.Parse(PrecioEntry.Text);

        // LLAMADA CORREGIDA: Solo los 7 parámetros que definimos en ApiService
        var exito = await _apiService.CrearProducto(
            NombreEntry.Text,
            DescripcionEntry.Text,
            precio,
            _imagenStream,
            _nombreArchivo,
            usuarioId,
            categoriaId
        );

       if (exito)
{
    await DisplayAlert("Éxito", "Producto publicado", "OK");
    
    // 🔥 ESTA LÍNEA ES LA CLAVE: Envía una señal de "recarga"
    MessagingCenter.Send<App>((App)Application.Current, "ActualizarPerfil");

    await Navigation.PopAsync();
}
        else
        {
            await DisplayAlert("Error", "No se pudo publicar el producto", "OK");
        }
    }
}