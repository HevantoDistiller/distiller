using System.Collections.ObjectModel;

namespace Distiller.ViewModels.Info;

internal class LicensesViewModel
{
    public ObservableCollection<LicenseViewModel> AllLicenses { get; }

    public string LicenseInfo => "The Distiller application uses the following external libraries, icons etc.";

    
    public LicensesViewModel()
    {
        AllLicenses = new ObservableCollection<LicenseViewModel>();
    }

    public async Task LoadLicenses()
    {
        if (AllLicenses.Count == 0) {
            var _lst = await Models.LicenseInfo.LoadAll();
            foreach (var li in _lst) {
                AllLicenses.Add(new LicenseViewModel(li));
            }
        }
    }
}