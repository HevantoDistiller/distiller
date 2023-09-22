namespace Distiller.Models;

internal class LicenseInfo {
    public string Text { get; set; }
    public string Link { get; set; }

    public LicenseInfo()
    {
        Text = "";
        Link = "";
    }

    public async static Task<LicenseInfo> Load(string filename)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync($"Licenses/{filename}");
        using var reader = new StreamReader(stream);

        var text = reader.ReadLine();
        var link = reader.ReadLine();

        return
            new()
            {
                Text = text,
                Link = link
            };
    }

    public async static Task<IEnumerable<LicenseInfo>> LoadAll()
    {
        List<LicenseInfo> lst = new List<LicenseInfo>
        {
            await Load("uicons.txt"),
            await Load("brewicons.txt")
        };
        return lst;
    }
}