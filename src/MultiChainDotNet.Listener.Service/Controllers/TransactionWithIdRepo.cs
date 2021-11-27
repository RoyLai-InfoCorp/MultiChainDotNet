using LiteDB;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TransactionWithIdRepo
{
	private string _db = "multichain-listener.db";
	private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

	public TransactionWithIdRepo()
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
		using (var db = new LiteDatabase(_db))
		{
			var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
			col.DeleteAll();
		}
	}

	public async Task Insert(DecodeRawTransactionResult result)
	{

		await _semaphore.WaitAsync();
		using (var db = new LiteDatabase(_db))
		{
			try
			{
				var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
				var lastId = 0;
				if (col.Count() > 0)
					lastId = col.FindAll().Max(x => x.Id);
				var newTxn = new TransactionWithId { Id = lastId + 1, Raw = result };
				col.Insert(newTxn);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				_semaphore.Release();
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
			return col.FindAll()?.MaxBy(x => x.Id);
		}
	}

	public TransactionWithId GetById(int id)
	{
		using (var db = new LiteDatabase(_db))
		{
			var col = db.GetCollection<TransactionWithId>(nameof(TransactionWithId));
			return col.FindOne(x => x.Id==id);
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
