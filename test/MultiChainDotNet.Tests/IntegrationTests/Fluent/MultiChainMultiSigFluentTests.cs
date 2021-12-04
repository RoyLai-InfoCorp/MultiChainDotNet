// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAddress;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.Utils;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Builders;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Fluent;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.IntegrationTests.Fluent
{
	[TestFixture]
	public class MultiChainMultiSigFluentTests : TestCommandFactoryBase
	{
		public class TestState
		{
			public Guid Id { get; set; }
			public string StreamName { get; set; }
			public string AssetName { get; set; }
			public string MultiSigAddress { get; set; }
			public string RedeemScript { get; set; }

		}

		IMultiChainCommandFactory _cmdFactory;
		ILogger _logger;
		MultiChainTransactionCommand _txnCmd;

		[SetUp]
		public async Task SetUp()
		{
			_cmdFactory = _container.GetRequiredService<IMultiChainCommandFactory>();
			_txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_logger = _loggerFactory.CreateLogger<MultiChainMultiSigFluentTests>();
		}

		private async Task Prepare_MultiSigAddress(int n, string[] addresses)
		{
			// PREPARE

			// Create multisig address
			var addrCmd = _cmdFactory.CreateCommand<MultiChainAddressCommand>();
			var multisig = await addrCmd.CreateMultiSigAsync(n, addresses);
			await addrCmd.ImportAddressAsync(multisig.Result.Address);
			Console.WriteLine(multisig.Result.ToJson());

			// Grant multisig permission to send and receive
			var permCmd = _cmdFactory.CreateCommand<MultiChainPermissionCommand>();
			await permCmd.GrantPermissionAsync(multisig.Result.Address, "send,receive");

			// Issue asset into multisig
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			var result = await assetCmd.SendAssetAsync(multisig.Result.Address, "openasset", 10);
			if (result.IsError)
				throw result.Exception;

			// Fund the relayers
			await assetCmd.SendAsync(_relayer1.NodeWallet, 1000_000);
			await assetCmd.SendAsync(_relayer2.NodeWallet, 1000_000);
			await assetCmd.SendAsync(_relayer3.NodeWallet, 1000_000);
			await assetCmd.SendAsync(multisig.Result.Address, 1000_000);

			_stateDb.SaveState(new TestState { MultiSigAddress = multisig.Result.Address, RedeemScript = multisig.Result.RedeemScript, AssetName = "openasset" });
		}

		[Test, Order(210)]
		public async Task Should_send_asset_with_1_of_2_multisignature()
		{
			await Prepare_MultiSigAddress(1, new string[] { _relayer1.Pubkey, _relayer2.Pubkey });
			var state = _stateDb.GetState<TestState>();

			// PREPARE
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			var balance = await assetCmd.GetAddressBalancesAsync(state.MultiSigAddress);

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(state.MultiSigAddress)
				.To(_testUser1.NodeWallet)
					.SendAsset(state.AssetName, 1)
				.CreateMultiSigTransaction(_txnCmd)
					.AddMultiSigSigner(new DefaultSigner(_relayer1.Ptekey))
					.MultiSign(state.RedeemScript)
					.Send()
					;

			// ASSERT
			Assert.IsNotNull(txid);
		}

		[Test, Order(220)]
		public async Task Should_send_asset_with_2_of_3_multisignature()
		{
			await Prepare_MultiSigAddress(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var state = _stateDb.GetState<TestState>();

			// PREPARE
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(state.MultiSigAddress)
				.To(_testUser1.NodeWallet)
					.SendAsset(state.AssetName, 1)
				.CreateMultiSigTransaction(_txnCmd)
					.AddMultiSigSigner(new DefaultSigner(_relayer1.Ptekey))
					.AddMultiSigSigner(new DefaultSigner(_relayer2.Ptekey))
					.MultiSign(state.RedeemScript)
					.Send()
					;

			// ASSERT
			Console.WriteLine(txid);
		}

		[Test, Order(230)]
		public async Task Should_send_asset_with_2_of_3_multisigature_in_stages()
		{
			await Prepare_MultiSigAddress(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var state = _stateDb.GetState<TestState>();

			// ACT 1
			var request = new MultiChainFluent()
				.AddLogger(_logger)
				.From(state.MultiSigAddress)
				.To(_testUser1.NodeWallet)
					.SendAsset(state.AssetName, 1)
					.CreateRawTransaction(_txnCmd);

			// ACT 2
			var signatures1 = new MultiChainFluent()
				.AddLogger(_logger)
				.UseMultiSigTransaction(_txnCmd)
					.AddMultiSigSigner(new DefaultSigner(_relayer1.Ptekey))
					.MultiSignPartial(request, state.RedeemScript);

			var signatures2 = new MultiChainFluent()
				.AddLogger(_logger)
				.UseMultiSigTransaction(_txnCmd)
					.AddMultiSigSigner(new DefaultSigner(_relayer2.Ptekey))
					.MultiSignPartial(request,state.RedeemScript);

			// ACT 3
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.UseMultiSigTransaction(_txnCmd)
					.AddRawMultiSignatureTransaction(request)
					.AddMultiSignatures(new List<string[]> { signatures1, signatures2 })
					.MultiSign(state.RedeemScript)
					.Send();

			Assert.That(String.IsNullOrEmpty(txid), Is.False);
		}



	}
}
