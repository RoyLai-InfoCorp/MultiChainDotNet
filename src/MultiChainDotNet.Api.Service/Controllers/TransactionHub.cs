using Microsoft.AspNetCore.SignalR;
using MultiChainDotNet.Core.MultiChainTransaction;
using System.Threading.Tasks;
using static MultiChainDotNet.Api.Service.Controllers.BlockController;

namespace MultiChainDotNet.Api.Service.Controllers
{
	public class TransactionHub : Hub
	{
		public async Task Publish(DecodeRawTransactionResult raw)
		{
			await Clients.All.SendAsync("Publish", raw);
		}

		public async Task Block(BlockNotifyResult block)
		{
			await Clients.All.SendAsync("Block", block);
		}

	}
}
