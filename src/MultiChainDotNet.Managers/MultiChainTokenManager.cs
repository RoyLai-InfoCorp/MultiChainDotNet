using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainToken;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiChainDotNet.Core.MultiChainAsset.GetAssetInfoResult;
using static MultiChainDotNet.Core.MultiChainToken.GetTokenBalancesResult;

namespace MultiChainDotNet.Managers
{
	public class MultiChainTokenManager : IMultiChainTokenManager
	{
		private readonly ILogger _logger;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;
		MultiChainTokenCommand _tokenCmd;
		MultiChainTransactionCommand _txnCmd;
		MultiChainPermissionCommand _permCmd;
		MultiChainAssetCommand _assetCmd;

		public MultiChainTokenManager(ILogger<MultiChainAssetManager> logger,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig)
		{
			_mcConfig = mcConfig;
			_tokenCmd = cmdFactory.CreateCommand<MultiChainTokenCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_permCmd = cmdFactory.CreateCommand<MultiChainPermissionCommand>();
			_assetCmd = cmdFactory.CreateCommand<MultiChainAssetCommand>();
			_logger = logger;
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainTokenManager(ILogger<MultiChainAssetManager> logger,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_mcConfig = mcConfig;
			_tokenCmd = cmdFactory.CreateCommand<MultiChainTokenCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_permCmd = cmdFactory.CreateCommand<MultiChainPermissionCommand>();
			_assetCmd = cmdFactory.CreateCommand<MultiChainAssetCommand>();
			_logger = logger;
			_defaultSigner = signer;
		}

		public async Task<bool> IsExist(string assetName)
		{
			try
			{
				var info = await GetNonfungibleAssetInfo(assetName);
				return info is { };
			}
			catch (MultiChainException ex)
			{
				if (ex.Code == MultiChainErrorCode.RPC_ENTITY_NOT_FOUND)
					return false;
				throw;
			}
		}

		public string SendToken(string toAddress, string nfaName, string token, int qty = 1, object data = null)
		{
			return SendToken(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, nfaName, token, qty, data);
		}

		public string SendToken(SignerBase signer, string fromAddress, string toAddress, string nfaName, string tokenId, int qty = 1, object data = null)
		{
			_logger.LogDebug($"Executing SendTokenAsync");

			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.SendToken(nfaName, tokenId, qty)
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

		public Task<string> SendAnnotateTokenAsync(string toAddress, string nfaName, string tokenId, int qty = 1, object annotation = null)
		{
			return SendAnnotateTokenAsync(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, nfaName, tokenId, qty, annotation);
		}

		public Task<string> SendAnnotateTokenAsync(SignerBase signer, string fromAddress, string toAddress, string nfaName, string tokenId, int qty = 1, object annotation = null)
		{
			_logger.LogDebug($"Executing SendAssetAsync");
			try
			{
				return Task.FromResult(new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.SendToken(nfaName, tokenId, qty)
						.AnnotateJson(annotation)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public Task<string> IssueNonfungibleAsset(string toAddress, string nfaName, object data = null)
		{
			return IssueNonfungibleAsset(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, nfaName, data);
		}

		public async Task<string> IssueNonfungibleAsset(SignerBase signer, string fromAddress, string toAddress, string nfaName, object data = null)
		{
			_logger.LogDebug($"Executing IssueAsync");

			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.IssueAsset(0)
					.With()
						.IssueNonFungibleAsset(nfaName)
						.DeclareJson(data)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;
				await TaskHelper.WaitUntilTrue(async () =>
				{
					var exist = await _tokenCmd.GetNfaInfo(nfaName);
					if (exist.IsError)
					{
						if (exist.Exception.IsMultiChainException(MultiChainErrorCode.RPC_ENTITY_NOT_FOUND))
							return false;
						throw exist.Exception;
					}
					return true;
				}, 5, 500);

				txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.Permit("issue", nfaName)
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

		public string IssueToken(string toAddress, string nfaName, string tokenId, int qty, object annotation = null)
		{
			return IssueTokenAnnotate(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, nfaName, tokenId, qty, annotation);
		}

		public string IssueTokenAnnotate(SignerBase signer, string fromAddress, string toAddress, string nfaName, string tokenId, int qty, object annotation = null)
		{
			_logger.LogDebug($"Executing IssueAsync");

			try
			{
				return new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.IssueToken(nfaName, tokenId, qty)
						.AnnotateJson(annotation)
					.With()
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public async Task<GetAssetInfoResult> GetNonfungibleAssetInfo(string assetName)
		{
			_logger.LogDebug($"Executing GetTokenInfoAsync");

			if (String.IsNullOrEmpty(assetName))
				return null;
			return (await _assetCmd.GetAssetInfoAsync(assetName)).Result;
		}

		public async Task<IList<GetAssetInfoIssuesResult>> ListNftByAssetAsync(string nfaName)
		{
			_logger.LogDebug($"Executing ListNftByAssetAsync");
			var result = await _tokenCmd.ListNftByAssetAsync(nfaName);
			if (result.IsError)
				throw result.Exception;
			return result.Result;
		}

		public async Task<IList<GetTokenBalanceItem>> ListNftByAddressAsync(string address, string  nfaName=null, string tokenId = null)
		{
			_logger.LogDebug($"Executing ListNftByAddressAsync");
			var result = await _tokenCmd.ListNftByAddressAsync(address, nfaName, tokenId);
			if (result.IsError)
				throw result.Exception;
			return result.Result;
		}

		public Task SubscribeAsync(string assetName)
		{
			return _assetCmd.SubscribeAsync(assetName);
		}


	}

}
