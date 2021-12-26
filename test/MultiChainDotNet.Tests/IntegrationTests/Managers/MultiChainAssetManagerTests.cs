// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
	public class MultiChainAssetManagerTests : TestCommandFactoryBase
	{
		IMultiChainAssetManager _assetManager;
		IMultiChainTransactionManager _txnManager;

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
			services.AddSingleton<SignerBase>(_ => new DefaultSigner(_admin.Ptekey));
		}

		[SetUp]
		public void Setup()
		{
			_assetManager = _container.GetRequiredService<IMultiChainAssetManager>();
			_txnManager = _container.GetRequiredService<IMultiChainTransactionManager>();
		}

		const UInt64 SEND_TXN_FEE_AT_LEAST = 0;
		const UInt64 SEND_TXN_FEE_AT_MOST = 3000;

		[Test, Order(20)]
		public async Task Should_be_able_to_pay_with_metadata()
		{
			//Prepare
			var senderBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet))?.Raw ?? 0;
			var receiverBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet))?.Raw ?? 0;

			// ACT
			if (senderBalBefore < 1_000_000)
				throw new Exception("Sender does not have enough native crypto to send.");
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var txid = _assetManager.Pay(_testUser1.NodeWallet, 1_000_000, payload);

			// ASSERT

			// Assert fees
			var receiverBalAfter = (await _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet)).Raw;
			Assert.That(receiverBalAfter, Is.EqualTo(receiverBalBefore + 1_000_000), "Receiver balance doesn't match");
			var senderBalAfter = (await _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet)).Raw;
			Assert.That(senderBalAfter,
				Is
				.AtLeast(senderBalBefore - 1_000_000 - SEND_TXN_FEE_AT_MOST)
				.And
				.AtMost(senderBalBefore - 1_000_000 - SEND_TXN_FEE_AT_LEAST),
				$"sender paid extra for fee {senderBalBefore - senderBalAfter - 1_000_000}");

			// Assert payload
			var metadata = await _txnManager.GetDeclarationAsync(txid);
			Assert.That(metadata, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}

		const UInt64 SENDASSET_TXN_FEE_AT_LEAST = 0;
		const UInt64 SENDASSET_TXN_FEE_AT_MOST = 3000;
		[Test, Order(30)]
		public async Task Should_be_able_to_sendasset_with_metadata()
		{
			//Prepare
			var assetName = "openasset";
			_assetManager.IssueMore(_admin.NodeWallet, assetName, 1_000);
			var senderAssetBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet, assetName)).Raw;
			var receiverAssetBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, assetName))?.Raw ?? 0;
			var senderBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet)).Raw;

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var txid = _assetManager.SendAsset(_testUser1.NodeWallet, assetName, 1_000, payload);

			// ASSERT

			// Assert Fees
			var senderBalAfter = _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet).Result.Raw;
			Assert.That(senderBalAfter,
				Is
				.AtLeast(senderBalBefore - SENDASSET_TXN_FEE_AT_MOST)
				.And
				.AtMost(senderBalBefore - SENDASSET_TXN_FEE_AT_LEAST),
				$"sender paid extra for fee {senderBalBefore - senderBalAfter}");

			// Assert balance
			var senderAssetBalAfter = (await _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet, assetName))?.Raw ?? 0;
			var receiverAssetBalAfter = (await _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, assetName)).Raw;
			Assert.That(receiverAssetBalAfter, Is.EqualTo(receiverAssetBalBefore + 1000), "Receiver balance doesn't match");
			Assert.That(senderAssetBalAfter, Is.EqualTo(senderAssetBalBefore - 1000), $"sender paid extra for fee{senderAssetBalBefore - senderAssetBalAfter - 1000 * 1_000_000}");

			// Assert payload
			var metadata = await _txnManager.GetDeclarationAsync(txid);
			Assert.That(metadata, Is.EqualTo(JsonConvert.SerializeObject(payload)));

		}

		[Test, Order(40)]
		public async Task Should_be_able_to_issue_asset_with_metadata()
		{
			//Prepare
			var assetName = RandomName();

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var txid = _assetManager.Issue(_testUser1.NodeWallet, assetName, 1_000_000, true, payload);

			// ASSERT

			// Assert balance
			var receiverBalAfter = (await _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, assetName)).Raw;
			Assert.That(receiverBalAfter, Is.EqualTo(1_000_000), $"Receiver current balance {receiverBalAfter}");

			// Assert payload
			var metadata = await _txnManager.GetDeclarationAsync(txid);
			Assert.That(metadata, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}

		[Test, Order(50)]
		public async Task Should_be_able_to_issue_more_asset_with_metadata()
		{
			//Prepare
			var assetInfo = await _assetManager.GetAssetInfoAsync("openasset");
			var receiverBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, "openasset")).Raw;

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var txid = _assetManager.IssueMore(_testUser1.NodeWallet, "openasset", 1_000_000, payload);

			// ASSERT

			var receiverBalAfter = (await _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, "openasset")).Raw;
			Assert.That(receiverBalAfter, Is.EqualTo(receiverBalBefore + 1_000_000), $"Receiver current balance {receiverBalAfter}");

			// Assert payload
			var metadata = await _txnManager.GetDeclarationAsync(txid);
			Assert.That(metadata, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}

		[Test]
		public async Task Should_not_throw_exception_when_spending_output_with_non_inline_metadata()
		{
			//Prepare
			var assetName = RandomName();
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var issueResult = _assetManager.Issue(_admin.NodeWallet, assetName, 1, true, payload);

			// ACT
			var result = _assetManager.SendAsset(_testUser1.NodeWallet, assetName, 1);

			// ASSERT
			var comment = await _txnManager.GetDeclarationAsync(issueResult);
			Assert.That(comment, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}


		[Test]
		public async Task Should_throw_exception_when_spending_output_with_inline_metadata()
		{
			//Prepare
			var assetName = RandomName();
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var issueResult = _assetManager.IssueAnnotate(_admin.NodeWallet, assetName, 1, true, payload);

			// ACT
			Action action = () => _assetManager.SendAsset(_testUser1.NodeWallet, assetName, 1);
			action.Should().Throw<Exception>().WithMessage("*There are outputs with inline metadata*");

			var comment = await _txnManager.GetAnnotationAsync(assetName, issueResult);
			Assert.That(comment, Is.EqualTo(JsonConvert.SerializeObject(payload)));

		}

		[Test, Order(60)]
		public async Task Should_send_annotated_asset()
		{
			//Prepare
			var assetName = RandomName();
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var issueResult = _assetManager.Issue(_admin.NodeWallet, assetName, 1, true);

			// ACT
			var result = await _assetManager.SendAnnotateAssetAsync(_testUser1.NodeWallet, assetName, 1, payload);

			// ASSERT

			var comment = await _txnManager.GetAnnotationAsync(assetName, result);
			Assert.That(comment, Is.EqualTo(JsonConvert.SerializeObject(payload)));

		}


	}
}
