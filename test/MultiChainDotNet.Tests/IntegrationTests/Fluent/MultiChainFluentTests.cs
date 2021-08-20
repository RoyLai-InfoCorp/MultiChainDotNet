using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainTransaction;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Fluent.Builders;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainAddress;
using System.Linq;
using MultiChainDotNet.Fluent;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);

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
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);

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
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);

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
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);
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
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);
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
			var streamName = Guid.NewGuid().ToString("N").Substring(0, 20);

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


	}
}
