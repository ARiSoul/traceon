using Syncfusion.Licensing;

namespace Arisoul.Traceon.App;

public partial class App : Application
{
    public App()
    {
        SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtecnVSQ2NfUURxV0RWYUA=");

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}