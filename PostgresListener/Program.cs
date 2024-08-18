using Dapper;
using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=database;Username=root;Password=password";

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

conn.Notification += (o, e) => Console.WriteLine($"Received notification: {e.Payload}");

await conn.ExecuteAsync("LISTEN my_channel;");

Console.Write("Listening for events....");

while (true)
{
    await conn.WaitAsync();
}