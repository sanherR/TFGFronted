using System.Net.Http.Json;
using Apptech.Services;
using Apptech.Models;
using System.Diagnostics;

namespace Apptech.views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public LoginPage()
    {
        InitializeComponent();
        
    }

    private async void OnRegisterTapped2(object sender, EventArgs e)
    {
        var boton = (Button)sender;

        // Desactivamos el botón para evitar múltiples clics
        boton.IsEnabled = false;
        boton.Text = "Verificando...";

        try 
        {
            // Intentamos el login
            var user = await _apiService.Login(txtEmail.Text, txtPassword.Text);

            if (user != null)
            {
                Debug.WriteLine($"✅ LOGIN EXITOSO: {user.Nombre} (ID: {user.UsuarioId})");

                // GUARDADO DE DATOS (Ajustado para que ApiService.cs los encuentre)
                Preferences.Set("token", user.Token);
                Preferences.Set("userId", user.UsuarioId); // 'userId' coincide con ApiService.cs
                Preferences.Set("user_name", user.Nombre);

                // Navegamos a la MainPage
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                await DisplayAlert("Error", "Credenciales incorrectas o servidor no disponible.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR CRÍTICO EN LOGIN: {ex.Message}");
            await DisplayAlert("Error de Conexión", "No se pudo conectar con la API. Revisa que el backend esté encendido.", "OK");
        }
        finally 
        {
            // Siempre restauramos el botón, pase lo que pase
            boton.IsEnabled = true;
            boton.Text = "INICIAR SESIÓN";
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}