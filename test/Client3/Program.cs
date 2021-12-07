using Microsoft.AspNetCore.SignalR.Client;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Client3
{
	internal class Program
	{

		static HubConnection connection;
		static async Task WaitForConnection()
		{
			connection = new HubConnectionBuilder()
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
		}

		static async Task Main(string[] args)
		{
			await WaitForConnection();
			connection.On<DecodeRawTransactionResult>("Publish", (raw) =>
			{
				Console.WriteLine(JsonConvert.SerializeObject(raw));
			});
			Console.ReadLine();
		}
	}
}
