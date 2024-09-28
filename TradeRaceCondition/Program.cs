using Dapper;
using Npgsql;
using TradeRaceCondition;

const string connectionString = "Host=localhost;Port=5432;Database=database;Username=root;Password=password";

await CreateTable();

await InsertItem("1", "User1");
await InsertItem("2", "User1");
await InsertItem("3", "User2");
await InsertItem("4", "User3");

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

var transactions = Console.ReadKey().KeyChar == '1'
    ? new List<ItemTransaction>()
    {
        new ItemTransaction("1", "User1", "User2"),
        new ItemTransaction("3", "User2", "User1")
    }
    : new List<ItemTransaction>()
    {
        new ItemTransaction("1", "User1", "User3"),
        new ItemTransaction("2", "User1", "User3"),
        new ItemTransaction("4", "User3", "User1")
    };

var transaction = await StartTransactions(conn, transactions);

await transaction.CommitAsync();

static async Task<NpgsqlTransaction> StartTransactions(NpgsqlConnection connection, List<ItemTransaction> transactions)
{
    var transaction = connection.BeginTransaction();

    foreach (var itemTransaction in transactions)
    {
        var sql = @"
                            UPDATE Items 
                            SET Owner = @ToUser
                            WHERE id = @ItemId AND Owner = @FromUser;
                        ";

        var rowsAffected = await connection.ExecuteAsync(sql,
            new
            {
                itemTransaction.ItemId,
                itemTransaction.FromUser,
                itemTransaction.ToUser
            },
            transaction: transaction);

        if (rowsAffected == 0)
        {
            throw new Exception($"Item transfer failed for ItemId: {itemTransaction.ItemId}, owner mismatch.");
        }
    }

    return transaction;
}

static async Task InsertItem(string id, string owner)
{
    await using var conn = new NpgsqlConnection(connectionString);

    await conn.ExecuteAsync("INSERT INTO Items (Id, Owner) VALUES (@Id, @Owner)",
        new
        {
            Id = id,
            Owner = owner
        });
}

static async Task CreateTable()
{
    await using var conn = new NpgsqlConnection(connectionString);

    await conn.ExecuteAsync(@"DROP TABLE IF EXISTS Items;");

    await conn.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Items (
                    Id VARCHAR(255) PRIMARY KEY,
                    Owner VARCHAR(255) NOT NULL
                );");
}