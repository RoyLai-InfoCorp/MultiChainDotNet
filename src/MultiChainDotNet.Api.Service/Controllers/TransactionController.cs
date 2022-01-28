using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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

		public class WalletNotifyResult
		{
			[JsonProperty("txn")]
			public DecodeRawTransactionResult Transaction { get; set; }

			[JsonProperty("height")]
			public int Height { get; set; }
		}

		public TransactionController(IHubContext<TransactionHub> transactionHub)
		{
			_transactionHub = transactionHub;
		}

		[HttpPost()]
		public async Task Post(WalletNotifyResult transaction)
		{
			await _transactionHub.Clients.All.SendAsync("Publish", JsonConvert.SerializeObject(transaction));
		}

	}
}
