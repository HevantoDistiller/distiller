using SmartFormat;

namespace Distiller.Models;

internal class About {
    public string HtmlContent { get; set; }

    public About()
    {
        HtmlContent = "";
    }

    public async static Task<About> Load()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("about.html");
        using var reader = new StreamReader(stream);

        var data = new {
            AppName = AppInfo.Name,
            Version = AppInfo.VersionString,
            AppInfo.BuildString
        };
        var htmlContent = Smart.Format(reader.ReadToEnd(), data);

        return
            new()
            {
                HtmlContent = htmlContent
            };
    }
}