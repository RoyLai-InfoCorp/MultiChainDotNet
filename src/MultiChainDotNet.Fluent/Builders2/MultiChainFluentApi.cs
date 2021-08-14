using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Builders2.MultiStageMultiSig;
using MultiChainDotNet.Fluent.Builders2.SubmitMultiSig;
using MultiChainDotNet.Fluent.Signers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2
{

	public class MultiChainFluentApi: IFromBuilder, IToBuilder, IRequestBuilder, IUseMultiStageMultiSig
	{
		ILogger<MultiChainFluentApi> _logger;
		internal TxnFromBuilder _fromBuilder;
		internal List<TxnToBuilder> _toBuilders = new List<TxnToBuilder>();
		internal TxnWithBuilder _withBuilder = null;
		internal TransactionSender _txnSender = null;
		internal MultiSigSender _multisigSender = null;
		internal MultiStageMultiSigBuilder _multiStageMultiSigBuilder = null;
		internal MultiSigSubmitBuilder _multiSigSubmitBuilder = null;
		internal string _raw;

		public MultiChainFluentApi(ILogger<MultiChainFluentApi> logger)
		{
			_withBuilder = new TxnWithBuilder(this);
			_logger = logger;
		}

		public IToBuilder From(string address)
		{
			_fromBuilder = new TxnFromBuilder(address);
			return this;
		}

		public IToActionBuilder To(string address)
		{
			var toBuilder = new TxnToBuilder(address, this);
			_toBuilders.Add(toBuilder);
			return toBuilder;
		}

		public IWithActionBuilder With()
		{
			return _withBuilder;
		}

		public string CreateRawTransaction(MultiChainTransactionCommand txnCmd)
		{
			var (fromAddress, tos, with) = CreateRawSendFrom();
			string request = Task.Run(async () =>
			{
				var result = await txnCmd.CreateRawSendFromAsync(fromAddress, tos, with);
				if (result.IsError)
					throw result.Exception;
				return result.Result;
			}).GetAwaiter().GetResult();

			return request;
		}

		public IAddSignerBuilder CreateTransaction(MultiChainTransactionCommand txnCmd)
		{
			var request = CreateRawTransaction(txnCmd);
			_txnSender = new TransactionSender(_logger, txnCmd, request);
			return _txnSender;
		}

		public IAddMultiSigSenderBuilder CreateMultiSigTransaction(MultiChainTransactionCommand txnCmd)
		{
			var request = CreateRawTransaction(txnCmd);
			_multisigSender = new MultiSigSender(_logger, txnCmd, request);
			return _multisigSender;
		}


		private (string From, Dictionary<string, object> To, List<object> With) CreateRawSendFrom()
		{
			var fromAddress = _fromBuilder.Build();
			var tos = new Dictionary<string, object>();
			foreach (var toBuilder in _toBuilders)
			{
				var (addressTo, itemsTo) = toBuilder.Build();
				tos[addressTo] = itemsTo;
			}
			var with = _withBuilder.Build();
			return (fromAddress, tos, with);
		}

		public string Describe()
		{
			var (fromAddress, tos, with) = CreateRawSendFrom();
			return $"createrawsendfrom {fromAddress} '{JsonConvert.SerializeObject(tos)}' '{JsonConvert.SerializeObject(with)}'";
		}

		public MultiStageMultiSig.IAddMultiSigRawTransaction UseMultiStageMultiSig()
		{
			_multiStageMultiSigBuilder = new MultiStageMultiSigBuilder(_logger);
			return _multiStageMultiSigBuilder;
		}

		public SubmitMultiSig.IAddMultiSigRawTransaction UseMultiSigSubmit()
		{
			_multiSigSubmitBuilder = new MultiSigSubmitBuilder(_logger);
			return _multiSigSubmitBuilder;
		}

	}
}
