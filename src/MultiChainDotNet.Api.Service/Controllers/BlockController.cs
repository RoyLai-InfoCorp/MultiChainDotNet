using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service.Controllers
{

	/// <summary>
	/// This endpoint is used for receiving new transaction notifications from MultiChain node and broadcast to websocket
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class BlockController : ControllerBase
	{
		private IHubContext<TransactionHub> _transactionHub;

		public BlockController(IHubContext<TransactionHub> transactionHub)
		{
			_transactionHub = transactionHub;
		}

		public class BlockNotifyResult
		{
			[JsonProperty("block")]
			public string Block { get; set; }
		}

		[HttpPost()]
		public async Task Post(BlockNotifyResult block)
		{
			await _transactionHub.Clients.All.SendAsync("Block", JsonConvert.SerializeObject(block));
		}

	}
}
