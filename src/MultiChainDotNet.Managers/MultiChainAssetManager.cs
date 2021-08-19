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
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;
		MultiChainAssetCommand _assetCmd;
		MultiChainTransactionCommand _txnCmd;

		public MultiChainAssetManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig)
		{
			_loggerFactory = loggerFactory;
			_mcConfig = mcConfig;
			_assetCmd = cmdFactory.CreateCommand<MultiChainAssetCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_logger = loggerFactory.CreateLogger<MultiChainAssetManager>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainAssetManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_loggerFactory = loggerFactory;
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

		public async Task<MultiChainResult<string>> PayAsync(string toAddress, UInt64 amt, object data = null)
		{
			_logger.LogDebug($"Executing PayAsync");

			if (_defaultSigner is { })
				return await PayAsync(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, amt, data);

			double qty = amt / _mcConfig.Multiple;
			return await _assetCmd.SendAsync(toAddress, qty, "", data);
		}

		public Task<MultiChainResult<string>> PayAnnotateAsync(SignerBase signer, string fromAddress, string toAddress, UInt64 amt, object data = null)
		{
			_logger.LogDebug($"Executing PayAsync");

			double qty = amt / _mcConfig.Multiple;
			try
			{
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.Pay(qty)
					.AnnotateJson(data);
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return Task.FromResult(new MultiChainResult<string>(txid));
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return Task.FromResult(new MultiChainResult<string>(ex));
			}
		}

		public Task<MultiChainResult<string>> PayAsync(SignerBase signer, string fromAddress, string toAddress, UInt64 amt, object data=null)
		{
			_logger.LogDebug($"Executing PayAsync");

			double qty = amt / _mcConfig.Multiple;
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
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return Task.FromResult(new MultiChainResult<string>(txid));
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return Task.FromResult(new MultiChainResult<string>(ex));
			}
		}

		public async Task<MultiChainResult<string>> SendAssetAsync(string toAddress, string assetName, UInt64 amt, object data = null)
		{
			_logger.LogDebug($"Executing SendAssetAsync");

			if (_defaultSigner is { })
				return await SendAssetAsync(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, amt, data);

			// Remember to subscribe first
			await _assetCmd.SubscribeAsync(assetName);

			var assetResult = await _assetCmd.GetAssetInfoAsync(assetName);
			if (assetResult.IsError)
				return new MultiChainResult<string>(assetResult.Exception);
			double qty = amt / assetResult.Result.Multiple;
			return await _assetCmd.SendAsync(toAddress, qty, assetName, data);
		}

		public async Task<MultiChainResult<string>> SendAssetAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null)
		{
			_logger.LogDebug($"Executing SendAssetAsync");

			try
			{
				// Remember to subscribe first
				await _assetCmd.SubscribeAsync(assetName);

				var assetResult = _assetCmd.GetAssetInfoAsync(assetName).Result;
				if (assetResult.IsError)
					return new MultiChainResult<string>(assetResult.Exception);
				double qty = amt / assetResult.Result.Multiple;

				var balance = await GetAssetBalanceByAddressAsync(fromAddress, assetName);

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
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
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

		public async Task<MultiChainResult<string>> SendAnnotateAssetAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null)
		{
			_logger.LogDebug($"Executing SendAssetAsync");

			try
			{
				// Remember to subscribe first
				await _assetCmd.SubscribeAsync(assetName);

				var assetResult = _assetCmd.GetAssetInfoAsync(assetName).Result;
				if (assetResult.IsError)
					return new MultiChainResult<string>(assetResult.Exception);
				double qty = amt / assetResult.Result.Multiple;

				var balance = await GetAssetBalanceByAddressAsync(fromAddress, assetName);

				var requestor = new TransactionRequestor();
				var to = requestor
					.From(fromAddress)
					.To(toAddress)
					.SendAsset(assetName, qty)
					.AnnotateJson(data)
					;
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
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


		public Task<MultiChainResult<string>> IssueAnnotateAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, bool canIssueMore = true, object data = null)
		{
			_logger.LogDebug($"Executing IssueAsync");

			try
			{
				// Note: No need to subscribe since assetName hasn't exist yet

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueAsset(amt)
					.AnnotateJson(data)
					;
				requestor
					.With()
					.IssueDetails(assetName, 1, canIssueMore)
					;
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return Task.FromResult(new MultiChainResult<string>(txid));
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return Task.FromResult(new MultiChainResult<string>(ex));
			}
		}

		public MultiChainResult<string> Issue(string toAddress, string assetName, UInt64 amt, bool canIssueMore = true, object data = null)
		{
			return Issue(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, amt, canIssueMore, data);
		}
		public MultiChainResult<string> Issue(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, bool canIssueMore = true, object data = null)
		{
			_logger.LogDebug($"Executing IssueAsync");

			try
			{
				// Note: No need to subscribe since assetName hasn't exist yet

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueAsset(amt)
					;
				requestor
					.With()
					.IssueDetails(assetName, 1, canIssueMore)
					.DeclareJson(data)
					;
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
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


		public async Task<MultiChainResult<string>> IssueMoreAsync(string toAddress, string assetName, UInt64 amt, object data = null)
		{
			_logger.LogDebug($"Executing IssueMoreAsync");

			if (_defaultSigner is { })
				return await IssueMoreAsync (_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, assetName, amt, data);

			// Remember to subscribe first
			await _assetCmd.SubscribeAsync(assetName);

			var assetResult = _assetCmd.GetAssetInfoAsync(assetName).Result;
			if (assetResult.IsError)
				return new MultiChainResult<string>(assetResult.Exception);
			double qty = amt / assetResult.Result.Multiple;
			return await _assetCmd.IssueMoreAssetAsync(toAddress, assetName, qty);
		}

		public async Task<MultiChainResult<string>> IssueMoreAnnotatedAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null)
		{
			_logger.LogDebug($"Executing IssueMoreAnnotatedAsync");

			try
			{
				// Remember to subscribe first
				await _assetCmd.SubscribeAsync(assetName);

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueMoreAsset(assetName, amt)
					.AnnotateJson(data)
					;
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
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

		public async Task<MultiChainResult<string>> IssueMoreAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null)
		{
			_logger.LogDebug($"Executing IssueMoreAsync");

			try
			{
				// Remember to subscribe first
				await _assetCmd.SubscribeAsync(assetName);

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueMoreAsset(assetName, amt)
					;
				if (data is { })
				requestor
					.With()
					.DeclareJson(data)
					;

				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
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


		public MultiChainResult<string> SendMultiSigAssetAsync(IList<SignerBase> signers, string fromAddress, string toAddress, string assetName, double qty, string redeemScript)
		{
			_logger.LogDebug($"Executing SendMultiSigAssetAsync");

			try
			{
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.SendAsset(assetName, qty)
					;
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), _txnCmd);
				foreach (SignerBase signer in signers)
					txnMgr.AddSigner(signer);
				var txid = txnMgr
					.MultiSign(raw, redeemScript)
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

		public MultiChainResult<string> CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, double qty)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");

			try
			{
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.SendAsset(assetName, qty)
					;
				var raw = requestor.Request(_txnCmd);
				return new MultiChainResult<string>(raw);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}

		}

		//public MultiChainResult<string> CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty, object data = null)
		//{
		//	_logger.LogDebug($"Executing CreateSignatureSlipAsync");

		//	try
		//	{
		//		var requestor = new TransactionRequestor();
		//		requestor
		//			.From(fromAddress)
		//			.To(toAddress)
		//			.IssueMoreAsset(assetName, qty)
		//			;
		//		if (data is { })
		//			requestor
		//				.With()
		//				.DeclareJson(data)
		//				;
		//		var raw = requestor.Request(_txnCmd);
		//		return new MultiChainResult<string>(raw);
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.LogWarning(ex.ToString());
		//		return new MultiChainResult<string>(ex);
		//	}

		//}

		public MultiChainResult<string> CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty, object data = null)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");

			try
			{
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.To(toAddress)
					.IssueMoreAsset(assetName, qty)
					;
				if (data is { })
					requestor
						.With()
						.DeclareJson(data)
						;
				var raw = requestor.Request(_txnCmd);
				return new MultiChainResult<string>(raw);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}

		}

		public MultiChainResult<string[]> SignMultiSig(SignerBase signer, string signatureSlip, string redeemScript)
		{
			_logger.LogDebug($"Executing SignMultiSig");

			try
			{
				var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), _txnCmd);
				var signatures = txnMgr
					.AddSigner(signer)
					.MultiSignPartial(signatureSlip, redeemScript)
					;
				return new MultiChainResult<string[]>(signatures);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string[]>(ex);
			}
		}

		public MultiChainResult<string> SendMultiSigAsset(IList<string[]> signatures, string signatureSlip, string redeemScript)
		{
			_logger.LogDebug($"Executing SendMultiSigAssetAsync");

			try
			{
				var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), _txnCmd);
				var txid = txnMgr
					.MultiSign(signatureSlip, redeemScript, signatures)
					.Send();
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}

		}

		public async Task<MultiChainResult<string>> SendMultiSigAssetAsync(IList<string[]> signatures, string signatureSlip, string redeemScript)
		{
			_logger.LogDebug($"Executing SendMultiSigAssetAsync");

			try
			{
				var multisigMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), _txnCmd);
				var signed = multisigMgr
					.MultiSign(signatureSlip, redeemScript, signatures)
					.RawSigned();
				var result = await _txnCmd.SendRawTransactionAsync(signed);
				return new MultiChainResult<string>(result.Result);
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
