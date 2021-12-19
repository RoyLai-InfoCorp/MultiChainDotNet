using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:12028/transaction")
    .WithAutomaticReconnect()
    .Build();
Console.WriteLine(connection.State);
while (connection.State != HubConnectionState.Connected)
{
    try
    {
        await Task.Delay(1000);
        await connection.StartAsync();
    }
    catch
    {
    }
}
Console.WriteLine(connection.State);
connection.On<string>("Publish", (raw) =>
{
    Console.WriteLine(raw);
});
Console.ReadLine();

