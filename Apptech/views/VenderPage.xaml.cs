    namespace  Apptech.views;
    using Microsoft.Maui.Storage; 
    using Apptech.Messages;
    using Apptech.Services;
    using Xamarin.Google.Crypto.Tink.Shaded.Protobuf;

    public partial class VenderPage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();

        public VenderPage()
        {
            InitializeComponent();
        }

        private async void BtnAgregarProducto_Clicked(object sender, EventArgs e)
        {

           
        try
        {
             
            String nombre = NombreEntry.Text;
            String descripcion = DescripcionEntry.Text;
            String imagenURL = string.Empty;
            int precio = int.Parse(PrecioEntry.Text);

              if(precio < 0)
            {
                await DisplayAlert("Error", "Debes ingresar un precio igual o mayor a cero", "OK");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(nombre))
            {
            await DisplayAlert("Error", "Debes ingresar nombre e imagen del producto", "OK");
            return;
            }
            if (string.IsNullOrWhiteSpace(descripcion) )
            {
                await DisplayAlert("Error", "Debes ingresar la descripción del producto", "OK");
                return;
            }
            var archivo = await MediaPicker.Default.PickPhotoAsync();
           

            if (archivo == null)
            {
            await DisplayAlert("Error", "No se seleccionó ninguna imagen", "OK");
                return;
            }

           
                await _apiService.CrearProducto(nombre, descripcion, precio.ToString(), archivo);
        }
        catch (FormatException)
        {
            await DisplayAlert("Error", "Debes ingresar un precio válido", "OK");
            return;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
            return;

           


            
           


        }
        }   
    }