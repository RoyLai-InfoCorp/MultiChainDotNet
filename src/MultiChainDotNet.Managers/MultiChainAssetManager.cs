// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Managers
{
	public class MultiChainAssetManager : IMultiChainAssetManager
	{
		private readonly ILogger _logger;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;
		MultiChainAssetCommand _assetCmd;
		MultiChainTransactionCommand _txnCmd;

		public MultiChainAssetManager(ILogger<MultiChainAssetManager> logger,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig)
		{
			_mcConfig = mcConfig;
			_assetCmd = cmdFactory.CreateCommand<MultiChainAssetCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_logger = logger;
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainAssetManager(ILogger<MultiChainAssetManager> logger,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_mcConfig = mcConfig;
			_assetCmd = cmdFactory.CreateCommand<MultiChainAssetCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_logger = logger;
			_defaultSigner = signer;
		}

		public async Task<bool> IsExist(string assetName)
		{
			try
			{
				await SubscribeAsync(assetName);
			}
			catch (Exception e)
			{
				if (!e.IsMultiChainException(MultiChainErrorCode.RPC_ENTITY_NOT_FOUND))
				{
					_logger.LogWarning(e.ToString());
					throw;
				}
				return false;
			}
			return true;
		}

		public string Pay(string toAddress, UInt64 units, object data = null)
		{
			return Pay(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, units, data);
		}
		public string Pay(SignerBase signer, string fromAddress, string toAddress, UInt64 units, object data = null)
		{
			_logger.LogDebug($"Executing PayAsync");
			double qty = units / _mcConfig.Multiple;
			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.Pay(qty)
					.With()
						.DeclareJson(data)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;
				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}


		public string PayAnnotate(string toAddress, UInt64 units, object annotation)
		{
			return PayAnnotate(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, units, annotation);
		}
		public string PayAnnotate(SignerBase signer, string fromAddress, string toAddress, UInt64 units, object annotation)
		{
			_logger.LogDebug($"Executing PayAsync");

			double qty = units / _mcConfig.Multiple;
			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.Pay(qty)
						.AnnotateJson(annotation)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;
				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}


		private UInt64 GetAssetMultiple(string assetName)
		{
			UInt64 multiple = Task.Run(async () =>
			{
				await _assetCmd.SubscribeAsync(assetName);
				var assetInfo = await _assetCmd.GetAssetInfoAsync(assetName);
				if (assetInfo.IsError)
					throw assetInfo.Exception;
				return assetInfo.Result.Multiple;
			}).GetAwaiter().GetResult();
			return multiple;
		}

		public string SendAsset(string toAddress, string assetName, UInt64 units, object data = null)
		{
			return SendAsset(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, data);
		}
		public string SendAsset(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, object data = null)
		{
			_logger.LogDebug($"Executing SendAssetAsync");

			try
			{
				// Must subscribe and get the multiple before running
				UInt64 multiple = GetAssetMultiple(assetName);
				double qty = units / multiple;

				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.SendAsset(assetName, qty)
					.With()
						.DeclareJson(data)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;
				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}

		}


		public Task<string> SendAnnotateAssetAsync(string toAddress, string assetName, UInt64 units, object annotation)
		{
			return SendAnnotateAssetAsync(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, annotation);
		}
		public async Task<string> SendAnnotateAssetAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, object annotation)
		{
			_logger.LogDebug($"Executing SendAssetAsync");
			Exception ex_ = new Exception();
			try
			{

				// Must subscribe and get the multiple before running
				UInt64 multiple = GetAssetMultiple(assetName);
				double qty = units / multiple;

				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.SendAsset(assetName, qty)
						.AnnotateJson(annotation)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;

				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				if (!ex.IsMultiChainException(MultiChainErrorCode.RPC_WALLET_INSUFFICIENT_FUNDS))
					throw;
				ex_ = ex;
			}

			// Exception message ambiguous. Determine if its wallet has insufficient asset or insufficient native currency
			try
			{
				var balance = await GetAssetBalanceByAddressAsync(fromAddress, assetName);
				if (balance.Raw < units)
					throw new MultiChainException(MultiChainErrorCode.RPC_WALLET_INSUFFICIENT_ASSET);
				throw ex_;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}

		}


		public string Issue(string toAddress, string assetName, UInt64 units, bool canIssueMore = true, object data = null)
		{
			return Issue(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, canIssueMore, data);
		}
		public string Issue(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, bool canIssueMore = true, object data = null)
		{
			_logger.LogDebug($"Executing IssueAsync");

			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.IssueAsset(units)
					.With()
						.IssueDetails(assetName, 1, canIssueMore)
						.DeclareJson(data)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;

				Task.Run(async () =>
				{
					await SubscribeAsync(assetName);
				}).GetAwaiter().GetResult();

				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}


		public string IssueAnnotate(string toAddress, string assetName, UInt64 units, bool canIssueMore, object annotation)
		{
			return IssueAnnotate(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, canIssueMore, annotation);
		}
		public string IssueAnnotate(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, bool canIssueMore, object annotation)
		{
			_logger.LogDebug($"Executing IssueAsync");

			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.IssueAsset(units)
						.AnnotateJson(annotation)
					.With()
						.IssueDetails(assetName, 1, canIssueMore)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;

				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}


		public string IssueMore(string toAddress, string assetName, UInt64 units, object data = null)
		{
			return IssueMore(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, data);
		}
		public string IssueMore(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, object data = null)
		{
			_logger.LogDebug($"Executing IssueMoreAsync");

			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.IssueMoreAsset(assetName, units)
					.With()
						.DeclareJson(data)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;

				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public string IssueMoreAnnotated(string toAddress, string assetName, UInt64 units, object annotation)
		{
			return IssueMoreAnnotated(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, annotation);
		}
		public string IssueMoreAnnotated(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, object annotation)
		{
			_logger.LogDebug($"Executing IssueMoreAnnotatedAsync");

			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.IssueMoreAsset(assetName, units)
						.AnnotateJson(annotation)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;

				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public async Task<GetAssetInfoResult> GetAssetInfoAsync(string assetName)
		{
			_logger.LogDebug($"Executing GetAssetInfoAsync");

			if (String.IsNullOrEmpty(assetName))
				return null;
			var result = await _assetCmd.GetAssetInfoAsync(assetName);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}

		public async Task<GetAddressBalancesResult> GetAssetBalanceByAddressAsync(string address, string assetName = null)
		{
			_logger.LogDebug($"Executing GetAssetBalanceByAddressAsync");

			var result = await _assetCmd.GetAddressBalancesAsync(address);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}

			try
			{
				if (String.IsNullOrEmpty(assetName))
				{
					var nativeCurrency = result.Result.FirstOrDefault(x => String.IsNullOrEmpty(x.Name));
					if (nativeCurrency is null)
						return null;
					return nativeCurrency;
				}

				GetAddressBalancesResult single = result.Result.FirstOrDefault(x => x.Name == assetName);
				if (single is null)
				{
					return new GetAddressBalancesResult
					{
						Qty = 0,
						AssetRef = "",
						Name = assetName,
						Raw = 0
					};
				}

				var assetInfo = await GetAssetInfoAsync(assetName);
				single.Raw = Convert.ToUInt64(single.Qty * assetInfo.Multiple);
				return single;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public async Task<List<GetAddressBalancesResult>> ListAssetBalancesByAddressAsync(string address)
		{
			_logger.LogDebug($"Executing ListAssetBalancesByAddressAsync");

			var assetsResult = await _assetCmd.GetAddressBalancesAsync(address);
			if (assetsResult.IsError)
			{
				_logger.LogWarning(assetsResult.Exception.ToString());
				throw assetsResult.Exception;
			}
			try
			{
				foreach (GetAddressBalancesResult single in assetsResult.Result)
				{
					if (!String.IsNullOrEmpty(single.Name))
					{
						var assetInfo = await GetAssetInfoAsync(single.Name);
						single.Raw = Convert.ToUInt64(single.Qty * assetInfo.Multiple);
					}
				}
				return assetsResult.Result;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public async Task<List<ListAssetsResult>> ListAssetsAsync(string assetName = "*", bool verbose = false)
		{
			_logger.LogDebug($"Executing ListAssetsAsync");

			var result = await _assetCmd.ListAssetsAsync(assetName, verbose);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}
		public async Task<List<AssetTransactionsResult>> ListAssetTransactionsAsync(string assetName)
		{
			_logger.LogDebug($"Executing ListAssetTransactionsAsync");

			var result = await _assetCmd.ListAssetTransactionsAsync(assetName);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}

		public async Task SubscribeAsync(string assetName)
		{
			_logger.LogDebug($"Executing SubscribeAsync");
			var result = await _assetCmd.SubscribeAsync(assetName);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
		}

	}
}
