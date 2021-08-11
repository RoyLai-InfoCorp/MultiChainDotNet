using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
    public class MultiChainAssetManagerTests: TestCommandFactoryBase
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
		}

		[SetUp]
		public void Setup()
		{
			_assetManager = _container.GetRequiredService<IMultiChainAssetManager>();
			_txnManager = _container.GetRequiredService<IMultiChainTransactionManager>();
		}

		const UInt64 SEND_TXN_FEE_AT_LEAST = 300;
		const UInt64 SEND_TXN_FEE_AT_MOST = 400;

		[Test, Order(20)]
		public async Task should_be_able_to_pay_with_metadata()
		{
			//Prepare
			var senderBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet)).Result.Raw;
			var receiverBalBefore = _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet).Result.Result.Raw;

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var result = await _assetManager.PayAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _testUser1.NodeWallet, 1_000_000, payload);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;

			// Assert fees
			var receiverBalAfter = _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet).Result.Result.Raw;
			Assert.That(receiverBalAfter, Is.EqualTo(receiverBalBefore + 1_000_000), "Receiver balance doesn't match");
			var senderBalAfter = _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet).Result.Result.Raw;
			Assert.That(senderBalAfter, 
				Is
				.AtLeast(senderBalBefore - 1_000_000 - SEND_TXN_FEE_AT_MOST)
				.And
				.AtMost(senderBalBefore - 1_000_000 - SEND_TXN_FEE_AT_LEAST), 
				$"sender paid extra for fee {senderBalBefore-senderBalAfter - 1_000_000}");

			// Assert payload
			var metadata = await _txnManager.GetDeclarationAsync(txid);
			Assert.That(metadata.Result, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}

		const UInt64 SENDASSET_TXN_FEE_AT_LEAST = 500;
		const UInt64 SENDASSET_TXN_FEE_AT_MOST = 600;
		[Test, Order(20)]
		public async Task should_be_able_to_sendasset_with_metadata()
		{
			//Prepare
			var assetName = "openasset";
			await _assetManager.IssueMoreAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _admin.NodeWallet, assetName, 1_000);
			var senderAssetBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet, assetName)).Result.Raw;
			var receiverAssetBalBefore = _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, assetName).Result?.Result?.Raw ?? 0;
			var senderBalBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet)).Result.Raw;

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var result = await _assetManager.SendAssetAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _testUser1.NodeWallet, assetName, 1_000, payload);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;

			// Assert Fees
			var senderBalAfter = _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet).Result.Result.Raw;
			Assert.That(senderBalAfter,
				Is
				.AtLeast(senderBalBefore - SENDASSET_TXN_FEE_AT_MOST)
				.And
				.AtMost(senderBalBefore - SENDASSET_TXN_FEE_AT_LEAST),
				$"sender paid extra for fee {senderBalBefore - senderBalAfter}");

			// Assert balance
			var senderAssetBalAfter = _assetManager.GetAssetBalanceByAddressAsync(_admin.NodeWallet, assetName).Result?.Result?.Raw ?? 0;
			var receiverAssetBalAfter = _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, assetName).Result.Result.Raw;
			Assert.That(receiverAssetBalAfter, Is.EqualTo(receiverAssetBalBefore + 1000), "Receiver balance doesn't match");
			Assert.That(senderAssetBalAfter, Is.EqualTo(senderAssetBalBefore - 1000), $"sender paid extra for fee{senderAssetBalBefore - senderAssetBalAfter - 1000 * 1_000_000}");

			// Assert payload
			var metadata = await _txnManager.GetDeclarationAsync(txid);
			Assert.That(metadata.Result, Is.EqualTo(JsonConvert.SerializeObject(payload)));
			
		}

		[Test, Order(30)]
		public async Task Should_be_able_to_issue_asset_with_metadata()
		{
			//Prepare
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var result = await _assetManager.IssueAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _testUser1.NodeWallet, assetName, 1_000_000, true, payload);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;

			// Assert balance
			var receiverBalAfter = _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, assetName).Result.Result.Raw;
			Assert.That(receiverBalAfter, Is.EqualTo(1_000_000), $"Receiver current balance {receiverBalAfter}");

			// Assert payload
			var metadata = await _txnManager.GetDeclarationAsync(txid);
			Assert.That(metadata.Result, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}

		[Test, Order(30)]
		public async Task Should_be_able_to_issue_more_asset_with_metadata()
		{
			//Prepare
			var assetInfo = await _assetManager.GetAssetInfoAsync("openasset");
			var balresultBefore = (await _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, "openasset")).Result;
			var receiverBalBefore = balresultBefore.Raw;

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var result = await _assetManager.IssueMoreAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _testUser1.NodeWallet, "openasset", 1_000_000, payload);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;

			var receiverBalAfter = _assetManager.GetAssetBalanceByAddressAsync(_testUser1.NodeWallet, "openasset").Result.Result.Raw;
			Assert.That(receiverBalAfter, Is.EqualTo(receiverBalBefore + 1_000_000), $"Receiver current balance {receiverBalAfter}");

			// Assert payload
			var metadata = await _txnManager.GetDeclarationAsync(txid);
			Assert.That(metadata.Result, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}

		[Test]
		public async Task Should_not_throw_exception_when_spending_output_with_non_inline_metadata()
		{
			//Prepare
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var issueResult = await _assetManager.IssueAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _admin.NodeWallet, assetName, 1, true, payload);

			// ACT
			var result = await _assetManager.SendAssetAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _testUser1.NodeWallet, assetName, 1);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var comment = await _txnManager.GetDeclarationAsync(issueResult.Result);
			Assert.That(comment.Result, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}


		[Test]
		public async Task Should_throw_exception_when_spending_output_with_inline_metadata()
		{
			//Prepare
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var issueResult = await _assetManager.IssueAnnotateAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _admin.NodeWallet, assetName, 1, true, payload);

			// ACT
			var result = await _assetManager.SendAssetAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _testUser1.NodeWallet, assetName, 1);

			// ASSERT
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ExceptionMessage, Contains.Substring("There are outputs with inline metadata"));
			Console.WriteLine(result.Exception.ToString());

			var comment = await _txnManager.GetAnnotationAsync(assetName, issueResult.Result);
			Assert.That(comment.Result, Is.EqualTo(JsonConvert.SerializeObject(payload)));

		}

		[Test]
		public async Task Should_send_annotated_asset()
		{
			//Prepare
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var issueResult = await _assetManager.IssueAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _admin.NodeWallet, assetName, 1, true);

			// ACT
			var result = await _assetManager.SendAnnotateAssetAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, _testUser1.NodeWallet, assetName, 1, payload);

			// ASSERT
			Assert.That(result.IsError, Is.False,result.ExceptionMessage);

			var comment = await _txnManager.GetAnnotationAsync(assetName, result.Result);
			Assert.That(comment.Result, Is.EqualTo(JsonConvert.SerializeObject(payload)));

		}

		[Test]
		public async Task Should_not_be_able_to_create_transaction_when_multisig_address_has_no_fund()
		{
			var addressMgr = _container.GetRequiredService<IMultiChainAddressManager>();
			var multisigResult = await addressMgr.CreateMultiSigAsync(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });
			var multisig = multisigResult.Result.Address;
			var redeemScript = multisigResult.Result.RedeemScript;
			var balanceResult = await _assetManager.GetAssetBalanceByAddressAsync(multisig, "");
			if (balanceResult.Result.Raw < 1000)
			{
				var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
				var issueResult = await _assetManager.IssueAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, multisig, assetName, 10, true);

				// ACT
				var signatureResult = _assetManager.CreateSignatureSlipAsync(multisig, _testUser2.NodeWallet, assetName, 1);
				Assert.That(((MultiChainException)signatureResult.Exception).Code, Is.EqualTo(MultiChainErrorCode.RPC_WALLET_INSUFFICIENT_FUNDS));
			}
		}


		[Test]
		public async Task Should_be_able_to_multisign_send_asset_transaction()
		{
			var addressMgr = _container.GetRequiredService<IMultiChainAddressManager>();
			var multisigResult = await addressMgr.CreateMultiSigAsync(2, new string[] { _relayer1.Pubkey, _relayer2.Pubkey, _relayer3.Pubkey });

			var multisig = multisigResult.Result.Address;
			var redeemScript = multisigResult.Result.RedeemScript;
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			await _assetManager.IssueAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, multisig, assetName, 10, true);
			var balanceResult = await _assetManager.GetAssetBalanceByAddressAsync(multisig, "");
			if (balanceResult.Result.Raw < 1000)
			{
				await _assetManager.PayAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, multisig, 5000);
			}

			// ACT
			var signatureResult = _assetManager.CreateSignatureSlipAsync(multisig, _testUser2.NodeWallet, assetName, 1);
			Assert.That(signatureResult.IsError, Is.False, signatureResult.ExceptionMessage);
			var signatureSlip = signatureResult.Result;

			var relayer1Sig = _assetManager.SignMultiSig(new DefaultSigner(_relayer1.Ptekey), signatureSlip, redeemScript);
			var relayer2Sig = _assetManager.SignMultiSig(new DefaultSigner(_relayer1.Ptekey), signatureSlip, redeemScript);
			var result = _assetManager.SendMultiSigAssetAsync(new List<string[]> { relayer1Sig.Result, relayer2Sig.Result }, signatureSlip, redeemScript);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

	}
}
