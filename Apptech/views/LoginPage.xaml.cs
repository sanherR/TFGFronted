using System.Net.Http.Json; // ¡No olvides este using para que funcione el PostAsJsonAsync!
using Apptech.Services; // Para usar ApiService
namespace Apptech.views;
using Apptech.Models; 

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

    boton.IsEnabled = false;
    boton.Text = "Verificando...";
    
    var user = await _apiService.Login(txtEmail.Text, txtPassword.Text);

    if (user != null)
    {
        Console.WriteLine($"USER ID: {user?.UsuarioId}");
        Console.WriteLine("TOKEN DEVUELTO LOGIN: " + user.Token);

        Preferences.Set("token", user.Token);
        Preferences.Set("usuario_id", user.UsuarioId);
        Preferences.Set("user_name", user.Nombre);

        Console.WriteLine("USUARIO ID GUARDADO: " + Preferences.Get("usuario_id", 0));
        Console.WriteLine("TOKEN GUARDADO: " + Preferences.Get("token", ""));
        

        await Navigation.PushAsync(new MainPage());
    }
    else
    {
        await DisplayAlert("Error", "Credenciales incorrectas", "OK");
    }

    boton.IsEnabled = true;
    boton.Text = "INICIAR SESIÓN";
}

    // Este es el enlace de "¿No tienes cuenta? Regístrate"
    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}