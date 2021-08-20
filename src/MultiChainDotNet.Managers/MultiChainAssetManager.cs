﻿using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Builders;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainAssetManager : IMultiChainAssetManager
	{
		//private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;
		MultiChainAssetCommand _assetCmd;
		MultiChainTransactionCommand _txnCmd;

		public MultiChainAssetManager(ILogger<MultiChainAssetManager> logger,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig)
		{
			//_loggerFactory = loggerFactory;
			_mcConfig = mcConfig;
			_assetCmd = cmdFactory.CreateCommand<MultiChainAssetCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			//_logger = loggerFactory.CreateLogger<MultiChainAssetManager>();
			_logger = logger;
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainAssetManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			//_loggerFactory = loggerFactory;
			_mcConfig = mcConfig;
			_assetCmd = cmdFactory.CreateCommand<MultiChainAssetCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_logger = loggerFactory.CreateLogger<MultiChainAssetManager>();
			_defaultSigner = signer;
		}

		public async Task<bool> IsExist(string assetName)
		{
			var subscribe = await SubscribeAsync(assetName);
			if (subscribe.IsError)
			{
				if (!MultiChainException.IsException(subscribe.Exception, MultiChainErrorCode.RPC_ENTITY_NOT_FOUND))
					throw subscribe.Exception;
				return false;
			}
			return true;
		}

		public MultiChainResult<string> Pay(string toAddress, UInt64 units, object data = null)
		{
			return Pay(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, units, data);
		}
		public MultiChainResult<string> Pay(SignerBase signer, string fromAddress, string toAddress, UInt64 units, object data = null)
		{
			_logger.LogDebug($"Executing PayAsync");

			double qty = units / _mcConfig.Multiple;
			try
			{
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.Pay(qty)
					;
				requestor
					.With()
					.DeclareJson(data)
					;
				var raw = requestor.Request(_txnCmd);
				//var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txnMgr = new TransactionSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}
		}


		public MultiChainResult<string> PayAnnotate(string toAddress, UInt64 units, object annotation)
		{
			return PayAnnotate(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, units, annotation);
		}
		public MultiChainResult<string> PayAnnotate(SignerBase signer, string fromAddress, string toAddress, UInt64 units, object annotation )
		{
			_logger.LogDebug($"Executing PayAsync");

			double qty = units / _mcConfig.Multiple;
			try
			{
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.Pay(qty)
					.AnnotateJson(annotation);
				var raw = requestor.Request(_txnCmd);
				//var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txnMgr = new TransactionSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
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

		public MultiChainResult<string> SendAsset(string toAddress, string assetName, UInt64 units, object data = null)
		{
			return SendAsset(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, data);
		}
		public MultiChainResult<string> SendAsset(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, object data = null)
		{
			_logger.LogDebug($"Executing SendAssetAsync");

			try
			{
				// Must subscribe and get the multiple before running
				UInt64 multiple = GetAssetMultiple(assetName);
				double qty = units / multiple;

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.SendAsset(assetName, qty)
					;
				requestor
					.With()
					.DeclareJson(data)
					;
				var raw = requestor.Request(_txnCmd);
				//var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txnMgr = new TransactionSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}

		}


		public MultiChainResult<string> SendAnnotateAsset(string toAddress, string assetName, UInt64 units, object annotation)
		{
			return SendAnnotateAsset(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, annotation);
		}
		public MultiChainResult<string> SendAnnotateAsset(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, object annotation)
		{
			_logger.LogDebug($"Executing SendAssetAsync");
			try
			{

				// Must subscribe and get the multiple before running
				UInt64 multiple = GetAssetMultiple(assetName);
				double qty = units / multiple;

				var requestor = new TransactionRequestor();
				var to = requestor
					.From(fromAddress)
					.To(toAddress)
					.SendAsset(assetName, qty)
					.AnnotateJson(annotation)
					;
				var raw = requestor.Request(_txnCmd);
				//var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txnMgr = new TransactionSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}

		}


		public MultiChainResult<string> Issue(string toAddress, string assetName, UInt64 units, bool canIssueMore = true, object data = null)
		{
			return Issue(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, canIssueMore, data);
		}
		public MultiChainResult<string> Issue(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, bool canIssueMore = true, object data = null)
		{
			_logger.LogDebug($"Executing IssueAsync");

			try
			{
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueAsset(units)
					;
				requestor
					.With()
					.IssueDetails(assetName, 1, canIssueMore)
					.DeclareJson(data)
					;
				var raw = requestor.Request(_txnCmd);
				//var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txnMgr = new TransactionSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;

				Task.Run(async () =>
				{
					await SubscribeAsync(assetName);
				});

				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}
		}


		public MultiChainResult<string> IssueAnnotate(string toAddress, string assetName, UInt64 units, bool canIssueMore, object annotation)
		{
			return IssueAnnotate(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, canIssueMore, annotation);
		}
		public MultiChainResult<string> IssueAnnotate(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, bool canIssueMore, object annotation)
		{
			_logger.LogDebug($"Executing IssueAsync");

			try
			{
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueAsset(units)
					.AnnotateJson(annotation)
					;
				requestor
					.With()
					.IssueDetails(assetName, 1, canIssueMore)
					;
				var raw = requestor.Request(_txnCmd);
				//var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txnMgr = new TransactionSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}
		}


		public MultiChainResult<string> IssueMore(string toAddress, string assetName, UInt64 units, object data = null)
		{
			return IssueMore(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units);
		}
		public MultiChainResult<string> IssueMore(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, object data = null)
		{
			_logger.LogDebug($"Executing IssueMoreAsync");

			try
			{

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueMoreAsset(assetName, units)
					;
				if (data is { })
				requestor
					.With()
					.DeclareJson(data)
					;

				var raw = requestor.Request(_txnCmd);
				//var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txnMgr = new TransactionSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}
		}

		public MultiChainResult<string> IssueMoreAnnotated(string toAddress, string assetName, UInt64 units, object annotation)
		{
			return IssueMoreAnnotated(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, units, annotation);
		}
		public MultiChainResult<string> IssueMoreAnnotated(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 units, object annotation)
		{
			_logger.LogDebug($"Executing IssueMoreAnnotatedAsync");

			try
			{

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueMoreAsset(assetName, units)
					.AnnotateJson(annotation)
					;
				var raw = requestor.Request(_txnCmd);
				//var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txnMgr = new TransactionSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}
		}

		public async Task<MultiChainResult<GetAssetInfoResult>> GetAssetInfoAsync(string assetName)
		{
			_logger.LogDebug($"Executing GetAssetInfoAsync");

			if (String.IsNullOrEmpty(assetName))
				return new MultiChainResult<GetAssetInfoResult>();
			return await _assetCmd.GetAssetInfoAsync(assetName);
		}

		public async Task<MultiChainResult<GetAddressBalancesResult>> GetAssetBalanceByAddressAsync(string address, string assetName=null)
		{
			_logger.LogDebug($"Executing GetAssetBalanceByAddressAsync");

			var result = await _assetCmd.GetAddressBalancesAsync(address);
			if (result.IsError)
				return new MultiChainResult<GetAddressBalancesResult>(result.Exception);

			try
			{
				if (String.IsNullOrEmpty(assetName))
				{
					var nativeCurrency = result.Result.FirstOrDefault(x => String.IsNullOrEmpty(x.Name));
					if (nativeCurrency is null)
						return new MultiChainResult<GetAddressBalancesResult>();
					return new MultiChainResult<GetAddressBalancesResult>(nativeCurrency);
				}

				GetAddressBalancesResult single = result.Result.FirstOrDefault(x => x.Name == assetName);
				if (single is null)
					return new MultiChainResult<GetAddressBalancesResult>();

				var assetInfo = await GetAssetInfoAsync(assetName);
				single.Raw = Convert.ToUInt64(single.Qty * assetInfo.Result.Multiple);
				return new MultiChainResult<GetAddressBalancesResult>(single);
			}
			catch (Exception ex)
			{
				return new MultiChainResult<GetAddressBalancesResult>(ex);
			}
		}

		public async Task<MultiChainResult<List<GetAddressBalancesResult>>> ListAssetBalancesByAddressAsync(string address)
		{
			_logger.LogDebug($"Executing ListAssetBalancesByAddressAsync");

			var assetsResult = await _assetCmd.GetAddressBalancesAsync(address);
			if (assetsResult.IsError)
				return assetsResult;
			try
			{
				foreach (GetAddressBalancesResult single in assetsResult.Result)
				{
					if (!String.IsNullOrEmpty(single.Name))
					{
						var assetInfo = await GetAssetInfoAsync(single.Name);
						single.Raw = Convert.ToUInt64(single.Qty * assetInfo.Result.Multiple);
					}
				}
				return assetsResult;
			}
			catch (Exception ex)
			{
				return new MultiChainResult<List<GetAddressBalancesResult>>(ex);
			}
		}

		public async Task<MultiChainResult<List<ListAssetsResult>>> ListAssetsAsync(string assetName = "*", bool verbose = false)
		{
			_logger.LogDebug($"Executing ListAssetsAsync");

			return await _assetCmd.ListAssetsAsync(assetName, verbose);
		}
		public async Task<MultiChainResult<List<AssetTransactionsResult>>> ListAssetTransactionsAsync(string assetName)
		{
			_logger.LogDebug($"Executing ListAssetTransactionsAsync");

			return await _assetCmd.ListAssetTransactionsAsync(assetName);
		}
		public async Task<MultiChainResult<VoidType>> SubscribeAsync(string assetName)
		{
			_logger.LogDebug($"Executing SubscribeAsync");

			return await _assetCmd.SubscribeAsync(assetName);
		}

	}
}
