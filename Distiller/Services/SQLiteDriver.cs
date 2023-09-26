using System.Data.Common;
using System.Data.SQLite;

namespace Distiller.Services;

internal class SQLiteDriver : IDBDriver
{

    private string _connectionString;

    public SQLiteDriver(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection OpenConnection()
    {
        DbConnection conn = new SQLiteConnection(EnsureDBLocation(_connectionString));
        return conn;
    }

    private string EnsureDBLocation(string connectionString)
    {
        DbConnectionStringBuilder builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        string fileName = Path.GetFileName(((string)builder["Data Source"]).Trim());
        builder["Data Source"] = Path.Combine(FS.AppDataDirectory, fileName);

        return builder.ConnectionString;
    }
}