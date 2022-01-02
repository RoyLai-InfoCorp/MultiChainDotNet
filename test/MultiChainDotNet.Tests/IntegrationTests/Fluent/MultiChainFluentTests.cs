// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainToken;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Signers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.IntegrationTests.Fluent
{
	[TestFixture]
	public class MultiChainFluentTests : TestCommandFactoryBase
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
			_logger = _loggerFactory.CreateLogger<MultiChainFluentTests>();
		}

		[Test, Order(10)]
		public async Task Should_send_payment()
		{
			_stateDb.ClearState<TestState>();

			//Prepare
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			var result1 = await assetCmd.GetAddressBalancesAsync(_testUser1.NodeWallet);
			var balancesBefore = result1.Result.FirstOrDefault(x => String.IsNullOrEmpty(x.Name)).Raw;
			_logger.LogInformation("Balance before:" + balancesBefore.ToString());

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Pay(1_000_000)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			var result2 = await assetCmd.GetAddressBalancesAsync(_testUser1.NodeWallet);
			var balancesAfter = result2.Result.FirstOrDefault(x => String.IsNullOrEmpty(x.Name)).Raw;
			_logger.LogInformation("Balance after:" + balancesAfter.ToString());


			Assert.That(balancesAfter, Is.EqualTo(balancesBefore + 1_000_000));
		}

		[Test, Order(20)]
		public void Should_issue_asset()
		{
			//Prepare
			var assetName = RandomName();

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.IssueAsset(1000)
				.With()
					.IssueDetails(assetName, 1, true)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);

			// STATE
			_stateDb.SaveState(new TestState { AssetName = assetName });
		}

		[Test, Order(22)]
		public async Task Should_issue_non_fungible_asset()
		{
			//Prepare
			var nfaName = RandomName();
			var tokenCmd = _cmdFactory.CreateCommand<MultiChainTokenCommand>();

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_admin.NodeWallet)
					.IssueAsset(0)
				.With()
					.IssueNonFungibleAsset(nfaName)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT

			// Can be found on blockchain
			var info = await tokenCmd.GetNfaInfo(nfaName);
			Console.WriteLine("Info:" + info.Result.ToJson());
			info.IsError.Should().BeFalse();

			// Can be found in wallet
			var nfas = await tokenCmd.ListNfaAsync(_admin.NodeWallet);
			nfas.Result.Any(x => x.Name == nfaName).Should().BeTrue();

		}

		[Test, Order(22)]
		public async Task Should_issue_token()
		{
			//Prepare
			var nfaName = RandomName();
			var tokenCmd = _cmdFactory.CreateCommand<MultiChainTokenCommand>();
			await tokenCmd.IssueNfaAsync(_admin.NodeWallet, nfaName);
			var permCmd = _cmdFactory.CreateCommand<MultiChainPermissionCommand>();
			await permCmd.GrantPermissionAsync(_admin.NodeWallet, $"{nfaName}.issue");

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_admin.NodeWallet)
					.IssueToken(nfaName,"nft1",1)
				.With()
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			var bal = await tokenCmd.GetTokenBalancesAsync(_admin.NodeWallet);
			bal.Result[_admin.NodeWallet].Where(x => x.NfaName == nfaName).Count().Should().Be(1);
			bal.Result[_admin.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");
		}

		[Test, Order(24)]
		public async Task Should_send_token()
		{
			//Prepare
			var nfaName = RandomName();
			var tokenCmd = _cmdFactory.CreateCommand<MultiChainTokenCommand>();
			await tokenCmd.IssueNfaAsync(_admin.NodeWallet, nfaName);
			var permCmd = _cmdFactory.CreateCommand<MultiChainPermissionCommand>();
			await permCmd.GrantPermissionAsync(_admin.NodeWallet, $"{nfaName}.issue");
			await tokenCmd.IssueNftAsync(_admin.NodeWallet, nfaName, "nft1");

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.SendToken(nfaName, "nft1", 1)
				.With()
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			var bal = await tokenCmd.GetTokenBalancesAsync(_testUser1.NodeWallet);
			bal.Result[_testUser1.NodeWallet].Where(x => x.NfaName == nfaName).Count().Should().Be(1);
			bal.Result[_testUser1.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");
		}




		private async Task<string> GetAnnotationAsync(string assetName, string txid)
		{
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var txnResult = await txnCmd.GetRawTransaction(txid);
			if (txnResult.IsError)
				throw txnResult.Exception;

			var decodedResult = await txnCmd.DecodeRawTransactionAsync(txnResult.Result);
			var firstData = decodedResult.Result.Vout.Where(x => x.Assets.Any(y => y.Name == assetName) && x.Data is { }).FirstOrDefault()?.Data?[0];
			if (firstData is { })
			{
				var json = JToken.FromObject(firstData);
				var found = json.SelectToken("json");
				if (found is { })
					return found.ToString(Formatting.None);
			}
			return null;
		}

		private async Task<string> GetDeclarationAsync(string txid)
		{
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var txnResult = await txnCmd.GetRawTransaction(txid);
			if (txnResult.IsError)
				throw txnResult.Exception;

			var decodedResult = await txnCmd.DecodeRawTransactionAsync(txnResult.Result);
			var firstData = decodedResult.Result.Vout.Where(x => x.Assets is null && x.Data is { }).FirstOrDefault()?.Data?[0];
			if (firstData is { })
			{
				var json = JToken.FromObject(firstData);
				var found = json.SelectToken("json");
				if (found is { })
					return found.ToString(Formatting.None);
			}
			return null;
		}

		[Test, Order(22)]
		public async Task Should_issue_annotate_asset()
		{
			//Prepare
			var assetName = RandomName();

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.IssueAsset(1000)
					.AnnotateJson(new { Name = "Annotation" })
				.With()
					.IssueDetails(assetName, 1, true)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);

			var annotation = await GetAnnotationAsync(assetName, txid);
			Assert.That(annotation, Is.EqualTo("{\"Name\":\"Annotation\"}"));


			// STATE
			_stateDb.SaveState(new TestState { AssetName = assetName });
		}

		[Test, Order(24)]
		public async Task Should_issue_asset_with_declaration()
		{
			//Prepare
			var assetName = RandomName();

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.IssueAsset(1000)
				.With()
					.IssueDetails(assetName, 1, true)
					.DeclareJson(new { Name = "Declaration" })
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);

			var annotation = await GetDeclarationAsync(txid);
			Assert.That(annotation, Is.EqualTo("{\"Name\":\"Declaration\"}"));

		}

		[Test, Order(30)]
		public async Task Should_issue_more_asset()
		{
			//Prepare
			var assetName = RandomName();
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			await assetCmd.IssueAssetFromAsync(_admin.NodeWallet, _admin.NodeWallet, assetName, 1000, 1, true);

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.IssueMoreAsset(assetName, 1000)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
		}

		[Test, Order(40)]
		public async Task Should_send_asset()
		{
			//Prepare
			var assetName = RandomName();
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			await assetCmd.IssueAssetFromAsync(_admin.NodeWallet, _admin.NodeWallet, assetName, 1000, 1, true);

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.SendAsset(assetName, 100)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
		}

		//----- STREAMS

		[Test, Order(70)]
		public void Should_create_stream()
		{
			// PREPARE
			var streamName = RandomName();

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.With()
				.CreateStream(streamName, false)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);

			// STATE
			var state = new TestState { StreamName = streamName };
			_stateDb.SaveState(state);
		}

		[Test, Order(80)]
		public async Task Should_publish_streamitem()
		{
			var state = _stateDb.GetState<TestState>();

			// PREPARE
			var streamCmd = _cmdFactory.CreateCommand<MultiChainStreamCommand>();
			await streamCmd.SubscribeAsync(state.StreamName);

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.With()
				.PublishJson(state.StreamName, "lid-123456", new { name = "cow1", dob = DateTime.Parse("1-jan-2000"), age = 20 })
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;


			// ASSERT
			Assert.IsNotNull(txid);

		}

		[Test, Order(90)]
		public void Should_grant_stream_permission()
		{
			var state = _stateDb.GetState<TestState>();

			// PREPARE

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Permit($"write", state.StreamName)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
		}

		[Test, Order(100)]
		public void Should_grant_global_permission()
		{
			// PREPARE

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Permit("create,issue")
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
		}

		[Test, Order(110)]
		public async Task Should_revoke_global_permission()
		{
			// PREPARE

			// ACT
			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Revoke("create,issue")
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
		}

		// USE UTXO

		[Test, Order(120)]
		public async Task Should_issue_more_asset_from_unspent()
		{
			//Prepare
			var assetName = "openasset";

			// ACT
			await Task.Delay(3000);
			var unspents = await _txnCmd.ListUnspentAsync(_admin.NodeWallet);
			Console.WriteLine("UTXO before:" + JsonConvert.SerializeObject(unspents.Result));

			//var unspent = unspents.Result.First(x => x.Amount >= 1000);
			var unspent = unspents.Result.First();
			Console.WriteLine("Unspent selected:" + JsonConvert.SerializeObject(unspent));

			var txid = new MultiChainFluent()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.From(unspent.TxId, unspent.Vout)
				.To(_testUser1.NodeWallet)
					.IssueMoreAsset(assetName, 1000)
				.CreateNormalTransaction(_txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);

			unspents = await _txnCmd.ListUnspentAsync(_admin.NodeWallet);
			Console.WriteLine("UTXO after:" + JsonConvert.SerializeObject(unspents.Result));

		}

	}
}
