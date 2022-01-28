using Microsoft.AspNetCore.SignalR;
using MultiChainDotNet.Core.MultiChainTransaction;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service.Controllers
{
	public class TransactionHub : Hub
	{
		public async Task Publish(DecodeRawTransactionResult raw)
		{
			await Clients.All.SendAsync("Publish", raw);
		}

		public async Task Blocks(string blockhash)
		{
			await Clients.All.SendAsync("Blocks", blockhash);
		}

	}
}
