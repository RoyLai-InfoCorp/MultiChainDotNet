using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service.Controllers
{
    public class ClientList
    {
		private BlockingCollection<WebSocket> _clients = new BlockingCollection<WebSocket>();
		public BlockingCollection<WebSocket> Clients => _clients;

		public int Add(WebSocket socket)
		{
			_clients.Add(socket);
			return _clients.Count;
		}
		public int Remove(WebSocket socket)
		{
			_clients.TryTake(out socket);
			return _clients.Count;
		}

	}
}
