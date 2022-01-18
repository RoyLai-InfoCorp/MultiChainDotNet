// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
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
		private readonly IServiceProvider _container;
		private readonly ILogger _logger;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;

		public MultiChainMultiSigManager(IServiceProvider container)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainMultiSigManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainMultiSigManager(IServiceProvider container, SignerBase signer)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainMultiSigManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = signer;
		}


		public string SendMultiSigAssetAsync(IList<SignerBase> signers, string fromAddress, string toAddress, string assetName, UInt64 qty, string redeemScript)
		{
			_logger.LogDebug($"Executing SendMultiSigAssetAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var txid = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
						.SendAsset(assetName, qty)
						.CreateMultiSigTransaction(txnCmd)
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
		}

		public string CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var raw = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
						.SendAsset(assetName, qty)
						.CreateRawTransaction(txnCmd)
						;

					return raw;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex.ToString());
					throw;
				}
			}


		}


		public string CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty, object data)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var raw = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
						.SendAsset(assetName, qty)
						.With()
						.DeclareJson(data)
						.CreateRawTransaction(txnCmd)
						;

					return raw;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex.ToString());
					throw;
				}
			}


		}

		public string CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var raw = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
						.IssueMoreAsset(assetName, qty)
						.CreateRawTransaction(txnCmd)
						;

					return raw;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex.ToString());
					throw;
				}
			}


		}


		public string CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty, object data)
		{
			_logger.LogDebug($"Executing CreateSignatureSlipAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var raw = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
						.IssueMoreAsset(assetName, qty)
						.With()
						.DeclareJson(data)
						.CreateRawTransaction(txnCmd)
						;

					return raw;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex.ToString());
					throw;
				}
			}


		}

		public string[] SignMultiSig(string signatureSlip, string redeemScript)
		{
			return SignMultiSig(_defaultSigner, signatureSlip, redeemScript);
		}

		public string[] SignMultiSig(SignerBase signer, string signatureSlip, string redeemScript)
		{
			_logger.LogDebug($"Executing SignMultiSig");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var signatures = new MultiChainFluent()
						.AddLogger(_logger)
						.UseMultiSigTransaction(txnCmd)
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

		}

		public string SendMultiSigAsset(IList<string[]> signatures, string signatureSlip, string redeemScript)
		{
			_logger.LogDebug($"Executing SendMultiSigAssetAsync");
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var txid = new MultiChainFluent()
						.AddLogger(_logger)
						.UseMultiSigTransaction(txnCmd)
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
}
