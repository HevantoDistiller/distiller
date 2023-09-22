using Distiller.ViewModels.Info;

namespace Distiller.Views.Info;

public partial class LicensesPage : ContentPage
{
    public LicensesPage()
    {
        InitializeComponent();
    }

    private async void ContentPage_NavigatedTo(object sender, NavigatedToEventArgs e)
    {
        if (BindingContext is LicensesViewModel licensesViewModel) {
            await licensesViewModel.LoadLicenses();
        }
    }

}