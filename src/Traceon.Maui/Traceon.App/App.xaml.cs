using Syncfusion.Licensing;

namespace Arisoul.Traceon.App;

public partial class App : Application
{
    public App()
    {
        SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZeeXVVRWhfUkB1WkBWYEg=");

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}