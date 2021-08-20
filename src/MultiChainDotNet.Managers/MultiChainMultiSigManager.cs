using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Builders;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				//var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), _txnCmd);
				var txnMgr = new MultiSigSender(_txnCmd);
				txnMgr.AddLogger(_logger);
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
				//var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), _txnCmd);
				var txnMgr = new MultiSigSender(_txnCmd);
				var signatures = txnMgr
					.AddLogger(_logger)
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
				//var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), _txnCmd);
				var txnMgr = new MultiSigSender(_txnCmd);
				var txid = txnMgr
					.AddLogger(_logger)
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
				//var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), _txnCmd);
				var txnMgr = new MultiSigSender(_txnCmd);

				var signed = txnMgr
					.AddLogger(_logger)
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

	}
}
