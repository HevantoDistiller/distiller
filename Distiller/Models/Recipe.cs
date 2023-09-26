using System.ComponentModel;
using System.Data.Common;
using System.Security.Policy;
using Distiller.Services;

namespace Distiller.Models;

internal class Recipe : DBObject {
    public string Category { get; set; }
    public string Name { get; set; }
    public double Yield { get; set; }
    public string YieldUnit { get; set; }
    public string Description { get; set; }

    static Recipe()
    {
        RegisterCommandBuilder(typeof(Recipe), SQLite, typeof(SQLiteCommandBuilder));
    }

    public Recipe() : this("")
    {
    }

    public Recipe(string category) : base()
    {
        Category = category;
        Name = "";
        Yield = 0.0;
        YieldUnit = "";
        Description = "";
    }

    public static async Task<Recipe> LoadAsync(long id)
    {
        Recipe recipe = new()
        {
            ID = id
        };
        await dbService.GetById(recipe);
        return recipe;
    }

    public Recipe Load(long id)
    {
        Task<Recipe> task = Task.Run(async () => await LoadAsync(id));
        return task.Result;
    }

    public static async Task<IEnumerable<Recipe>> LoadAllForCategoryAsync(string category)
    {
        List<IDBObject> recipes = new();
        List<WhereParam> whereParams = new()
        {
            new WhereParam("category", category)
        };
        await dbService.Find(new Recipe(), whereParams, recipes);
        return recipes
            .ConvertAll(elem => (Recipe)elem)
            .OrderBy(elem => elem.Name);
    }

    public static IEnumerable<Recipe> LoadAllForCategory(string category)
    {
        Task<IEnumerable<Recipe>> task = Task.Run(async () => await LoadAllForCategoryAsync(category));
        return task.Result;
    }

    public override void ReadGetByIdResult(DbDataReader reader)
    {
        Category = reader.GetString(0);
        Name = reader.GetString(1);
        Yield = reader.GetDouble(2);
        YieldUnit = reader.GetString(3);
        Description = reader.GetString(4);
    }

    public override void ReadFindResult(DbDataReader reader, ICollection<IDBObject> resultSet)
    {
        Recipe elem = new()
        {
            ID = reader.GetInt64(0),
            Category = reader.GetString(1),
            Name = reader.GetString(2),
            Yield = reader.GetDouble(3),
            YieldUnit = reader.GetString(4),
            Description = reader.GetString(5),
            CreatedAt = reader.GetDateTime(6),
            UpdatedAt = reader.GetDateTime(7)
        };
        resultSet.Add(elem);
    }

    private class SQLiteCommandBuilder : IDBCommandBuilder
    {
        Recipe _recipe;

        public SQLiteCommandBuilder(Recipe recipe)
        {
            _recipe = recipe;
        }

        public IEnumerable<DbCommand> CreateTableCommand(DbConnection conn)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText = 
            @"
                CREATE TABLE IF NOT EXISTS Recipe (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    category TEXT NOT NULL,
                    name TEXT NOT NULL,
                    yield REAL NOT NULL,
                    yield_unit TEXT NOT NULL,
                    description TEXT,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                );
            ";
            commands.Add(command);

            command = conn.CreateCommand();
            command.CommandText = 
            @"
                CREATE INDEX IdxRecipe_category
                ON Recipe(category);
            ";
            commands.Add(command);

            command = conn.CreateCommand();
            command.CommandText = 
            @"
                CREATE INDEX IdxRecipe_name
                ON Recipe(name);
            ";
            commands.Add(command);

            command = conn.CreateCommand();
            command.CommandText = 
            @"
                CREATE UNIQUE INDEX IdxRecipe_category_name
                ON Recipe(category, name);
            ";
            commands.Add(command);

            return commands;
        }

        public IEnumerable<DbCommand> DeleteCommand(DbConnection conn)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText = 
            @"
                DELETE FROM Recipe
                WHERE ID = $id;
            ";
            AddParameter(command, "$id", _recipe.ID);
            commands.Add(command);

            return commands;
        }

        public IEnumerable<DbCommand> FindCommand(DbConnection conn, IEnumerable<WhereParam> whereParams)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText =
            @"
                SELECT
                    id,
                    category,
                    name,
                    yield,
                    yield_unit,
                    description,
                    created_at,
                    updated_at
                FROM Recipe
            ";
            WhereParam.BuildClause(whereParams, command);
            command.CommandText += ";";
            commands.Add(command);

            return commands;
        }

        public IEnumerable<DbCommand> GetByIdCommand(DbConnection conn)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText = 
            @"
                SELECT
                    category,
                    name,
                    yield,
                    yield_unit,
                    description,
                    created_at,
                    updated_at
                FROM Recipe
                WHERE id = $id;
            ";
            AddParameter(command, "$id", _recipe.ID);
            commands.Add(command);

            return commands;
        }

        public IEnumerable<DbCommand> InsertCommand(DbConnection conn)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText = 
            @"
                INSERT INTO Recipe (
                    category, name, yield, yield_unit, description,
                    created_at, updated_at
                ) VALUES (
                    $category, $name, $yield, $yield_unit, $description,
                    $created_at, $updated_at
                );
            ";
            AddParameter(command, "$category", _recipe.Category);
            AddParameter(command, "$name", _recipe.Name);
            AddParameter(command, "$yield", _recipe.Yield);
            AddParameter(command, "$yield_unit", _recipe.YieldUnit);
            AddParameter(command, "$description", _recipe.Description);
            AddParameter(command, "$created_at", _recipe.CreatedAt);
            AddParameter(command, "$updated_at", _recipe.UpdatedAt);
            commands.Add(command);

            command = conn.CreateCommand();
            command.CommandText =
            @"
                SELECT last_insert_rowid()
                FROM Recipe;
            ";
            commands.Add(command);

            return commands;
        }

        public IEnumerable<DbCommand> UpdateCommand(DbConnection conn)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText = 
            @"
                UPDATE Recipe SET
                    name = $name,
                    yield = $yield,
                    yield_unit = $yield_unit,
                    description = $description,
                    updated_at = $updated_at
                WHERE id = $id;
            ";
            AddParameter(command, "$name", _recipe.Name);
            AddParameter(command, "$yield", _recipe.Yield);
            AddParameter(command, "$yield_unit", _recipe.YieldUnit);
            AddParameter(command, "$description", _recipe.Description);
            AddParameter(command, "$updated_at", _recipe.UpdatedAt);
            AddParameter(command, "$id", _recipe.ID);
            commands.Add(command);

            return commands;
        }
    }
}