namespace Distiller.Controls;

public partial class FlyoutHeader : ContentView
{
    public FlyoutHeader()
    {
        InitializeComponent();

        AppNameLabel.Text = AppInfo.Name;
    }
}