using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Listener.Service.Controllers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Listener.Service
{
	public static class Extensions
	{
		public static void Clear<T>(this BlockingCollection<T> blockingCollection)
		{
			if (blockingCollection == null)
			{
				throw new ArgumentNullException("blockingCollection");
			}

			while (blockingCollection.Count > 0)
			{
				T item;
				blockingCollection.TryTake(out item);
			}
		}

	}

	public class InMemoryStore : ITransactionStore
	{
		BlockingCollection<TransactionWithId> _transactionList = new BlockingCollection<TransactionWithId>();

		public void DeleteAll()
		{
			_transactionList.Clear();
		}

		public TransactionWithId GetById(int id)
		{
			return _transactionList.SingleOrDefault(x => x.Id == id);
		}

		public TransactionWithId GetByTxid(string txid)
		{
			return _transactionList.SingleOrDefault(x => x.Raw.Txid == txid);
		}

		public TransactionWithId GetLast()
		{
			return _transactionList.Last();
		}

		public Task Insert(DecodeRawTransactionResult result)
		{
			_transactionList.Add(new TransactionWithId { Id = _transactionList.Count + 1, Raw = result });
			return Task.CompletedTask;
		}

		public IList<TransactionWithId> ListAll()
		{
			return _transactionList.ToList();
		}
	}
}
