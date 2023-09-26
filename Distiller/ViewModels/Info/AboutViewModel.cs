using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Distiller.ViewModels.Info;

internal class AboutViewModel : ObservableObject
{
    public string Title => AppInfo.Name;
    public string Version => AppInfo.VersionString;
    public string GithubPage => "https://github.com/HevantoDistiller/distiller";

    public string AboutHtml {
        get {
            if (_About is null) {
                return "";
            }
            return _About.HtmlContent;
        }
    }

    public ICommand ShowGithubPage => new AsyncRelayCommand(LaunchGithubPage);

    private Models.About _About = null;

    public AboutViewModel()
    {
    }

    public async Task LoadAboutHtml()
    {
        if (_About is null) {
            _About = await Models.About.Load();
            OnPropertyChanged(nameof(AboutHtml));
        }
    }

    async Task LaunchGithubPage() =>
        await Launcher.Default.OpenAsync(GithubPage);
}