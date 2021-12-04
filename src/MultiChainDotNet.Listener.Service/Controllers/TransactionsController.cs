using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiChainDotNet.Listener.Service.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TransactionsController : ControllerBase
	{
		ITransactionStore _repo;

		public TransactionsController(ITransactionStore repo)
		{
			_repo = repo;
		}

		[HttpGet("/RawTransactions/Id/{id}")]
		public Task<TransactionWithId> GetRawTransactionById(int id)
		{
			return Task.FromResult(_repo.GetById(id));
		}

		[HttpGet("/RawTransactions/Txid/{txid}")]
		public Task<TransactionWithId> GetRawTransactionByTxid(string txid)
		{
			return Task.FromResult(_repo.GetByTxid(txid));
		}


		[HttpGet("/RawTransactions/Last")]
		public Task<TransactionWithId> GetLastTransaction()
		{
			return Task.FromResult(_repo.GetLast());
		}


		[HttpGet("/RawTransactions")]
		public Task<IList<TransactionWithId>> ListRawTransactions()
		{
			return Task.FromResult(_repo.ListAll());
		}

		[HttpPost("/RawTransactions")]
		public async Task AddRawTransaction(DecodeRawTransactionResult content)
		{
			await _repo.Insert(content);
		}

		[HttpDelete("/RawTransactions")]
		public Task DeleteRawTransactions()
		{
			_repo.DeleteAll();
			return Task.CompletedTask;
		}


	}
}
