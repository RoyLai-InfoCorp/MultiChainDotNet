using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Api.Service.Extensions;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service.Controllers
{

	/// <summary>
	/// This endpoint is used for receiving new transaction notifications from MultiChain node
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class TransactionController : ControllerBase
	{
		private TransactionQueue _queue;
		private ClientList _clients;

		public TransactionController(TransactionQueue queue, ClientList clients)
		{
			_queue = queue;
			_clients = clients;
		}

		private async Task Broadcast()
		{
			while (_queue.Count > 0)
			{
				var txn = _queue.Dequeue();
				if (txn is { })
				{
					Parallel.ForEach(_clients.Clients, async (client) =>
					{
						await client.SendAsync(JsonConvert.SerializeObject(txn));
					});
				}
			}
		}

		[HttpPost()]
		public async Task Post(DecodeRawTransactionResult transaction)
		{
			_queue.Enqueue(transaction);
			await Broadcast();
		}

		[HttpGet()]
		public async Task<IList<DecodeRawTransactionResult>> List()
		{
			return _queue.ToList();
		}

	}
}
