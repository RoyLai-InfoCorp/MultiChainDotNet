using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainStream;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
	public class MultiChainTransactionCommand : MultiChainCommandBase
	{
		public MultiChainTransactionCommand(ILogger<MultiChainTransactionCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogTrace($"Initialized MultiChainTransactionCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public MultiChainTransactionCommand(ILogger<MultiChainTransactionCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogTrace($"Initialized MultiChainTransactionCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public async Task<MultiChainResult<GetTxOutResult>> GetTxOutAsync(string txid, int vout)
		{
			return await JsonRpcRequestAsync<GetTxOutResult>("gettxout", txid, vout);
		}

		public async Task<MultiChainResult<DecodeRawTransactionResult>> DecodeRawTransactionAsync(string hexdata)
		{
			return await JsonRpcRequestAsync<DecodeRawTransactionResult>("decoderawtransaction", hexdata);
		}

		public async Task<MultiChainResult<string>> SendRawTransactionAsync(string txHex)
		{
			return await JsonRpcRequestAsync<string>("sendrawtransaction", txHex);
		}

		public string DescribeCreateRawSendFromAsync(string from, object to, object with)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			return $"createrawsendfrom {from} '{JsonConvert.SerializeObject(to)}' '{JsonConvert.SerializeObject(with)}'";
		}

		public async Task<MultiChainResult<string>> CreateRawSendFromAsync(string from, object to, object with = null)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));

			if (with == null)
				with = new object[] { };

			_logger.LogTrace("===>" + DescribeCreateRawSendFromAsync(from, to, with));
			return await JsonRpcRequestAsync<string>("createrawsendfrom", from, to, with);
		}

		public async Task<MultiChainResult<string>> GetRawTransaction(string txid)
		{
			return await JsonRpcRequestAsync<string>("getrawtransaction", txid);
		}

		public async Task<MultiChainResult<List<ListAddressTransactionResult>>> ListAddressTransactions(string address)
		{
			return await JsonRpcRequestAsync<List<ListAddressTransactionResult>>("listaddresstransactions", address);
		}

		public async Task<MultiChainResult<string>> CreateRawTransactionAsync(List<TxIdVoutStruct> txidVout, object to, object with = null)
		{
			if (with is null)
				return await JsonRpcRequestAsync<string>("createrawtransaction",txidVout,to);

			return await JsonRpcRequestAsync<string>("createrawtransaction",txidVout,to,with);
		}

		public async Task<MultiChainResult<string>> CreateRawTransactionAsync(string txid, int vout, object to, object with = null)
		{
			if (with is null)
				return await JsonRpcRequestAsync<string>("createrawtransaction",
				new object[] { new { txid = txid, vout = vout } }, 
				to);

			return await JsonRpcRequestAsync<string>("createrawtransaction", 
				new object[] { new { txid = txid, vout = vout } },  
				to, 
				with);
		}

		public async Task<MultiChainResult<List<ListUnspentResult>>> ListUnspentAsync(string addresses)
		{
			return await ListUnspentAsync(new string[] { addresses });
		}

		public async Task<MultiChainResult<List<ListUnspentResult>>> ListUnspentAsync(string[] addresses)
		{
			return await ListUnspentAsync(1, 9999999, addresses);
		}

		public async Task<MultiChainResult<List<ListUnspentResult>>> ListUnspentAsync(UInt32 minconf = 1, UInt32 maxconf = 9999999, string[] addresses = null)
		{
			if (addresses is { })
				return await JsonRpcRequestAsync<List<ListUnspentResult>>("listunspent", minconf, maxconf, addresses);
			return await JsonRpcRequestAsync<List<ListUnspentResult>>("listunspent", minconf, maxconf);
		}

		public async Task<MultiChainResult<SignRawTransactionResult>> SignRawTransactionAsync(string transaction)
		{
			return await JsonRpcRequestAsync<SignRawTransactionResult>("signrawtransaction", transaction);
		}

		public async Task<MultiChainResult<string>> AppendRawChangeAsync(string transaction, string address)
		{
			return await JsonRpcRequestAsync<string>("appendrawchange", transaction,address);
		}

		public async Task<MultiChainResult<string>> AppendRawTransactionAsync(string transaction, string address)
		{
			return await JsonRpcRequestAsync<string>("appendrawchange", transaction, address);
		}


		public async Task<MultiChainResult<TxIdVoutStruct>> PrepareLockUnspentFromAsync(string addressFrom, string assetName, Double qty)
		{
			return await JsonRpcRequestAsync<TxIdVoutStruct>("preparelockunspentfrom", addressFrom, 
				new Dictionary<string, Double> { {assetName,qty } });
		}

		public async Task<MultiChainResult<string>> LockUnspentAsync(bool unlock, string txid, UInt16 vout)
		{
			return await JsonRpcRequestAsync<string>("lockunspent", unlock,
				new object[] { new { txid = txid, vout = vout } });
		}


	}
}
