using LiteDB;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Listener.Service.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace MultiChainDotNet.Listener.Service
{
	public class PersistentStore : ITransactionStore
	{
		private string _db = "multichain-listener.db";
		ConcurrentQueue<DecodeRawTransactionResult> _transactionList = new ConcurrentQueue<DecodeRawTransactionResult>();

		public PersistentStore()
		{
			BsonMapper.Global.RegisterType<object>
		   (
			   serialize: (input) =>
			   {
				   return JsonConvert.SerializeObject(input);
			   },
			   deserialize: (output) =>
			   {
				   return JsonConvert.DeserializeObject<object>(output.AsString);
			   }
		   );
			File.Delete("multichain-listener.db");
		}

		public async Task Insert(DecodeRawTransactionResult result)
		{
			_transactionList.Enqueue(result);
			using (var db = new LiteDatabase(_db))
			{
				try
				{
					var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
					var lastId = 0;
					if (col.Count() > 0)
						lastId = col.FindAll().Max(x => x.Id);

					var success = _transactionList.TryDequeue(out var raw);
					if (!success)
						return;

					var newTxn = new TransactionWithId { Id = lastId + 1, Raw = raw };
					col.Insert(newTxn);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
				finally
				{
				}
			}
		}

		public IList<TransactionWithId> ListAll()
		{
			using (var db = new LiteDatabase(_db))
			{
				var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
				return col.FindAll().ToList();
			}

		}

		public TransactionWithId GetLast()
		{
			using (var db = new LiteDatabase(_db))
			{
				var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
				var list = col.FindAll();
				if (list is null || list.ToList().Count == 0)
					return null;

				var lastId = list.Max(x => x.Id);
				return list.SingleOrDefault(x => x.Id == lastId);
			}
		}

		public TransactionWithId GetById(int id)
		{
			using (var db = new LiteDatabase(_db))
			{
				var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
				return col.FindOne(x => x.Id == id);
			}
		}

		public TransactionWithId GetByTxid(string txid)
		{
			using (var db = new LiteDatabase(_db))
			{
				var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
				return col.FindOne(x => x.Raw.Txid == txid);
			}
		}

		public void DeleteAll()
		{
			using (var db = new LiteDatabase(_db))
			{
				var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
				col.DeleteAll();
			}
		}

	}

}
