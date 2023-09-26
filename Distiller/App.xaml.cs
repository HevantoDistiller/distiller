namespace Distiller;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		LoadSettings();
		LoadStaticData();

		MainPage = new AppShell();
	}

	private void LoadSettings()
	{
		// Ensure the singleton gets initialized
		var cfg = Distiller.Models.AppConfig.Instance;
	}

	private void LoadStaticData()
	{
		var units = Distiller.Models.UnitsOfMeasurement.Instance;
	}
}
