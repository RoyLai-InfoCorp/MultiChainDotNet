using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace Client3
{
	public static class WebSocketExtensions
	{

		static string DecodeWebSocketResult(byte[] buffer, WebSocketReceiveResult received)
		{
			return (new ArraySegment<byte>(buffer, 0, received.Count)).ToArray().Bytes2UTF8();
		}

		public static async Task<(string message, WebSocketReceiveResult result)> ClientReceiveAsync(this ClientWebSocket ws, byte[] buffer, CancellationToken cancellationToken = default(CancellationToken))
		{
			WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
			string message = DecodeWebSocketResult(buffer, result);
			return (message, result);
		}

		public static async Task SendAsync(this ClientWebSocket ws, string message, CancellationToken cancellationToken = default(CancellationToken))
		{
			await ws.SendAsync(new ArraySegment<byte>(message.UTF82Bytes()), WebSocketMessageType.Text, true, cancellationToken);
		}

	}
}
