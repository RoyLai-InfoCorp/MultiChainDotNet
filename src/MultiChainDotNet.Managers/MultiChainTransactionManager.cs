// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Signers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainTransactionManager : IMultiChainTransactionManager
	{
		private readonly ILogger _logger;
		private readonly IMultiChainCommandFactory _commandFactory;
		private MultiChainConfiguration _mcConfig;

		public MultiChainTransactionManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory commandFactory,
			MultiChainConfiguration mcConfig)
		{
			_commandFactory = commandFactory;
			_logger = loggerFactory.CreateLogger<MultiChainTransactionManager>();
			_mcConfig = mcConfig;
		}

		public MultiChainTransactionManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory commandFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_commandFactory = commandFactory;
			_logger = loggerFactory.CreateLogger<MultiChainTransactionManager>();
		}


		public async Task<MultiChainResult<string>> GetAnnotationAsync(string assetName, string txid)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			var txnResult = await txnCmd.GetRawTransaction(txid);
			if (txnResult.IsError)
				return new MultiChainResult<string>(txnResult.Exception);

			var decodedResult = await txnCmd.DecodeRawTransactionAsync(txnResult.Result);
			if (decodedResult.IsError)
				return new MultiChainResult<string>(decodedResult.Exception);

			var firstData = decodedResult.Result.Vout
				.Where(x => x.Assets is { } && x.Assets.Any(y => y.Name == assetName) && x.Data is { })?
				.FirstOrDefault()?.Data?[0];
			if (firstData is { })
			{
				var json = JToken.FromObject(firstData);
				var found = json.SelectToken("json");
				if (found is { })
					return new MultiChainResult<string>(found.ToString(Formatting.None));
			}
			return new MultiChainResult<string>();
		}

		public async Task<MultiChainResult<string>> GetDeclarationAsync(string txid)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			var txnResult = await txnCmd.GetRawTransaction(txid);
			if (txnResult.IsError)
				return new MultiChainResult<string>(txnResult.Exception);

			var decodedResult = await txnCmd.DecodeRawTransactionAsync(txnResult.Result);
			var firstData = decodedResult.Result.Vout.Where(x => x.Assets is null && x.Data is { }).FirstOrDefault()?.Data?[0];
			if (firstData is { })
			{
				var json = JToken.FromObject(firstData);
				var found = json.SelectToken("json");
				if (found is { })
					return new MultiChainResult<string>(found.ToString(Formatting.None));
			}
			return new MultiChainResult<string>();
		}

		public async Task<MultiChainResult<DecodeRawTransactionResult>> DecodeRawTransactionAsync(string txid)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			var txnResult = await txnCmd.GetRawTransaction(txid);
			if (txnResult.IsError)
				return new MultiChainResult<DecodeRawTransactionResult>(txnResult.Exception);

			return await txnCmd.DecodeRawTransactionAsync(txnResult.Result);
		}

		public async Task<MultiChainResult<List<ListAddressTransactionResult>>> ListTransactionsByAddress(string address, int count, int skip, bool verbose)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			return await txnCmd.ListAddressTransactions(address, count, skip, verbose);
		}

		public async Task<MultiChainResult<List<ListAssetTransactionResult>>> ListTransactionsByAsset(string assetName, bool verbose = false, int count = 10, int start = -10, bool localOrdering = false)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			return await txnCmd.ListAssetTransactions(assetName, verbose, count, start, localOrdering);
		}

		public async Task<MultiChainResult<List<ListAssetTransactionResult>>> ListAllTransactionsByAsset(string assetName)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			int page = 0;
			int count = 100;
			bool empty = false;
			List<ListAssetTransactionResult> list = new List<ListAssetTransactionResult>();
			while (!empty)
			{
				var buffer = await txnCmd.ListAssetTransactions(assetName, false, count, page * count, false);
				if (buffer.IsError)
					throw buffer.Exception;

				if (buffer.Result.Count == 0)
					empty = true;
				else
				{
					foreach (var item in buffer.Result)
						list.Add(item);
				}
				page++;
			}

			return new MultiChainResult<List<ListAssetTransactionResult>>(list);
		}

		public async Task<MultiChainResult<List<ListAddressTransactionResult>>> ListAllTransactionsByAddress(string address, string assetName = null)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			int page = 0;
			int count = 100;
			bool empty = false;
			List<ListAddressTransactionResult> list = new List<ListAddressTransactionResult>();
			while (!empty)
			{
				var buffer = await txnCmd.ListAddressTransactions(address, count, page * count, false);
				if (buffer.IsError)
					throw buffer.Exception;

				if (buffer.Result.Count == 0)
					empty = true;
				else
				{
					foreach (var item in buffer.Result)
						list.Add(item);
				}
				page++;
			}

			if (!string.IsNullOrEmpty(assetName))
			{
				var filteredList = list.Where(x => x.Balance.Assets.Any(x => x.Name == assetName)).ToList();
				return new MultiChainResult<List<ListAddressTransactionResult>>(filteredList);
			}

			return new MultiChainResult<List<ListAddressTransactionResult>>(list);
		}



		private (List<TxIdVoutStruct> SelectedUnspents, Dictionary<string, Double> ReturnUnspents) SelectUnspent(List<ListUnspentResult> unspents, UInt64 requiredPayment, string requiredAssetName, Double requiredAssetQty, UInt64 fees = 1000)
		{
			UInt64 totalPayment = 0;
			Double totalAssetQty = 0;
			List<TxIdVoutStruct> list = new List<TxIdVoutStruct>();
			var returnAssets = new Dictionary<string, Double>();

			foreach (var unspent in unspents)
			{
				var unspentNeeded = false;
				if (totalAssetQty < requiredAssetQty)
				{
					if (unspent.Assets.Any(x => x.Name == requiredAssetName))
					{
						unspentNeeded = true;
						foreach (var asset in unspent.Assets)
						{
							if (asset.Name == requiredAssetName)
								totalAssetQty += asset.Qty;
							else
							{
								if (returnAssets.ContainsKey(asset.Name))
									returnAssets[asset.Name] = returnAssets[asset.Name] + asset.Qty;
								else
									returnAssets[asset.Name] = asset.Qty;
							}
						}
					}
				}
				if (totalPayment < (requiredPayment + fees) && unspent.Amount > 0)
				{
					totalPayment += unspent.Amount;
					unspentNeeded = true;
				}
				if (unspentNeeded)
					list.Add(new TxIdVoutStruct { TxId = unspent.TxId, Vout = unspent.Vout });
			}

			if (totalPayment < requiredPayment + fees)
				throw new Exception("Insufficient balance for fees");
			if (totalAssetQty < requiredAssetQty)
				throw new Exception("Insufficient balance for assets");

			if (requiredAssetName is { })
				returnAssets.Add(requiredAssetName, totalAssetQty - requiredAssetQty);

			returnAssets.Add("", totalPayment - fees);

			return (list, returnAssets);
		}

		private async Task<Dictionary<string, double>> ListUnspentBalances(string address)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			Dictionary<string, double> unspentBal = new Dictionary<string, double>();
			var unspents = await txnCmd.ListUnspentAsync(address);
			unspentBal[""] = 0;
			foreach (var unspent in unspents.Result)
			{
				foreach (var asset in unspent.Assets)
					unspentBal[asset.Name] = 0;
			}

			foreach (var unspent in unspents.Result)
			{
				unspentBal[""] += unspent.Amount;
				foreach (var asset in unspent.Assets)
					unspentBal[asset.Name] += asset.Qty;
			}
			return unspentBal;
		}

	}
}
