using System.Data.Common;
using Distiller.Services;

namespace Distiller.Models;

internal class RecipeIngredient : DBObject {
    public long RecipeID { get; set; }
    public string Name { get; set; }
    public double Amount { get; set; }
    public string Unit { get; set; }

    static RecipeIngredient()
    {
        RegisterCommandBuilder(typeof(RecipeIngredient), SQLite, typeof(SQLiteCommandBuilder));
    }

    public RecipeIngredient() : base()
    {
        RecipeID = 0;
        Name = "";
        Amount = 0.0;
        Unit = "";
    }

    public static async Task<RecipeIngredient> Load(long id)
    {
        RecipeIngredient ingredient = new()
        {
            ID = id
        };
        await dbService.GetById(ingredient);
        return ingredient;
    }

    public static async Task<IEnumerable<RecipeIngredient>> LoadAllForRecipeAsync(long recipeId)
    {
        List<IDBObject> ingredients = new();
        List<WhereParam> whereParams = new()
        {
            new WhereParam("recipe_id", recipeId)
        };
        await dbService.Find(new RecipeIngredient(), whereParams, ingredients);
        return ingredients
            .ConvertAll(elem => (RecipeIngredient)elem)
            .OrderBy(elem => elem.Name);
    }

    public static IEnumerable<RecipeIngredient> LoadAllForRecipe(long recipeId)
    {
        Task<IEnumerable<RecipeIngredient>> task = Task.Run(async () => await LoadAllForRecipeAsync(recipeId));
        return task.Result;
    }

    public override void ReadGetByIdResult(DbDataReader reader)
    {
        RecipeID = reader.GetInt64(0);
        Name = reader.GetString(1);
        Amount = reader.GetDouble(2);
        Unit = reader.GetString(3);
        CreatedAt = reader.GetDateTime(4);
        UpdatedAt = reader.GetDateTime(5);
    }

    public override void ReadFindResult(DbDataReader reader, ICollection<IDBObject> resultSet)
    {
        RecipeIngredient elem = new()
        {
            ID = reader.GetInt64(0),
            RecipeID = reader.GetInt64(1),
            Name = reader.GetString(2),
            Amount = reader.GetDouble(3),
            Unit = reader.GetString(4),
            CreatedAt = reader.GetDateTime(5),
            UpdatedAt = reader.GetDateTime(6)
        };
        resultSet.Add(elem);
    }

    private class SQLiteCommandBuilder : IDBCommandBuilder
    {
        RecipeIngredient _ingredient;

        public SQLiteCommandBuilder(RecipeIngredient ingredient)
        {
            _ingredient = ingredient;
        }

        public IEnumerable<DbCommand> CreateTableCommand(DbConnection conn)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText = 
            @"
                CREATE TABLE IF NOT EXISTS RecipeIngredient (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    recipe_id INTEGER NOT NULL,
                    name TEXT NOT NULL,
                    amount REAL NOT NULL,
                    unit TEXT NOT NULL,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                );
            ";
            commands.Add(command);

            command = conn.CreateCommand();
            command.CommandText = 
            @"
                CREATE INDEX IdxRecipeIngredient_recipe_id
                ON RecipeIngredients(recipe_id);
            ";
            commands.Add(command);

            command = conn.CreateCommand();
            command.CommandText = 
            @"
                CREATE INDEX IdxRecipeIngredient_name
                ON RecipeIngredients(name);
            ";
            commands.Add(command);

            command = conn.CreateCommand();
            command.CommandText =
            @"
                CREATE UNIQUE INDEX IdxRecipeIngredient_recipe_id_name
                ON RecipeIngredients(recipe_id, name);
            ";
            commands.Add(command);

            return commands;
        }

        public IEnumerable<DbCommand> InsertCommand(DbConnection conn)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText = 
            @"
                INSERT INTO RecipeIngredient (
                    recipe_id, name, amount, unit,
                    created_at, updated_at
                ) VALUES (
                    $recipeId, $name, $amount, $unit,
                    $created_at, $updated_at
                );
            ";
            AddParameter(command, "$recipeId", _ingredient.RecipeID);
            AddParameter(command, "$name", _ingredient.Name);
            AddParameter(command, "$amount", _ingredient.Amount);
            AddParameter(command, "$unit", _ingredient.Unit);
            AddParameter(command, "$created_at", _ingredient.CreatedAt);
            AddParameter(command, "$updated_at", _ingredient.UpdatedAt);
            commands.Add(command);

            command = conn.CreateCommand();
            command.CommandText = 
            @"
                SELECT last_insert_rowid()
                FROM RecipeIngredient;
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
                UPDATE RecipeIngredient SET
                    name = $name,
                    amount = $amount,
                    unit = $unit,
                    updated_at = $updated_at
                WHERE id = $id;
            ";
            AddParameter(command, "$name", _ingredient.Name);
            AddParameter(command, "$amount", _ingredient.Amount);
            AddParameter(command, "$unit", _ingredient.Unit);
            AddParameter(command, "$updated_at", _ingredient.UpdatedAt);
            AddParameter(command, "$id", _ingredient.ID);
            commands.Add(command);

            return commands;
        }

        public IEnumerable<DbCommand> DeleteCommand(DbConnection conn)
        {
            List<DbCommand> commands = new();

            DbCommand command = conn.CreateCommand();
            command.CommandText =
            @"
                DELETE FROM RecipeIngredient
                WHERE id = $id;
            ";
            AddParameter(command, "$id", _ingredient.ID);
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
                    recipe_id,
                    name,
                    amount,
                    unit,
                    created_at,
                    updated_at
                FROM RecipeIngredient
                WHERE id = $id;
            ";
            AddParameter(command, "$id", _ingredient.ID);
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
                    recipe_id,
                    name,
                    amount,
                    unit,
                    created_at,
                    updated_at
                FROM RecipeIngredient
            ";
            WhereParam.BuildClause(whereParams, command);
            command.CommandText += ";";
            commands.Add(command);

            return commands;
        }
    }
}
