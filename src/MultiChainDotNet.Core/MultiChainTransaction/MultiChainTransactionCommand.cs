// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

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
			if (txid is null)
				throw new ArgumentNullException(nameof(txid));

			return await JsonRpcRequestAsync<GetTxOutResult>("gettxout", txid, vout);
		}

		public async Task<MultiChainResult<DecodeRawTransactionResult>> DecodeRawTransactionAsync(string hexdata)
		{
			if (hexdata is null)
				throw new ArgumentNullException(nameof(hexdata));

			return await JsonRpcRequestAsync<DecodeRawTransactionResult>("decoderawtransaction", hexdata);
		}

		public async Task<MultiChainResult<string>> SendRawTransactionAsync(string txHex)
		{
			if (txHex is null)
				throw new ArgumentNullException(nameof(txHex));

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
			if (txid is null)
				throw new ArgumentNullException(nameof(txid));

			return await JsonRpcRequestAsync<string>("getrawtransaction", txid);
		}

		public async Task<MultiChainResult<List<ListAddressTransactionResult>>> ListAddressTransactions(string address, int count=10, int skip=0, bool verbose=false)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			return await JsonRpcRequestAsync<List<ListAddressTransactionResult>>("listaddresstransactions", address, count, skip, verbose);
		}

		/// <summary>		
		///  start: (number, optional, default=-count - last) Start from specific transaction, 0 based, if negative - from the end
		///  local-ordering: (boolean, optional, default=false) If true, transactions appear in the order they were processed by the wallet,
		///  if false - in the order they appear in blockchain
		/// </summary>
		/// <param name="assetName"></param>
		/// <param name="count"></param>
		/// <param name="start"></param>
		/// <param name="verbose"></param>
		/// <returns></returns>
		public async Task<MultiChainResult<List<ListAssetTransactionResult>>> ListAssetTransactions(string assetName, bool verbose=false, int count = 10, int start = -10, bool localOrdering = false)
		{
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));

			return await JsonRpcRequestAsync<List<ListAssetTransactionResult>>("listassettransactions", assetName, verbose, count, start, localOrdering);
		}

		public async Task<MultiChainResult<string>> CreateRawTransactionAsync(List<TxIdVoutStruct> txidVout, object to, object with = null)
		{
			if (txidVout is null)
				throw new ArgumentNullException(nameof(txidVout));


			if (with is null)
				return await JsonRpcRequestAsync<string>("createrawtransaction",txidVout,to);

			return await JsonRpcRequestAsync<string>("createrawtransaction",txidVout,to,with);
		}

		public async Task<MultiChainResult<string>> CreateRawTransactionAsync(string txid, int vout, object to, object with = null)
		{
			if (txid is null)
				throw new ArgumentNullException(nameof(txid));


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
			if (addresses is null)
				throw new ArgumentNullException(nameof(addresses));

			return await ListUnspentAsync(new string[] { addresses });
		}

		public async Task<MultiChainResult<List<ListUnspentResult>>> ListUnspentAsync(string[] addresses)
		{
			if (addresses is null)
				throw new ArgumentNullException(nameof(addresses));

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


		public async Task<MultiChainResult<TxIdVoutStruct>> PrepareLockUnspentFromAsync(string from, string assetName, Double qty)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));

			return await JsonRpcRequestAsync<TxIdVoutStruct>("preparelockunspentfrom", from, 
				new Dictionary<string, Double> { {assetName,qty } });
		}

		public async Task<MultiChainResult<string>> LockUnspentAsync(bool unlock, string txid, UInt16 vout)
		{
			return await JsonRpcRequestAsync<string>("lockunspent", unlock,
				new object[] { new { txid = txid, vout = vout } });
		}


	}
}
