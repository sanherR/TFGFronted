namespace Apptech;

using Apptech.views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registramos las rutas de las páginas secundarias
        // Esto permite hacer Navigation.PushAsync o Shell.Current.GoToAsync
        Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
        Routing.RegisterRoute(nameof(ListaChatsPage), typeof(ListaChatsPage));
        Routing.RegisterRoute(nameof(PerfilPage), typeof(PerfilPage));
        Routing.RegisterRoute(nameof(VenderPage), typeof(VenderPage));
    }
}