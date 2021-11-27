using MultiChainDotNet.Core.MultiChainTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TransactionWithId
{
    public int Id { get; set; }
	public DecodeRawTransactionResult Raw { get; set; }

	////------------------

	//private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);

	//public static LiteDbBase<TransactionWithId> _db = new LiteDbBase<TransactionWithId>("multichain-listener.db");

	//public async static Task Insert(DecodeRawTransactionResult result)
	//{

	//	await _semaphore.WaitAsync();
	//	try
	//	{
	//		var lastId = 0;
	//		var list = await _db.ListAll();
	//		if (list is { } && list.Count > 0)
	//			lastId = list.Max(x => x.Id);
	//		var newTxn = new TransactionWithId { Id = lastId + 1, Raw = result };
	//		await _db.Insert(newTxn);
	//	}
	//	catch (Exception ex)
	//	{
	//		Console.WriteLine(ex.ToString());
	//	}
	//	finally
	//	{
	//		_semaphore.Release();
	//	}
	//}

	//public async static Task<IList<TransactionWithId>> ListAll()
	//{
	//	return await _db.ListAll();
	//}

	//public async static Task<TransactionWithId> GetLast()
	//{
	//	return (await _db.ListAll())?.MaxBy(x => x.Id);
	//}

	//public async static Task<TransactionWithId> GetById(int id)
	//{
	//	return (await _db.ListAll())?.SingleOrDefault(x => x.Id == id);
	//}

	//public async static Task<TransactionWithId> GetByTxid(string txid)
	//{
	//	return (await _db.ListAll())?.SingleOrDefault(x => x.Raw.Txid == txid);
	//}

	//public async static Task DeleteAll()
	//{
	//	await _db.DeleteAll();
	//}

}
