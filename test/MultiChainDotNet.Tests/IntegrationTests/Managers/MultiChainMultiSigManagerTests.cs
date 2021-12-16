// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
	public class MultiChainMultiSigManagerTests : TestCommandFactoryBase
	{
		IMultiChainAssetManager _assetManager;
		IMultiChainTransactionManager _txnManager;
		IMultiChainMultiSigManager _multisSigManager;
		private IMultiChainPermissionsManager _permManager;

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
			services.AddTransient<IMultiChainPermissionsManager, MultiChainPermissionsManager>();
		}

		[SetUp]
		public void Setup()
		{
			_assetManager = _container.GetRequiredService<IMultiChainAssetManager>();
			_txnManager = _container.GetRequiredService<IMultiChainTransactionManager>();
			_multisSigManager = _container.GetRequiredService<IMultiChainMultiSigManager>();
			_permManager = _container.GetRequiredService<IMultiChainPermissionsManager>();
		}

		[Test, Order(70)]
		public async Task Should_not_be_able_to_create_transaction_when_multisig_address_has_no_fund()
		{
			var addressMgr = _container.GetRequiredService<IMultiChainAddressManager>();
			var multisigResult = addressMgr.CreateMultiSig(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var multisig = multisigResult.Address;
			var redeemScript = multisigResult.RedeemScript;
			var balanceResult = await _assetManager.GetAssetBalanceByAddressAsync(multisig, "");
			if (balanceResult.Raw < 1000)
			{
				var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
				var issueResult = _assetManager.Issue(multisig, assetName, 10, true);

				// ACT
				Action action = ()=> _multisSigManager.CreateSendAssetSignatureSlipAsync(multisig, _testUser2.NodeWallet, assetName, 1);
				action.Should().Throw<MultiChainException>().WithMessage($"*{MultiChainErrorCode.RPC_WALLET_INSUFFICIENT_FUNDS.ToString()}*");
			}
		}


		[Test, Order(80)]
		public async Task Should_be_able_to_multisign_send_asset_transaction()
		{
			var addressMgr = _container.GetRequiredService<IMultiChainAddressManager>();
			var multisigResult = addressMgr.CreateMultiSig(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var multisig = multisigResult.Address;
			var redeemScript = multisigResult.RedeemScript;
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			_assetManager.Issue(multisig, assetName, 10, true);
			var balanceResult = await _assetManager.GetAssetBalanceByAddressAsync(multisig, "");
			if (balanceResult.Raw < 1000)
			{
				_assetManager.Pay(multisig, 5000);
			}
			_permManager.GrantPermission(multisig, "send");

			// ACT
			var signatureSlip = _multisSigManager.CreateSendAssetSignatureSlipAsync(multisig, _testUser2.NodeWallet, assetName, 1);

			var relayer1Sig = _multisSigManager.SignMultiSig(new DefaultSigner(_relayer1.Ptekey), signatureSlip, redeemScript);
			var relayer2Sig = _multisSigManager.SignMultiSig(new DefaultSigner(_relayer2.Ptekey), signatureSlip, redeemScript);
			var result = _multisSigManager.SendMultiSigAsset(new List<string[]> { relayer1Sig, relayer2Sig }, signatureSlip, redeemScript);

			// ASSERT
			result.Should().NotBeNullOrEmpty();
		}


	}
}
