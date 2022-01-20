using Microsoft.Extensions.DependencyInjection;
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
		private readonly IServiceProvider _container;
		private readonly ILogger _logger;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;

		public MultiChainTokenManager(IServiceProvider container)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainTokenManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainTokenManager(IServiceProvider container, SignerBase signer)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainTokenManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
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
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var txid = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
							.SendToken(nfaName, tokenId, qty)
						.With()
							.AttachJson(data)
						.CreateNormalTransaction(txnCmd)
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

		}

		public Task<string> SendAnnotateTokenAsync(string toAddress, string nfaName, string tokenId, int qty = 1, object annotation = null)
		{
			return SendAnnotateTokenAsync(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, nfaName, tokenId, qty, annotation);
		}

		public Task<string> SendAnnotateTokenAsync(SignerBase signer, string fromAddress, string toAddress, string nfaName, string tokenId, int qty = 1, object annotation = null)
		{
			_logger.LogDebug($"Executing SendAssetAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					return Task.FromResult(new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
							.SendToken(nfaName, tokenId, qty)
							.AnnotateJson(annotation)
						.CreateNormalTransaction(txnCmd)
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

		}

		public Task<string> IssueNonfungibleAsset(string toAddress, string nfaName, object data = null)
		{
			return IssueNonfungibleAsset(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, nfaName, data);
		}

		public async Task<string> IssueNonfungibleAsset(SignerBase signer, string fromAddress, string toAddress, string nfaName, object data = null)
		{
			_logger.LogDebug($"Executing IssueAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var txid = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
							.IssueAsset(0)
						.With()
							.IssueNonFungibleAsset(nfaName)
							.AttachJson(data)
						.CreateNormalTransaction(txnCmd)
							.AddSigner(signer)
							.Sign()
							.Send()
						;

					await TaskHelper.WaitUntilTrue(async () =>
					{
						var tokenCmd = scope.ServiceProvider.GetRequiredService<MultiChainTokenCommand>();
						var exist = await tokenCmd.GetNfaInfo(nfaName);
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
						.CreateNormalTransaction(txnCmd)
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


		}

		public string IssueToken(string toAddress, string nfaName, string tokenId, int qty, object annotation = null)
		{
			return IssueTokenAnnotate(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, nfaName, tokenId, qty, annotation);
		}

		public string IssueTokenAnnotate(SignerBase signer, string fromAddress, string toAddress, string nfaName, string tokenId, int qty, object annotation = null)
		{
			_logger.LogDebug($"Executing IssueAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					return new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
							.IssueToken(nfaName, tokenId, qty)
							.AnnotateJson(annotation)
						.With()
						.CreateNormalTransaction(txnCmd)
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


		}

		public async Task<GetAssetInfoResult> GetNonfungibleAssetInfo(string assetName)
		{
			_logger.LogDebug($"Executing GetTokenInfoAsync");
			using (var scope = _container.CreateScope())
			{
				var assetCmd = scope.ServiceProvider.GetRequiredService<MultiChainAssetCommand>();
				if (String.IsNullOrEmpty(assetName))
					return null;
				return (await assetCmd.GetAssetInfoAsync(assetName)).Result;
			}


		}

		public async Task<IList<GetAssetInfoIssuesResult>> ListNftByAssetAsync(string nfaName)
		{
			_logger.LogDebug($"Executing ListNftByAssetAsync");
			using (var scope = _container.CreateScope())
			{
				var tokenCmd = scope.ServiceProvider.GetRequiredService<MultiChainTokenCommand>();
				var result = await tokenCmd.ListNftByAssetAsync(nfaName);
				if (result.IsError)
					throw result.Exception;
				return result.Result;
			}

		}

		public async Task<IList<GetTokenBalanceItem>> ListNftByAddressAsync(string address, string  nfaName=null, string tokenId = null)
		{
			_logger.LogDebug($"Executing ListNftByAddressAsync");
			using (var scope = _container.CreateScope())
			{
				var tokenCmd = scope.ServiceProvider.GetRequiredService<MultiChainTokenCommand>();
				var result = await tokenCmd.ListNftByAddressAsync(address, nfaName, tokenId);
				if (result.IsError)
					throw result.Exception;
				return result.Result;
			}

		}

		public Task SubscribeAsync(string assetName)
		{
			using (var scope = _container.CreateScope())
			{
				var assetCmd = scope.ServiceProvider.GetRequiredService<MultiChainAssetCommand>();
				return assetCmd.SubscribeAsync(assetName);
			}

		}


	}

}
