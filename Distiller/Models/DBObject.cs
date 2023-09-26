using System.Data.Common;
using Distiller.Services;

namespace Distiller.Models;

internal abstract class DBObject : IDBObject {
    internal static string SQLite = "sqlite";

    public long ID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    internal static Dictionary<Type, Dictionary<string, Type>> _commandBuilders;

    static DBObject()
    {
        _commandBuilders = new Dictionary<Type, Dictionary<string, Type>>();
    }

    protected DBObject()
    {
        ID = 0;
        CreatedAt = DateTime.MinValue;
        UpdatedAt = DateTime.MinValue;
    }

    internal static void RegisterCommandBuilder(Type objType, string dbType, Type builder)
    {
        if (_commandBuilders.ContainsKey(objType))
            _commandBuilders[objType] = new Dictionary<string, Type>();

        _commandBuilders[objType][dbType] = builder;
    }

    internal static IDBCommandBuilder GetCommandBuilder(Type objType, DBObject obj)
    {
        if (!_commandBuilders.ContainsKey(objType))
            return null;

        if (!_commandBuilders[objType].ContainsKey(AppConfig.Instance.DatabaseConnection.Type))
            return null;

        Type builderType = _commandBuilders[objType][AppConfig.Instance.DatabaseConnection.Type];
        return (IDBCommandBuilder)Activator.CreateInstance(builderType, Convert.ChangeType(obj, objType));
    }

    internal static void AddParameter(DbCommand command, string paramName, object paramValue)
    {
        DbParameter prm = command.CreateParameter();
        prm.ParameterName = paramName;
        prm.Value = paramValue;
        command.Parameters.Add(prm);
    }

    internal static DBService dbService {
        get {
            if (_dbService != null)
                return _dbService;

            AppConfig cfg = AppConfig.Instance;
            _dbService = new DBService(cfg.DatabaseConnection.Type, cfg.DatabaseConnection.ConnectionString);
            return _dbService;
        }
    }
    private static DBService _dbService = null;

    public abstract void ReadGetByIdResult(DbDataReader reader);
    public abstract void ReadFindResult(DbDataReader reader, ICollection<IDBObject> resultSet);

    public async Task Save()
    {
        if (ID == 0)
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = CreatedAt;
            ID = await dbService.Insert(this);
        }
        else
        {
            UpdatedAt = DateTime.Now;
            await dbService.Update(this);
        }
    }

    public async Task Delete()
    {
        await dbService.Delete(this);
    }

    public IEnumerable<DbCommand> CreateTableCommand(DbConnection conn)
    {
        IDBCommandBuilder builder = GetCommandBuilder(GetType(), this);
        if (builder is null)
            return null;
        return builder.CreateTableCommand(conn);
    }
    
    public IEnumerable<DbCommand> InsertCommand(DbConnection conn)
    {
        IDBCommandBuilder builder = GetCommandBuilder(GetType(), this);
        if (builder is null)
            return null;
        return builder.InsertCommand(conn);
    }

    public IEnumerable<DbCommand> UpdateCommand(DbConnection conn)
    {
        IDBCommandBuilder builder = GetCommandBuilder(GetType(), this);
        if (builder is null)
            return null;
        return builder.UpdateCommand(conn);
    }

    public IEnumerable<DbCommand> DeleteCommand(DbConnection conn)
    {
        IDBCommandBuilder builder = GetCommandBuilder(GetType(), this);
        if (builder is null)
            return null;
        return builder.DeleteCommand(conn);
    }

    public IEnumerable<DbCommand> GetByIdCommand(DbConnection conn)
    {
        IDBCommandBuilder builder = GetCommandBuilder(GetType(), this);
        if (builder is null)
            return null;
        return builder.GetByIdCommand(conn);
    }

    public IEnumerable<DbCommand> FindCommand(DbConnection conn, IEnumerable<WhereParam> whereParams)
    {
        IDBCommandBuilder builder = GetCommandBuilder(GetType(), this);
        if (builder is null)
            return null;
        return builder.FindCommand(conn, whereParams);
    }
}