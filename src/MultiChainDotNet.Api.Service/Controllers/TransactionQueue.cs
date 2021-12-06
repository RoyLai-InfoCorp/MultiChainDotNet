using MultiChainDotNet.Core.MultiChainTransaction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service.Controllers
{

	/// <summary>
	/// This queue will queue up new transactions notifications from the MultiChain wallet and to be consumed by the socket clients
	/// </summary>
    public class TransactionQueue
    {
		ConcurrentQueue<DecodeRawTransactionResult> _queue = new ConcurrentQueue<DecodeRawTransactionResult>();
		public int MaxSize { get; set; } = 100;

		public new void Enqueue(DecodeRawTransactionResult decodeRawTransactionResult)
		{
			_queue.Enqueue(decodeRawTransactionResult);
			while (_queue.Count > MaxSize)
				_queue.TryDequeue(out var item);
		}

		public DecodeRawTransactionResult Dequeue()
		{
			bool result = _queue.TryDequeue(out var item);
			if (result)
				return item;
			return null;
		}

		public IList<DecodeRawTransactionResult> ToList()
		{
			return _queue.ToList();
		}

		public int Count => _queue.Count;
    }
}
