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

namespace MultiChainDotNet.Tests.IntegrationTests.Fluent
{
	[TestFixture]
	public class MultiSigSenderTests : TestCommandFactoryBase
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

		[SetUp]
		public async Task SetUp()
		{
			_cmdFactory = _container.GetRequiredService<IMultiChainCommandFactory>();
			//_logger = _loggerFactory.CreateLogger<TransactionSenderTests>();
		}


		public async Task Prepare_MultiSigAddress(int n, string[] addresses)
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

		[Test, Order(10)]
		public async Task Should_send_asset_with_1_of_2_multisignature()
		{
			await Prepare_MultiSigAddress(1, new string[] { _relayer1.Pubkey, _relayer2.Pubkey });
			var state = _stateDb.GetState<TestState>();

			// PREPARE
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			var balance = await assetCmd.GetAddressBalancesAsync(state.MultiSigAddress);

			// ACT
			var requestor = new TransactionRequestor();
			requestor
				.From(state.MultiSigAddress)
				.To(_testUser1.NodeWallet)
				.SendAsset(state.AssetName, 1)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_relayer1.Ptekey))
				.MultiSign(raw, state.RedeemScript)
				.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
		}

		[Test, Order(20)]
		public async Task Should_send_asset_with_2_of_3_multisignature()
		{
			await Prepare_MultiSigAddress(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var state = _stateDb.GetState<TestState>();

			// PREPARE
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();

			// ACT
			var requestor = new TransactionRequestor();
			requestor
				.From(state.MultiSigAddress)
				//.From(multisig.Address)
				.To(_testUser1.NodeWallet)
				.SendAsset(state.AssetName, 1)
				//.SendAsset("asset1", 1)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_relayer1.Ptekey))
				.AddSigner(new DefaultSigner(_relayer2.Ptekey))
				//.MultiSign(raw, multisig.RedeemScript)
				.MultiSign(raw, state.RedeemScript)
				.Send()
				;

			// ASSERT
			Console.WriteLine(txid);
		}

		[Test, Order(30)]
		public async Task Should_send_asset_with_2_of_3_multisigature_in_stages()
		{
			await Prepare_MultiSigAddress(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var state = _stateDb.GetState<TestState>();

			// PREPARE
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();

			// ACT
			var requestor = new TransactionRequestor();
			requestor
				.From(state.MultiSigAddress)
				//.From(multisig.Address)
				.To(_testUser1.NodeWallet)
				.SendAsset(state.AssetName, 1)
				//.SendAsset("asset1", 1)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), txnCmd);
			var txHash = txnMgr
				.CreateMultiSigTransactionHashes(raw, state.RedeemScript);

			var txnMgrSigner1 = new MultiSigSender(_loggerFactory.CreateLogger<MultiSigSender>(), txnCmd);
			txnMgrSigner1
				.AddSigner(new DefaultSigner(_relayer1.Ptekey));
				//.MultiSignPartial(txHash, state.RedeemScript);


			txnMgr
				.AddSigner(new DefaultSigner(_relayer1.Ptekey))
				.AddSigner(new DefaultSigner(_relayer2.Ptekey))
				//.MultiSign(raw, multisig.RedeemScript)
				.MultiSign(raw, state.RedeemScript)
				.Send()
				;

			// ASSERT
			//Console.WriteLine(txid);
		}


	}
}
