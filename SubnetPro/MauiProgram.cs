using Microsoft.Extensions.Logging;
using SubnetPro.Core.History;
using SubnetPro.ViewModels;
using SubnetPro.Views;
#if ANDROID
using Microsoft.Maui.Platform;
#endif

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

#if ANDROID
        // Android dessine un trait sous les champs ; nos champs ont déjà leur bordure (style Field).
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform()));

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform()));
#endif

        // Données : version en mémoire (jour 4). Le jour 5 la remplace par SQLite.
        builder.Services.AddSingleton<IHistoryRepository, InMemoryHistoryRepository>();

        // Pages et ViewModels : chaque nouvelle page avec ViewModel s'enregistre ici.
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<HomePage>();

        builder.Services.AddTransient<SubnetViewModel>();
        builder.Services.AddTransient<SubnetPage>();

        builder.Services.AddTransient<VlsmViewModel>();
        builder.Services.AddTransient<VlsmPage>();

        builder.Services.AddTransient<ClassConversionViewModel>();
        builder.Services.AddTransient<ClassConversionPage>();

        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<HistoryPage>();

        builder.Services.AddTransient<ResultDetailViewModel>();
        builder.Services.AddTransient<ResultDetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}