// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;

namespace MultiChainDotNet.Managers
{
	public class MultiChainMultiSigManager : IMultiChainMultiSigManager
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;
		MultiChainTransactionCommand _txnCmd;

		public MultiChainMultiSigManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig)
		{
			_loggerFactory = loggerFactory;
			_mcConfig = mcConfig;
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_logger = loggerFactory.CreateLogger<MultiChainAssetManager>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public string SendMultiSigAssetAsync(IList<SignerBase> signers, string fromAddress, string toAddress, string assetName, UInt64 qty, string redeemScript)
		{
			_logger.LogDebug($"Executing SendMultiSigAssetAsync");

			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
					.SendAsset(assetName, qty)
					.CreateMultiSigTransaction(_txnCmd)
					.AddMultiSigSigners(signers)
					.MultiSign(redeemScript)
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

		public string CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");

			try
			{
				var raw = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
					.SendAsset(assetName, qty)
					.CreateRawTransaction(_txnCmd)
					;

				return raw;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}

		}


		public string CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty, object data)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");

			try
			{
				var raw = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
					.SendAsset(assetName, qty)
					.With()
					.DeclareJson(data)
					.CreateRawTransaction(_txnCmd)
					;

				return raw;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}

		}

		public string CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");

			try
			{
				var raw = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
					.IssueMoreAsset(assetName, qty)
					.CreateRawTransaction(_txnCmd)
					;

				return raw;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}

		}


		public string CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty, object data)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");

			try
			{
				var raw = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
					.IssueMoreAsset(assetName, qty)
					.With()
					.DeclareJson(data)
					.CreateRawTransaction(_txnCmd)
					;

				return raw;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}

		}

		public string[] SignMultiSig(string signatureSlip, string redeemScript)
		{
			return SignMultiSig(_defaultSigner, signatureSlip, redeemScript);
		}

		public string[] SignMultiSig(SignerBase signer, string signatureSlip, string redeemScript)
		{
			_logger.LogDebug($"Executing SignMultiSig");

			try
			{
				var signatures = new MultiChainFluent()
					.AddLogger(_logger)
					.UseMultiSigTransaction(_txnCmd)
						.AddMultiSigSigner(signer)
						.MultiSignPartial(signatureSlip, redeemScript)
					;

				return signatures;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public string SendMultiSigAsset(IList<string[]> signatures, string signatureSlip, string redeemScript)
		{
			_logger.LogDebug($"Executing SendMultiSigAssetAsync");
			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.UseMultiSigTransaction(_txnCmd)
						.AddRawMultiSignatureTransaction(signatureSlip)
						.AddMultiSignatures(signatures)
						.MultiSign(redeemScript)
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

	}
}
