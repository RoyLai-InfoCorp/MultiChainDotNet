using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using MultiChainDotNet.Fluent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2.SubmitMultiSig
{
	public class MultiSigSubmitBuilder : MultiSigBase, 
		IAddMultiSigRawTransaction, 
		IAddTransactionCommand, 
		IAddSignatures, 
		ICreateTransaction,
		ISend
	{
		ILogger _logger;
		string _raw;
		MultiChainTransactionCommand _txnCmd;
		List<string[]> _signatures;
		string _signed;


		public MultiSigSubmitBuilder(ILogger logger)
		{
			_logger = logger;
		}


		public IAddTransactionCommand AddMultiSigRawTransaction(string multisigRawTxn)
		{
			_raw = multisigRawTxn;
			return this;
		}

		public IAddSignatures AddTransactionCommand(MultiChainTransactionCommand txnCmd)
		{
			_txnCmd = txnCmd;
			return this;
		}

		public ICreateTransaction AddSignatures(List<string[]> signatures)
		{
			_signatures = signatures;
			return this;
		}

		public ISend CreateTransaction(string redeemScript)
		{
			_signed = MultiSign(_raw, redeemScript, _signatures);
			return this;
		}

		public string Send()
		{
			return Task.Run(async () =>
			{
				var result = await _txnCmd.SendRawTransactionAsync(_signed);
				if (result.IsError)
					throw result.Exception;
				return result.Result;
			}).GetAwaiter().GetResult();
		}
	}
}
