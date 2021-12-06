using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Api.Service.Extensions;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Api.Service.Controllers
{
	[ApiController]
	public class SocketController : ControllerBase
	{
		
		private TransactionQueue _queue;
		private ClientList _clients;

		public SocketController(TransactionQueue queue, ClientList clients)
		{
			_queue = queue;
			_clients = clients;
		}

		/// <summary>
		/// This method will register subscribed clients and add them to a list
		/// </summary>
		/// <returns></returns>
		[HttpGet("/socket/subscribe")]
		public async Task RegisterClient()
		{
			var buffer = new byte[1024];
			if (HttpContext.WebSockets.IsWebSocketRequest)
			{
				WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
				WebSocketReceiveResult res = null;
				string message;

				(message, res) = await webSocket.ReceiveAsync(buffer);
				_clients.Add(webSocket);
				await webSocket.SendAsync($"{message} ok");

				while (true)
				{
					(message, res) = await webSocket.ReceiveAsync(buffer);
					if (res.CloseStatus.HasValue)
					{
						await webSocket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
						_clients.Remove(webSocket);
						break;
					}
				}

			}
			else
			{
				HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			}
		}

	}
}
