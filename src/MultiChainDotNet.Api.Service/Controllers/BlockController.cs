using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
		private ILogger<BlockController> _logger;

		public BlockController(ILogger<BlockController> logger,IHubContext<TransactionHub> transactionHub)
		{
			_transactionHub = transactionHub;
			_logger = logger;
		}

		public class BlockNotifyResult
		{
			[JsonProperty("block")]
			public string Block { get; set; }
		}

		[HttpPost()]
		public async Task Post(BlockNotifyResult block)
		{
			_logger.LogDebug($"McWebSocket: Received blockhash {block.Block}");
			await _transactionHub.Clients.All.SendAsync("Block", JsonConvert.SerializeObject(block));
		}

	}
}
