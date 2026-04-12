using System.Net.Http.Json;
using Apptech.models; 

namespace Apptech.views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // --- 1. FEEDBACK VISUAL: Bloqueamos el botón para evitar duplicados ---
        var boton = (Button)sender;
        boton.IsEnabled = false;
        boton.Text = "REGISTRANDO...";

        try
        {
            // 2. Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || 
                string.IsNullOrWhiteSpace(txtEmail.Text) || 
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                await DisplayAlert("Atención", "Por favor, rellena todos los campos.", "OK");
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
                return;
            }

            // 3. Crear objeto
            var nuevoUsuario = new Usuario
            {
                Nombre = txtUsuario.Text,
                Email = txtEmail.Text,
                nombre_usuario = txtUsuario.Text,
                Contraseña = txtPassword.Text,
                direccion = "No especificada",
                telefono = "000000000"
            };

            // 4. Envío al servidor
            using var client = new HttpClient();
            string url = "http://10.0.2.2:5062/api/usuarios";

            var respuesta = await client.PostAsJsonAsync(url, nuevoUsuario);

            if (respuesta.IsSuccessStatusCode)
            {
                await DisplayAlert("¡Éxito!", "Cuenta creada correctamente.", "OK");
                await Navigation.PopAsync(); 
            }
            else
            {
                await DisplayAlert("Error", "No se pudo crear la cuenta. Revisa los datos.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error de conexión", "Servidor no alcanzado. Revisa el puerto: " + ex.Message, "OK");
        }
        finally
        {
            // --- 5. SIEMPRE devolvemos el botón a su estado normal ---
            boton.IsEnabled = true;
            boton.Text = "CREAR CUENTA";
        }
    }
}