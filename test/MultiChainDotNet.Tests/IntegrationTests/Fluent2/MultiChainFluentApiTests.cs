using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Tests.IntegrationTests.Fluent;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Builders2;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Core.MultiChainAsset;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.Utils;

namespace MultiChainDotNet.Tests.IntegrationTests.Fluent2
{
	[TestFixture]
	public class MultiChainFluentApiTests : TestCommandFactoryBase
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

		[SetUp]
		public async Task SetUp()
		{
			_cmdFactory = _container.GetRequiredService<IMultiChainCommandFactory>();
			_logger = _loggerFactory.CreateLogger<MultiChainFluentApi>();
		}

		[Test, Order(10)]
		public async Task Should_send_payment()
		{
			_stateDb.ClearState<TestState>();

			//Prepare
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			var result1 = await assetCmd.GetAddressBalancesAsync(_testUser1.NodeWallet);
			var balancesBefore = result1.Result.FirstOrDefault(x => String.IsNullOrEmpty(x.Name)).Raw;
			_logger.LogInformation("Balance before:" + balancesBefore.ToString());

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Pay(1_000_000)
				.CreateTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.IssueAsset(1000)
				.With()
					.IssueDetails(assetName,1,true)
				.CreateNormalTransaction(txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);

			// STATE
			_stateDb.SaveState(new TestState { AssetName = assetName });
		}

		public async Task<string> GetAnnotationAsync(string assetName, string txid)
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

		public async Task<string> GetDeclarationAsync(string txid)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.IssueAsset(1000)
					.AnnotateJson(new { Name = "Annotation" })
				.With()
					.IssueDetails(assetName, 1, true)
				.CreateNormalTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.IssueAsset(1000)
				.With()
					.IssueDetails(assetName, 1, true)
					.DeclareJson(new { Name = "Declaration" })
				.CreateNormalTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			await assetCmd.IssueAssetFromAsync(_admin.NodeWallet, _admin.NodeWallet, assetName, 1000, 1, true);

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.IssueMoreAsset(assetName, 1000)
				.CreateTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			await assetCmd.IssueAssetFromAsync(_admin.NodeWallet,_admin.NodeWallet,assetName,1000,1,true);

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.SendAsset(assetName, 100)
				.CreateTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var streamName = Guid.NewGuid().ToString("N").Substring(0, 20);

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.With()
				.CreateStream(streamName, false)
				.CreateNormalTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var streamCmd = _cmdFactory.CreateCommand<MultiChainStreamCommand>();
			await streamCmd.SubscribeAsync(state.StreamName);

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.With()
				.PublishJson(state.StreamName, "lid-123456", new { name = "cow1", dob = DateTime.Parse("1-jan-2000"), age = 20 })
				.CreateNormalTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Permit($"write", state.StreamName)
				.CreateTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Permit("create,issue")
				.CreateTransaction(txnCmd)
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
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Revoke("create,issue")
				.CreateTransaction(txnCmd)
					.AddSigner(new DefaultSigner(_admin.Ptekey))
					.Sign()
					.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
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

		[Test, Order(210)]
		public async Task Should_send_asset_with_1_of_2_multisignature()
		{
			await Prepare_MultiSigAddress(1, new string[] { _relayer1.Pubkey, _relayer2.Pubkey });
			var state = _stateDb.GetState<TestState>();

			// PREPARE
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetCmd = _cmdFactory.CreateCommand<MultiChainAssetCommand>();
			var balance = await assetCmd.GetAddressBalancesAsync(state.MultiSigAddress);

			// ACT
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(state.MultiSigAddress)
				.To(_testUser1.NodeWallet)
					.SendAsset(state.AssetName, 1)
				.CreateMultiSigTransaction(txnCmd)
					.AddSigner(new DefaultSigner(_relayer1.Ptekey))
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
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(state.MultiSigAddress)
				.To(_testUser1.NodeWallet)
					.SendAsset(state.AssetName, 1)
				.CreateMultiSigTransaction(txnCmd)
					.AddSigner(new DefaultSigner(_relayer1.Ptekey))
					.AddSigner(new DefaultSigner(_relayer2.Ptekey))
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

			// PREPARE
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();

			// ACT 1
			var request = new MultiChainFluentApi()
				.AddLogger(_logger)
				.From(state.MultiSigAddress)
				.To(_testUser1.NodeWallet)
					.SendAsset(state.AssetName, 1)
					.CreateRawTransaction(txnCmd);

			// ACT 2
			var signatures1 = new MultiChainFluentApi()
				.AddLogger(_logger)
				.UseMultiStageMultiSig()
				.AddMultiSigRawTransaction(request)
				.AddTransactionCommand(txnCmd)
				.AddSigner(new DefaultSigner(_relayer1.Ptekey))
				.MultiSignPartial(state.RedeemScript);

			var signatures2 = new MultiChainFluentApi()
				.AddLogger(_logger)
				.UseMultiStageMultiSig()
				.AddMultiSigRawTransaction(request)
				.AddTransactionCommand(txnCmd)
				.AddSigner(new DefaultSigner(_relayer2.Ptekey))
				.MultiSignPartial(state.RedeemScript);

			// ACT 3
			var txid = new MultiChainFluentApi()
				.AddLogger(_logger)
				.UseMultiSigSubmit()
				.AddMultiSigRawTransaction(request)
				.AddTransactionCommand(txnCmd)
				.AddSignatures(new List<string[]> { signatures1, signatures2 })
				.CreateTransaction(state.RedeemScript)
				.Send();

			Assert.That(String.IsNullOrEmpty(txid), Is.False);
		}


	}
}
