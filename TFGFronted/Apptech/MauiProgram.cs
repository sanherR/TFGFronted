using Microsoft.Extensions.Logging;
using Apptech.Services; // Asegúrate de que estos nombres coinciden con tus carpetas
using Apptech.views;

namespace Apptech;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- LAS LÍNEAS QUE ESTABAN EN ROJO VAN AQUÍ DENTRO ---
        
        // Registrar HttpClient para el PedidoService
        builder.Services.AddHttpClient<PedidoService>();

        // Registrar la página para que soporte Inyección de Dependencias
        builder.Services.AddTransient<ProductosPage>();
        
        // También registra el ApiService si no lo has hecho
        builder.Services.AddSingleton<ApiService>();

        // -----------------------------------------------------

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}