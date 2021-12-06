using MultiChainDotNet.Api.Abstractions.Extensions;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace Client2
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("press enter to register...");
			Console.ReadLine();
			var address = "abc";
			using (ClientWebSocket client = new ClientWebSocket())
			{
				var cts = new CancellationTokenSource();
				cts.CancelAfter(TimeSpan.FromSeconds(120));
				var buffer = new byte[1024];

				try
				{
					await client.ConnectAsync(new Uri("ws://localhost:12028/socket/subscribe"), cts.Token);
					await client.ClientSendAsync(address, cts.Token);
					while (client.State == WebSocketState.Open)
					{
						var (payload,res) = await client.ClientReceiveAsync(buffer,cts.Token);
						Console.WriteLine(payload);
					}
				}
				catch (WebSocketException e)
				{
					Console.WriteLine(e.Message);
				}

			}

			Console.ReadLine();
		}
	}
}
