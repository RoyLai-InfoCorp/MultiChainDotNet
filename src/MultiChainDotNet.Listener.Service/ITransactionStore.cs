using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Listener.Service.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Listener.Service
{
	public interface ITransactionStore
	{
		void DeleteAll();
		TransactionWithId GetById(int id);
		TransactionWithId GetByTxid(string txid);
		TransactionWithId GetLast();
		Task Insert(DecodeRawTransactionResult result);
		IList<TransactionWithId> ListAll();
	}
}