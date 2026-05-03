using Microsoft.Maui.Handlers;
using Microsoft.Extensions.Logging; // ESTA es la que te falta para el AddDebug
using Apptech.Services; 
using Apptech.views;

#if ANDROID
using Android.Graphics.Drawables;
using Microsoft.Extensions.Logging;
using Android.Content.Res;
#endif

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

#if ANDROID
        SearchBarHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            var native = handler.PlatformView;

            native.SetBackgroundColor(Android.Graphics.Color.Transparent);

            native.Background = new ColorDrawable(Android.Graphics.Color.Transparent);

            native.SetPadding(0, 0, 0, 0);
        });
#endif

#if ANDROID
SearchBarHandler.Mapper.AppendToMapping("Fix", (handler, view) =>
{
    var native = handler.PlatformView;

    native.SetBackgroundColor(Android.Graphics.Color.Transparent);
    native.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
    native.TranslationZ = 0;
});
#endif
// --- REGISTRO DE SERVICIOS Y PÁGINAS ---
        builder.Services.AddSingleton<ApiService>();
        
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ProductosPage>();
        builder.Services.AddTransient<VenderPage>();
        builder.Services.AddTransient<DetalleProductoPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}