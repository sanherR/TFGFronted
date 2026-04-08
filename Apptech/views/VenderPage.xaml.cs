        namespace  Apptech.views;
        using Microsoft.Maui.Storage; 
        using Apptech.Services;

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
                    using var stream = await archivo.OpenReadAsync();


            
                    await _apiService.CrearProducto(nombre, descripcion, precio, stream, archivo.FileName);

                    await DisplayAlert("Éxito", "Producto creado correctamente", "OK");


                    await Navigation.PopAsync(); 
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
        private async void prueba(object sender, EventArgs e)
            {
            try
            {
                var productos = await _apiService.ObtenerProductos();
                    await DisplayAlert("Conexión OK", $"Productos obtenidos: {productos.Count}", "OK"); 
            }
            catch (Exception ex){
        
                await DisplayAlert("Error", $"No se pudo conectar con el servidor: {ex.Message}", "OK");
                return;
            }
        }
        
    }