using MultiChainDotNet.Api.Abstractions;
using MultiChainDotNet.Api.Abstractions.Extensions;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using Refit;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client1
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("Press 1 to send a multichain transaction. Anything else to quit");
			string cmd = "";
			while (cmd.ToLower()!="q")
			{
				cmd = Console.ReadLine();
				switch(cmd)
				{
					case "1":
						try
						{
							var response = await RefitExtensions.For<IJsonRpcApi>("http://localhost:12028/").Execute(new JsonRpcRequest
							{
								Method = "send",
								Params = new object[] {
								"1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN",
								0
							}});
							Console.WriteLine(response.ToString());
						}
						catch (ApiException ex)
						{
							Console.WriteLine(ex.Content);
						}
						break;
					default:
						return;
				}
			}
		}
	}
}
