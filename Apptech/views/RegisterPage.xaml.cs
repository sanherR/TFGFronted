using System.Net.Http.Json;
using Apptech.Models; 
using Apptech.Services;

namespace Apptech.views;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public RegisterPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gestiona el registro del nuevo usuario contra la API
    /// </summary>
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var boton = (Button)sender;

        boton.IsEnabled = false;
        boton.Text = "REGISTRANDO...";

        try
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                await DisplayAlert("Atención", "Rellena todos los campos", "OK");
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = txtUsuario.Text,
                Email = txtEmail.Text,
                Contrasena = txtPassword.Text,
                Direccion = "No especificada",
                Telefono = "000000000"
            };

            var ok = await _apiService.Register(nuevoUsuario);

            if (ok)
            {
                await DisplayAlert("OK", "Cuenta creada correctamente", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "No se pudo crear la cuenta", "OK");
            }
        }
        catch
        {
            await DisplayAlert("Error", "Fallo de conexión", "OK");
        }
        finally
        {
            // Nos aseguramos de restaurar el botón pase lo que pase
            boton.IsEnabled = true;
            boton.Text = "REGISTRARME";
        }
    }

    /// <summary>
    /// Cierra la pantalla actual al pulsar la flecha superior (ImageButton)
    /// </summary>
    private async void OnVolverAlLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Cierra la pantalla actual al pulsar el texto inferior (TapGestureRecognizer)
    /// </summary>
    private async void OnVolverAlLoginTextoTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}