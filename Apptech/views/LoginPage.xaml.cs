using System.Net.Http.Json; // ¡No olvides este using para que funcione el PostAsJsonAsync!

namespace Apptech.views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    // Este es el botón de "INICIAR SESIÓN"
    private async void OnRegisterTapped2(object sender, EventArgs e)
    {
        // 1. Verificamos que no haya campos vacíos
        if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            await DisplayAlert("Atención", "Por favor, introduce tu correo y contraseña", "OK");
            return;
        }

        // 2. Feedback visual (desactivamos el botón mientras carga)
        var boton = (Button)sender;
        boton.IsEnabled = false;
        boton.Text = "Verificando...";

        try
        {
            // 3. Creamos el objeto con los datos (Email y Password)
            var datosLogin = new 
            { 
                Email = txtEmail.Text, 
                Password = txtPassword.Text 
            };

            using var client = new HttpClient();
            // URL con la IP del emulador (10.0.2.2) y tu puerto (5062)
            string url = "http://10.0.2.2:5062/api/usuarios/login"; 

            // 4. Enviamos la petición al Backend
            var respuesta = await client.PostAsJsonAsync(url, datosLogin);

            if (respuesta.IsSuccessStatusCode)
            {
                // ¡ÉXITO! El backend dice que el usuario es válido
                await DisplayAlert("¡Bienvenido!", "Has iniciado sesión correctamente", "OK");
                
                // Ahora sí, navegamos a la página principal
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                // El backend devolvió Unauthorized (401)
                await DisplayAlert("Error", "Correo o contraseña incorrectos", "OK");
            }
        }
        catch (Exception ex)
        {
            // Error de conexión (Backend apagado, falta de internet, etc.)
            await DisplayAlert("Error de conexión", "No se pudo conectar con el servidor", "OK");
        }
        finally
        {
            // Reestablecemos el botón pase lo que pase
            boton.IsEnabled = true;
            boton.Text = "INICIAR SESIÓN";
        }
    }

    // Este es el enlace de "¿No tienes cuenta? Regístrate"
    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}