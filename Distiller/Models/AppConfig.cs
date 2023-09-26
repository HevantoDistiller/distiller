using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace Distiller.Models;

internal sealed class AppConfig {
    // This class is a singleton, and is automatically initialized
    // during application startup.
    internal class DatabaseConnectionConfig
    {
        public string Type {
            get { return _type; }
            set {         
                _type = Strings.LCase(value);
            }
        }
        private string _type;

        public string ConnectionString { get; set; }

        public DatabaseConnectionConfig()
        {
            Type = "sqlite";
            ConnectionString = "Data Source=distiller.db;Version=3;Compress=True";
        }
    }

    private static readonly Lazy<AppConfig> lazy = new Lazy<AppConfig>(() => Load());

    public static AppConfig Instance { get { return lazy.Value; } }

    public DatabaseConnectionConfig DatabaseConnection { get; set; }

    private AppConfig()
    {
        DatabaseConnection = null;
    }

    private void InitToDefaults()
    {
        DatabaseConnection = new DatabaseConnectionConfig();
    }

    private static AppConfig Load()
    {
        var serializer = new JsonSerializer();
        string fileName = System.IO.Path.Combine(FS.AppDataDirectory, "distiller.cfg");
        AppConfig cfg = new();

        if (!System.IO.Path.Exists(fileName))
        {
            cfg.InitToDefaults();

            using (var stream = File.Open(fileName, FileMode.CreateNew))
            using (var streamWriter = new StreamWriter(stream))
            using (var textWriter = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(textWriter, cfg);
            }
            return cfg;
        }


        using (var stream = File.Open(fileName, FileMode.Open))
        using (var streamReader = new StreamReader(stream))
        using (var textReader = new JsonTextReader(streamReader))
        {
            cfg = serializer.Deserialize<AppConfig>(textReader);
        }
        return cfg;
    }

    public void Save()
    {
        var serializer = new JsonSerializer();
        string fileName = System.IO.Path.Combine(FS.AppDataDirectory, "distiller.cfg");
        
        using (var stream = File.Open(fileName, FileMode.Create))
        using (var streamWriter = new StreamWriter(stream))
        using (var textWriter = new JsonTextWriter(streamWriter))
        {
            serializer.Serialize(textWriter, this);
        }
    }

    public async Task SaveAsync()
    {
        await Task.Run(() => Save());
    }
}