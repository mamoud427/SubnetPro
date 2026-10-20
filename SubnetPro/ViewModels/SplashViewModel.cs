using SubnetPro.Services;

namespace SubnetPro.ViewModels;

public class SplashViewModel : BaseViewModel
{
    private static readonly TimeSpan MinimumDisplay = TimeSpan.FromMilliseconds(1200);

    private readonly ISessionService session;
    private bool hasStarted;

    public SplashViewModel(ISessionService session)
    {
        this.session = session;
    }

    /// <summary>Relit la session puis ouvre l'accueil (utilisateur reconnu) ou la connexion.</summary>
    public async Task StartAsync()
    {
        if (hasStarted)
            return;
        hasStarted = true;

        var startedAt = DateTime.UtcNow;
        var signedIn = await session.RestoreAsync();

        // L'écran d'ouverture reste visible un instant, même si la session est lue instantanément.
        var remaining = MinimumDisplay - (DateTime.UtcNow - startedAt);
        if (remaining > TimeSpan.Zero)
            await Task.Delay(remaining);

        await Shell.Current.GoToAsync(signedIn ? "//home" : "//login");
    }
}