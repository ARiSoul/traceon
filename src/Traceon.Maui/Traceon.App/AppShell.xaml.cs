using Arisoul.Traceon.App.Views;

namespace Arisoul.Traceon.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(TrackedActionCreateOrEditPage), typeof(TrackedActionCreateOrEditPage));
        Routing.RegisterRoute(nameof(ActionEntryCreateOrEditPage), typeof(ActionEntryCreateOrEditPage));
    }
}
