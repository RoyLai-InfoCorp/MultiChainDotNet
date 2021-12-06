using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client1
{
	internal class Program
	{
		static HttpClient client = new HttpClient();
		static async Task Main(string[] args)
		{
			int counter = 0;

			Console.WriteLine("Press 1 to notify a new transaction. Q to quit");
			string cmd = "";
			while (cmd.ToLower()!="q")
			{
				cmd = Console.ReadLine();
			
				if (cmd == "1")
				{
					counter++;
					var txn = new DecodeRawTransactionResult { Txid = counter.ToString() };
					try
					{
						var response = await client.PostAsync("http://localhost:12028/transaction", new StringContent(JsonConvert.SerializeObject(txn), Encoding.UTF8, "application/json"));
						Console.WriteLine(response.ReasonPhrase);
						var res = await response.Content.ReadAsStringAsync();
						Console.WriteLine(res);

					}
					catch (Exception e)
					{
						Console.WriteLine(e.ToString());
						throw;
					}
				}
			}
		}
	}
}
