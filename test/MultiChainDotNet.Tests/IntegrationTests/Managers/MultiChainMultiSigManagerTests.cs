using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
	public class MultiChainMultiSigManagerTests : TestCommandFactoryBase
	{
		IMultiChainAssetManager _assetManager;
		IMultiChainTransactionManager _txnManager;
		IMultiChainMultiSigManager _multisSigManager;

		public class TestState
		{
			public Guid Id { get; set; }
			public string StreamName { get; set; }
			public string AssetName { get; set; }
			public string MultiSigAddress { get; set; }
			public string RedeemScript { get; set; }

		}

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services.AddTransient<IMultiChainAssetManager, MultiChainAssetManager>();
			services.AddTransient<IMultiChainTransactionManager, MultiChainTransactionManager>();
			services.AddTransient<IMultiChainAddressManager, MultiChainAddressManager>();
			services.AddTransient<IMultiChainMultiSigManager, MultiChainMultiSigManager>();
		}

		[SetUp]
		public void Setup()
		{
			_assetManager = _container.GetRequiredService<IMultiChainAssetManager>();
			_txnManager = _container.GetRequiredService<IMultiChainTransactionManager>();
			_multisSigManager = _container.GetRequiredService<IMultiChainMultiSigManager>();
		}

		[Test, Order(70)]
		public async Task Should_not_be_able_to_create_transaction_when_multisig_address_has_no_fund()
		{
			var addressMgr = _container.GetRequiredService<IMultiChainAddressManager>();
			var multisigResult = addressMgr.CreateMultiSig(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var multisig = multisigResult.Result.Address;
			var redeemScript = multisigResult.Result.RedeemScript;
			var balanceResult = await _assetManager.GetAssetBalanceByAddressAsync(multisig, "");
			if (balanceResult.Result.Raw < 1000)
			{
				var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
				var issueResult = _assetManager.Issue(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, multisig, assetName, 10, true);

				// ACT
				var signatureResult = _multisSigManager.CreateSendAssetSignatureSlipAsync(multisig, _testUser2.NodeWallet, assetName, 1);
				Assert.That(MultiChainException.IsException(signatureResult.Exception, MultiChainErrorCode.RPC_WALLET_INSUFFICIENT_FUNDS)
					, Is.True);
			}
		}


		[Test, Order(80)]
		public async Task Should_be_able_to_multisign_send_asset_transaction()
		{
			var addressMgr = _container.GetRequiredService<IMultiChainAddressManager>();
			var multisigResult = addressMgr.CreateMultiSig(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var multisig = multisigResult.Result.Address;
			var redeemScript = multisigResult.Result.RedeemScript;
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			_assetManager.Issue(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, multisig, assetName, 10, true);
			var balanceResult = await _assetManager.GetAssetBalanceByAddressAsync(multisig, "");
			if (balanceResult.Result.Raw < 1000)
			{
				_assetManager.Pay(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, multisig, 5000);
			}

			// ACT
			var signatureResult = _multisSigManager.CreateSendAssetSignatureSlipAsync(multisig, _testUser2.NodeWallet, assetName, 1);
			Assert.That(signatureResult.IsError, Is.False, signatureResult.ExceptionMessage);
			var signatureSlip = signatureResult.Result;

			var relayer1Sig = _multisSigManager.SignMultiSig(new DefaultSigner(_relayer1.Ptekey), signatureSlip, redeemScript);
			var relayer2Sig = _multisSigManager.SignMultiSig(new DefaultSigner(_relayer2.Ptekey), signatureSlip, redeemScript);
			var result = _multisSigManager.SendMultiSigAsset(new List<string[]> { relayer1Sig.Result, relayer2Sig.Result }, signatureSlip, redeemScript);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

	}
}
