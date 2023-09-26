using System.Data.Common;

namespace Distiller.Services;

internal struct WhereParam
{
    public string Field;
    public object Data;

    public WhereParam(string field, object data)
    {
        Field = field;
        Data = data;
    }

    public static void BuildClause(IEnumerable<WhereParam> prms, DbCommand command) {
        string op = "WHERE";
        foreach (WhereParam p in prms) {
            if (p.Data is not null)
            {
                command.CommandText += $" {op} {p.Field} = ${p.Field}";
                DbParameter dbp = command.CreateParameter();
                dbp.ParameterName = $"${p.Field}";
                dbp.Value = p.Data;
                command.Parameters.Add(dbp);
            }
            else
                command.CommandText += $" {op} {p.Field} is null";

            op = "AND";
        }
    }
}

internal interface IDBCommandBuilder
{
    IEnumerable<DbCommand> CreateTableCommand(DbConnection conn);
    IEnumerable<DbCommand> InsertCommand(DbConnection conn);
    IEnumerable<DbCommand> UpdateCommand(DbConnection conn);
    IEnumerable<DbCommand> DeleteCommand(DbConnection conn);
    IEnumerable<DbCommand> GetByIdCommand(DbConnection conn);
    IEnumerable<DbCommand> FindCommand(DbConnection conn, IEnumerable<WhereParam> whereParams);
}

internal interface IDBObject : IDBCommandBuilder
{
    long ID { get; set; }

    void ReadGetByIdResult(DbDataReader reader);
    void ReadFindResult(DbDataReader reader, ICollection<IDBObject> resultSet);
}

internal interface IDBDriver
{
    DbConnection OpenConnection();
}


internal class DBService
{
    private IDBDriver Driver { get; set; }

    public DBService(string driver, string connectionString)
    {
        Driver = null;
        if (driver == "sqlite") {
            Driver = new SQLiteDriver(connectionString);
        }

        if (Driver == null)
            throw new ArgumentException($"Invalid driver configuration: {driver}: {connectionString}");
    }

    public async Task EnsureTableExists(IDBObject dbObject)
    {
        DbConnection conn = Driver.OpenConnection();
        if (conn is null)
            throw new IOException("No database driver initialized");

        DbTransaction tx = await conn.BeginTransactionAsync();
        try {
            IEnumerable<DbCommand> commands = dbObject.CreateTableCommand(conn);
            if (commands.Count() > 0)
                foreach (DbCommand command in commands)
                    await command.ExecuteNonQueryAsync();

            tx.Commit();
        } catch (Exception) {
            tx.Rollback();
            throw;
        } finally {
            conn.Close();
        }
    }

    public async Task<long> Insert(IDBObject dbObject)
    {
        await EnsureTableExists(dbObject);

        DbConnection conn = Driver.OpenConnection();
        if (conn is null)
            throw new IOException("No database driver initialized");

        long id = 0;
        DbTransaction tx = await conn.BeginTransactionAsync();
        try {
            IEnumerable<DbCommand> commands = dbObject.InsertCommand(conn);
            if (commands.Count() > 0)
            {
                for (var i=0; i<commands.Count()-1; i++)
                    await commands.ElementAt(i).ExecuteNonQueryAsync();

                using (var reader = await commands.Last().ExecuteReaderAsync())
                    while (await reader.ReadAsync())
                        id = reader.GetInt64(0);
            }
            tx.Commit();
        } catch (Exception) {
            tx.Rollback();
            throw;
        } finally {
            conn.Close();
        }

        return id;
    }

    public async Task Update(IDBObject dbObject)
    {
        DbConnection conn = Driver.OpenConnection();
        if (conn is null)
            throw new IOException("No database driver initialized");

        DbTransaction tx = await conn.BeginTransactionAsync();
        try {
            IEnumerable<DbCommand> commands = dbObject.UpdateCommand(conn);
            if (commands.Count() > 0)
                foreach (DbCommand command in commands)
                    await command.ExecuteNonQueryAsync();
            tx.Commit();
        } catch (Exception) {
            tx.Rollback();
            throw;
        } finally {
            conn.Close();
        }
    }
    
    public async Task Delete(IDBObject dbObject)
    {
        DbConnection conn = Driver.OpenConnection();
        if (conn is null)
            throw new IOException("No database driver initialized");

        DbTransaction tx = await conn.BeginTransactionAsync();
        try {
            IEnumerable<DbCommand> commands = dbObject.DeleteCommand(conn);
            if (commands.Count() > 0)
                foreach (DbCommand command in commands)
                    await command.ExecuteNonQueryAsync();
            tx.Commit();
        } catch (Exception) {
            tx.Rollback();
            throw;
        } finally {
            conn.Close();
        }
    }

    public async Task GetById(IDBObject dbObject)
    {
        DbConnection conn = Driver.OpenConnection();
        if (conn is null)
            throw new IOException("No database driver initialized");

        DbTransaction tx = await conn.BeginTransactionAsync();
        try {
            IEnumerable<DbCommand> commands = dbObject.GetByIdCommand(conn);
            for (var i=0; i<commands.Count()-1; i++)
                await commands.ElementAt(i).ExecuteNonQueryAsync();

            using (var reader = await commands.Last().ExecuteReaderAsync())
                while (await reader.ReadAsync())
                    dbObject.ReadGetByIdResult(reader);
            tx.Commit();
        } catch (Exception) {
            tx.Rollback();
            throw;
        } finally {
            conn.Close();
        }
    }

    public async Task Find(IDBObject dbObject, IEnumerable<WhereParam> whereParams, ICollection<IDBObject> resultSet)
    {
        DbConnection conn = Driver.OpenConnection();
        if (conn is null)
            throw new IOException("No database driver initialized");

        DbTransaction tx = await conn.BeginTransactionAsync();
        try {
            IEnumerable<DbCommand> commands = dbObject.FindCommand(conn, whereParams);
            for (var i=0; i<commands.Count()-1; i++)
                await commands.ElementAt(i).ExecuteNonQueryAsync();

            using (var reader = await commands.Last().ExecuteReaderAsync())
                while (await reader.ReadAsync())
                    dbObject.ReadFindResult(reader, resultSet);
            tx.Commit();
        } catch (Exception) {
            tx.Rollback();
            throw;
        } finally {
            conn.Close();
        }
    }
}