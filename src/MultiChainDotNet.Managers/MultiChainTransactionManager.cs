using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainTransactionManager : IMultiChainTransactionManager
	{
		private readonly ILogger _logger;
		private readonly IMultiChainCommandFactory _commandFactory;
		public MultiChainTransactionManager(ILogger<MultiChainTransactionManager> logger, IMultiChainCommandFactory commandFactory)
		{
			_commandFactory = commandFactory;
			_logger = logger;
		}

		public async Task<MultiChainResult<string>> GetAnnotationAsync(string assetName, string txid)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			var txnResult = await txnCmd.GetRawTransaction(txid);
			if (txnResult.IsError)
				return new MultiChainResult<string>(txnResult.Exception);

			var decodedResult = await txnCmd.DecodeRawTransactionAsync(txnResult.Result);
			var firstData = decodedResult.Result.Vout.Where(x => x.Assets.Any(y => y.Name == assetName) && x.Data is { }).FirstOrDefault()?.Data?[0];
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




		public async Task<MultiChainResult<List<ListAddressTransactionResult>>> ListTransactionsByAddress(string address)
		{
			var txnCmd = _commandFactory.CreateCommand<MultiChainTransactionCommand>();
			return await txnCmd.ListAddressTransactions(address);
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
