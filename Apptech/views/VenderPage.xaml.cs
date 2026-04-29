        namespace Apptech.views;
        using Microsoft.Maui.Storage; 
        using Apptech.Models;
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
                var usuarioId = Preferences.Get("usuario_id", 0);
                var categoriaSeleccionada = (Categoria)CategoriaPicker.SelectedItem;

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

                if (usuarioId ==0)
                {
                await DisplayAlert("Error", "Debes tener una cuenta para poder vender productos", "OK");
                await Navigation.PushAsync(new LoginPage());
                return;
                }

                if (categoriaSeleccionada == null)
                {
                await DisplayAlert("Error", "Debes seleccionar una categoría para el producto", "OK");
                return;
                }
            
                    await _apiService.CrearProducto(nombre, descripcion, precio, stream, archivo.FileName, usuarioId, categoriaSeleccionada.Id);

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
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var categorias = await _apiService.ObtenerCategorias();
                CategoriaPicker.ItemsSource = categorias;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar las categorías: {ex.Message}", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (Handler != null)
            {
                MessagingCenter.Unsubscribe<VenderPage>(this, "REFRESH_PRODUCTOS");   ;
            }
        }
       
        
    }