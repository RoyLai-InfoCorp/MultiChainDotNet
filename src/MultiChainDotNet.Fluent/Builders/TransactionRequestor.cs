using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders
{
    public class TransactionRequestor
    {
		private TxnFromBuilder _fromBuilder;
		private List<TxnToBuilder> _toBuilders = new List<TxnToBuilder>();
		private TxnWithBuilder _withBuilder = new TxnWithBuilder();

		public TransactionRequestor()
		{
		}

		public TransactionRequestor From(string address)
		{
			_fromBuilder = new TxnFromBuilder(address);
			return this;
		}

		public TxnToBuilder To(string address)
		{
			var toBuilder = new TxnToBuilder(address);
			_toBuilders.Add(toBuilder);
			return toBuilder;
		}

		public TxnWithBuilder With()
		{
			_withBuilder = new TxnWithBuilder();
			return _withBuilder;
		}

		public string Request(MultiChainTransactionCommand txnCmd)
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

	}
}
