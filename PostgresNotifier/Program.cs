using Dapper;
using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=database;Username=root;Password=password";

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

while (true)
{
    var guid = Guid.NewGuid().ToString();
    Console.WriteLine($"Sending event {guid}");
    await conn.ExecuteAsync("NOTIFY my_channel, '" + guid + "';", new { Payload = guid });
    await Task.Delay(100);
}