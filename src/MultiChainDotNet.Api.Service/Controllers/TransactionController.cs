using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service.Controllers
{

	/// <summary>
	/// This endpoint is used for receiving new transaction notifications from MultiChain node and broadcast to websocket
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class TransactionController : ControllerBase
	{
		private IHubContext<TransactionHub> _transactionHub;
		private ILogger<TransactionController> _logger;

		public class WalletNotifyResult
		{
			[JsonProperty("txn")]
			public DecodeRawTransactionResult Transaction { get; set; }

			[JsonProperty("height")]
			public int Height { get; set; }
		}

		public TransactionController(ILogger<TransactionController> logger,IHubContext<TransactionHub> transactionHub)
		{
			_transactionHub = transactionHub;
			_logger = logger;
		}

		[HttpPost()]
		public async Task Post(WalletNotifyResult transaction)
		{
			var json = JsonConvert.SerializeObject(transaction);
			_logger.LogDebug($"McWebSocket: Received transaction {json}");
			await _transactionHub.Clients.All.SendAsync("Publish", json);
		}

	}
}
