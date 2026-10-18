using Microsoft.Extensions.Logging;
using SubnetPro.ViewModels;
using SubnetPro.Views;

namespace SubnetPro;

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

        // Pages et ViewModels : chaque nouvelle page avec ViewModel s'enregistre ici.
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<HomePage>();

        builder.Services.AddTransient<SubnetViewModel>();
        builder.Services.AddTransient<SubnetPage>();

        builder.Services.AddTransient<VlsmViewModel>();
        builder.Services.AddTransient<VlsmPage>();
        builder.Services.AddTransient<ClassConversionViewModel>();
        builder.Services.AddTransient<ClassConversionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}