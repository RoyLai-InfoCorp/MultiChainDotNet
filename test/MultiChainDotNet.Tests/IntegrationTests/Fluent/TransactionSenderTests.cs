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
	public class TransactionSenderTests : TestCommandFactoryBase
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
			_logger = _loggerFactory.CreateLogger<TransactionSenderTests>();
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
			_logger.LogInformation("Balance before:"+balancesBefore.ToString());

			// ACT
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
				.Pay(1_000_000);
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var result = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
				.Send()
				;

			// ASSERT
			var result2 = await assetCmd.GetAddressBalancesAsync(_testUser1.NodeWallet);
			var balancesAfter = result2.Result.FirstOrDefault(x => String.IsNullOrEmpty(x.Name)).Raw;
			_logger.LogInformation("Balance after:" + balancesAfter.ToString());


			Assert.That(balancesAfter, Is.EqualTo(balancesBefore + 1_000_000));
		}

		//----- ASSETS

		[Test, Order(20)]
		public void Should_issue_asset()
		{
			//Prepare
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 20);

			// ACT
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
				.IssueAsset(1000)
				;
			requestor
				.With()
				.IssueDetails(assetName, 1, true)
				//.Pay(MultiChainConfiguration.MinimumTxnFee)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd); 
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
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
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
				.IssueAsset(1000)
				.AnnotateJson(new { Name = "Annotation" })
				;
			requestor
				.With()
				.IssueDetails(assetName, 1, true)
				//.Pay(MultiChainConfiguration.MinimumTxnFee)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
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
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
				.IssueAsset(1000)
				;
			requestor
				.With()
				.IssueDetails(assetName, 1, true)
				.DeclareJson(new { Name = "Declaration"})
				//.Pay(MultiChainConfiguration.MinimumTxnFee)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
				.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);

			var annotation = await GetDeclarationAsync(txid);
			Assert.That(annotation, Is.EqualTo("{\"Name\":\"Declaration\"}"));


			// STATE
			_stateDb.SaveState(new TestState { AssetName = assetName });
		}



		[Test, Order(30)]
		public async Task Should_issue_more_asset()
		{
			var state = _stateDb.GetState<TestState>();

			//Prepare
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>(); 
			var assetName = state.AssetName;

			// ACT
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
				.IssueMoreAsset(assetName, 1000)
				//.Pay(MultiChainConfiguration.MinimumTxnFee)
				;
			var raw = requestor.Request(txnCmd);

			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
			.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
				.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
		}

		[Test, Order(40)]
		public async Task Should_send_asset()
		{
			var state = _stateDb.GetState<TestState>();

			//Prepare
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var assetName = state.AssetName;

			// ACT
			var requestor = new TransactionRequestor();
			requestor
				.From(_testUser1.NodeWallet)
				.To(_admin.NodeWallet)
				.SendAsset(assetName, 100)
				//.Pay(MultiChainConfiguration.MinimumTxnFee)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_testUser1.Ptekey))
				.Sign(raw)
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
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.With()
				.CreateStream(streamName,false)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
				.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);

			// STATE
			var state = new TestState { StreamName = streamName };
			_stateDb.SaveState(state);
		}

		[Test,Order(80)]
		public async Task Should_publish_streamitem()
		{
			var state = _stateDb.GetState<TestState>();

			// PREPARE
			var txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			var streamCmd = _cmdFactory.CreateCommand<MultiChainStreamCommand>();
			await streamCmd.SubscribeAsync(state.StreamName);

			// ACT
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.With()
				.PublishJson(state.StreamName,"lid-123456", new { name = "cow1", dob = DateTime.Parse("1-jan-2000"), age = 20 })
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
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
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Permit($"write",state.StreamName)
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
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
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Permit("create,issue")
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
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
			var requestor = new TransactionRequestor();
			requestor
				.From(_admin.NodeWallet)
				.To(_testUser1.NodeWallet)
					.Revoke("create,issue")
				;
			var raw = requestor.Request(txnCmd);
			var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), txnCmd);
			var txid = txnMgr
				.AddSigner(new DefaultSigner(_admin.Ptekey))
				.Sign(raw)
				.Send()
				;

			// ASSERT
			Assert.IsNotNull(txid);
		}


	}
}
