using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using SubnetPro.Core.Accounts;
using SubnetPro.Core.Data;
using SubnetPro.Core.History;
using SubnetPro.Services;
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

        // Données : SQLite, dans le dossier privé de l'application (conservé entre les lancements).
        builder.Services.AddSingleton(_ =>
            new AppDatabase(Path.Combine(FileSystem.AppDataDirectory, "subnetpro.db3")));

        // Comptes locaux et session (le hachage PBKDF2 est dans PasswordHasher).
        builder.Services.AddSingleton(new PasswordHasher());
        builder.Services.AddSingleton<IAccountRepository, SqliteAccountRepository>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<ISessionService>(sp => sp.GetRequiredService<SessionService>());
        builder.Services.AddSingleton<IUserContext>(sp => sp.GetRequiredService<SessionService>());

        // L'historique est filtré par utilisateur grâce à IUserContext.
        builder.Services.AddSingleton<IHistoryRepository, SqliteHistoryRepository>();

        // Pages et ViewModels : chaque nouvelle page avec ViewModel s'enregistre ici.
        builder.Services.AddTransient<SplashViewModel>();
        builder.Services.AddTransient<SplashPage>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

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