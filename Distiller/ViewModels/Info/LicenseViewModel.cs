using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Distiller.ViewModels.Info;

internal class LicenseViewModel : ObservableObject
{
    private Models.LicenseInfo _licenseInfo;
    public ICommand OpenUrlCommand => new AsyncRelayCommand<string>(LaunchBrowser);

    public string Text => _licenseInfo.Text;
    public string Link => _licenseInfo.Link;

    public LicenseViewModel()
    {
        _licenseInfo = new Models.LicenseInfo();
    }

    public LicenseViewModel(Models.LicenseInfo licenseInfo)
    {
        _licenseInfo = licenseInfo;
    }

    async Task LaunchBrowser(string url) => 
        await Launcher.Default.OpenAsync(url);
}