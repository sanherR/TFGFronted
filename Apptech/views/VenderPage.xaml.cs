using Apptech.Models;
using Apptech.Services;
using System.Diagnostics;

namespace Apptech.views;

public partial class VenderPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    private Stream _imagenStream;
    private string _nombreArchivo;
    private List<Categoria> _categorias;

    public VenderPage()
    {
        InitializeComponent();
        
    }

    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarCategorias();
    }

    private async Task CargarCategorias()
    {
        try 
        {
            Debug.WriteLine("Tentando cargar categorías desde la API...");
            _categorias = await _apiService.ObtenerCategorias(); 
            
            if (_categorias != null && _categorias.Count > 0)
            {
                CategoriaPicker.ItemsSource = _categorias;
                Debug.WriteLine($"Categorías cargadas: {_categorias.Count}");
            }
            else 
            {
                Debug.WriteLine("La lista de categorías volvió vacía.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cargar categorías: {ex.Message}");
        }
    }

    private async void BtnSeleccionarImagen_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona una imagen para el producto",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                _imagenStream = await result.OpenReadAsync();
                _nombreArchivo = result.FileName;
                await DisplayAlert("Imagen", "Imagen cargada correctamente", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "No se pudo cargar la imagen: " + ex.Message, "OK");
        }
    }

    private async void BtnAgregarProducto_Clicked(object sender, EventArgs e)
    {
        
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) || string.IsNullOrWhiteSpace(PrecioEntry.Text))
        {
            await DisplayAlert("Error", "Nombre y precio son obligatorios", "OK");
            return;
        }

        if (CategoriaPicker.SelectedItem == null || EstadoPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Selecciona una categoría y el estado del producto", "OK");
            return;
        }

        try 
        {
           
            int usuarioId = Preferences.Get("userId", 0);
            
           
            var categoriaSeleccionada = (Categoria)CategoriaPicker.SelectedItem;
            int categoriaId = categoriaSeleccionada.Id; 
            
            string estado = EstadoPicker.SelectedItem.ToString();
            string caracteristicas = CaracteristicasEditor.Text ?? "Sin especificar";
            
            
            if (!decimal.TryParse(PrecioEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal precio))
            {
                await DisplayAlert("Error", "El precio no tiene un formato válido", "OK");
                return;
            }

            // 3. Llamada al ApiService
            var exito = await _apiService.CrearProducto(
                NombreEntry.Text,
                DescripcionEntry.Text,
                precio,
                _imagenStream,
                _nombreArchivo,
                usuarioId,
                categoriaId,
                estado,
                caracteristicas
            );

            if (exito)
            {
                await DisplayAlert("Éxito", "¡Producto publicado!", "OK");
                
                
                MessagingCenter.Send<App>((App)Application.Current, "ActualizarPerfil");
                
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "El servidor no pudo guardar el producto.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Fallo crítico: " + ex.Message, "OK");
        }
    }
}