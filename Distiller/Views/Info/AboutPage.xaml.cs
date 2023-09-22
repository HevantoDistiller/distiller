using Distiller.ViewModels.Info;

namespace Distiller.Views.Info;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private async void ContentPage_NavigatedTo(object sender, NavigatedToEventArgs e)
    {
        if (BindingContext is AboutViewModel aboutViewModel) {
            await aboutViewModel.LoadAboutHtml();
        }
    }
}