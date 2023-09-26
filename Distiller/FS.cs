namespace Distiller;

public static class FS
{
    public static string AppDataDirectory
    {
        get {
            string dir = System.IO.Path.Combine(FileSystem.AppDataDirectory, $"{AppInfo.PackageName}.{AppInfo.Name}");
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            return dir;
        }
    }
}