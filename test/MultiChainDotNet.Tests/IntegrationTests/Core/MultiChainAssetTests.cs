using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
	[TestFixture]
    public class MultiChainAssetTests : TestBase
    {
		MultiChainAssetCommand _assetCmd;
		MultiChainTransactionCommand _txnCmd;
		MultiChainAddressCommand _addrCmd;
		MultiChainPermissionCommand _permCmd;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services.AddTransient<MultiChainAssetCommand>();
			services.AddTransient<MultiChainPermissionCommand>();
			services.AddTransient<MultiChainAddressCommand>();
			services.AddTransient<MultiChainTransactionCommand>();
		}

		[SetUp]
		public async Task SetUp()
		{
			_assetCmd = _container.GetRequiredService<MultiChainAssetCommand>();
			_txnCmd = _container.GetRequiredService<MultiChainTransactionCommand>();
			_addrCmd = _container.GetRequiredService<MultiChainAddressCommand>();
			_permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();

			//await Task.Delay(2000);
		}

		public async Task<string> GetMetaDataAsync(string txid)
		{
			var txnResult = await _txnCmd.GetRawTransaction(txid);
			if (txnResult.IsError)
				throw txnResult.Exception;

			var decodedResult = await _txnCmd.DecodeRawTransactionAsync(txnResult.Result);
			var firstData = decodedResult.Result.Vout.Where(x => x.Data is { }).FirstOrDefault()?.Data?[0];
			var json = JObject.FromObject(firstData);
			var found = json.SelectToken("json");
			return found.ToString(Formatting.None);
		}

		public async Task<UInt64?> GetRawBalance(string address, string assetName = null)
		{
			var list = (await _assetCmd.GetAddressBalancesAsync(address)).Result;
			if (list is null)
				return 0;

			if (String.IsNullOrEmpty(assetName))
			{
				var nativeCurrency = list.FirstOrDefault(x => String.IsNullOrEmpty(x.Name));
				if (nativeCurrency is null)
					return null;
				return nativeCurrency.Raw;
			}

			var  asset = list.FirstOrDefault(x => x.Name == assetName);
			var assetInfo = await _assetCmd.GetAssetInfoAsync(assetName);
			
			return assetInfo.Result is null || asset is null ? null : Convert.ToUInt64(asset.Qty * assetInfo.Result.Multiple);
		}

		[Test]
		public async Task should_be_able_to_send_with_inline_metadata()
		{
			var balBefore = await GetRawBalance(_testUser1.NodeWallet);

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var result = await _assetCmd.SendFromAsync(_admin.NodeWallet,_testUser1.NodeWallet, 1_000, "", payload);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;

			// Assert balance
			var balAfter = await GetRawBalance(_testUser1.NodeWallet);
			Assert.That(balAfter, Is.EqualTo(balBefore + 1_000));

			// Assert payload
			var metadata = await GetMetaDataAsync(txid);
			Assert.That(metadata, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}

		[Test]
		public async Task Should_be_able_to_send_asset_with_inline_metadata()
		{
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 10);
			await _assetCmd.IssueAssetFromAsync(_admin.NodeWallet, _admin.NodeWallet, assetName, 1000, 0.01, true);

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var result = await _assetCmd.SendFromAsync(_admin.NodeWallet, _testUser1.NodeWallet, 10, assetName, payload);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;

			// Assert balance
			var balAfter = await GetRawBalance(_testUser1.NodeWallet, assetName);
			//Assert.That(balAfter, Is.EqualTo(balBefore + 10));
			Assert.That(balAfter, Is.EqualTo(1000));

			// Assert payload
			var metadata = await GetMetaDataAsync(txid);
			Assert.That(metadata, Is.EqualTo(JsonConvert.SerializeObject(payload)));
		}

		[Test]
		public async Task Should_have_permission_to_send_asset_by_default()
		{
			var newSender = await _addrCmd.GetNewAddressAsync();
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 10);

			await _assetCmd.IssueAssetAsync(newSender.Result, assetName, 10, 0.001, true);

			// ACT			
			var result = await _assetCmd.SendAssetFromAsync(newSender.Result, _testUser1.NodeWallet, assetName, 10);
			Assert.That(result.IsError, Is.True, result.ExceptionMessage);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_INSUFFICIENT_PERMISSIONS));
			Assert.That(result.ExceptionMessage, Contains.Substring("doesn't have send permission"));
		}

		[Test]
		public async Task should_be_able_to_send_with_non_inline_metadata()
		{
			var balBefore = await GetRawBalance(_testUser1.NodeWallet);

			// ACT
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var result = await _assetCmd.SendWithDataFromAsync(_admin.NodeWallet, _testUser1.NodeWallet, 1_000, payload);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;

			// Assert balance
			var balAfter = await GetRawBalance(_testUser1.NodeWallet);
			Assert.That(balAfter, Is.EqualTo(balBefore + 1_000));

			// Assert payload
			var metadata = await GetMetaDataAsync(txid);
			Assert.That(metadata, Is.EqualTo(JsonConvert.SerializeObject(payload)));

		}


		[Test]
        public async Task Should_be_able_to_issue_asset()
		{
			var assetName = Guid.NewGuid().ToString("N").Substring(0,10);
			var result = await _assetCmd.IssueAssetFromAsync(_admin.NodeWallet, _testUser1.NodeWallet, assetName,10,0.001,true);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			var bal = await GetRawBalance(_testUser1.NodeWallet, assetName);
			Assert.That(bal, Is.EqualTo(10_000));
			
		}

		[Test]
		public async Task Should_be_able_to_issue_more_asset()
		{
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 10);
			var issueResult = await _assetCmd.IssueAssetFromAsync(_admin.NodeWallet, _admin.NodeWallet, assetName, 10, 0.001, true);
			
			// ACT
			var result = await _assetCmd.IssueMoreAssetFromAsync(_admin.NodeWallet, _testUser1.NodeWallet, assetName, 10);

			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var bal = await GetRawBalance(_testUser1.NodeWallet, assetName);
			Assert.That(bal, Is.EqualTo(10_000));
		}

		[Test]
		public async Task Should_not_be_able_to_issue_closed_asset()
		{
			var assetName = "closeasset";
			var result = await _assetCmd.IssueMoreAssetAsync(_admin.NodeWallet, assetName, 10);
			Assert.That(result.IsError, Is.True, result.ExceptionMessage);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_NOT_ALLOWED));
			Assert.That(result.ExceptionMessage, Contains.Substring("Issuing more units not allowed for this asset: closeasset"));
		}

		[Test]
		public async Task Should_be_able_to_subscribe()
		{
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 10);
			await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName, 10, 0.001, true);

			// ACT			
			var result = await _assetCmd.SubscribeAsync(assetName);

			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}



		[Test]
		public async Task Should_not_be_able_to_send_asset_with_insufficient_funds()
		{
			var newSender = (await _addrCmd.GetNewAddressAsync()).Result;
			await _permCmd.GrantPermissionAsync(newSender, "send");
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 10);
			await _assetCmd.IssueAssetAsync(newSender, assetName, 10, 0.001, true);

			// ACT			
			var result = await _assetCmd.SendAssetFromAsync(newSender, _testUser1.NodeWallet, assetName, 10);
			Assert.That(result.IsError, Is.True, result.ExceptionMessage);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_WALLET_INSUFFICIENT_FUNDS));
			Assert.That(result.ExceptionMessage, Contains.Substring("Insufficient funds"));
		}

		[Test]
		public async Task Should_be_able_to_send_asset_with_sufficient_permission_and_fund()
		{
			var newSender = (await _addrCmd.GetNewAddressAsync()).Result;
			await _permCmd.GrantPermissionAsync(newSender, "send");
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 10);
			await _assetCmd.IssueAssetAsync(newSender, assetName, 10, 0.001, true);
			await _assetCmd.SendAsync(newSender, 1_000);
			var senderBalanceBefore = await GetRawBalance(newSender);

			// ACT			
			var txResult = await _assetCmd.SendAssetFromAsync(newSender, _testUser1.NodeWallet, assetName, 10);

			// ASSERT
			Assert.That(txResult.IsError, Is.False, txResult.ExceptionMessage);

			// Sender's fee is spent
			var senderBalanceAfter = await GetRawBalance(newSender);
			Assert.That(senderBalanceBefore - senderBalanceAfter, Is.AtMost(500));

			// Recipient's asset is increased
			var receipientAssetAfter = await GetRawBalance(_testUser1.NodeWallet, assetName);
			Assert.That(receipientAssetAfter, Is.EqualTo(10_000));

		}

		[Test]
		public async Task Should_be_able_to_list_assets()
		{
			var assetName1 = Guid.NewGuid().ToString("N").Substring(0, 10);
			var txid1 = await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName1, 10, 0.001, true);
			var assetName2 = Guid.NewGuid().ToString("N").Substring(0, 10);
			var txid2 = await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName2, 10, 0.001, true);
			var assetName3 = Guid.NewGuid().ToString("N").Substring(0, 10);
			var txid3 = await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName3, 10, 0.001, true);

			// ACT			
			var result = await _assetCmd.ListAssetsAsync();

			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txids = result.Result.Select(x => x.Name).ToArray();
			Assert.That(txids, Has.Member(assetName1));
			Assert.That(txids, Has.Member(assetName2));
			Assert.That(txids, Has.Member(assetName3));

		}

		[Test]
		public async Task Should_be_able_to_get_asset_by_name()
		{
			var assetName1 = Guid.NewGuid().ToString("N").Substring(0, 10);
			var txid1 = await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName1, 10, 0.001, true);
			var assetName2 = Guid.NewGuid().ToString("N").Substring(0, 10);
			var txid2 = await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName2, 10, 0.001, true);
			var assetName3 = Guid.NewGuid().ToString("N").Substring(0, 10);
			var txid3 = await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName3, 10, 0.001, true);

			// ACT			
			var result = await _assetCmd.ListAssetsAsync(assetName1);

			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Assert.That(result.Result.Count, Is.EqualTo(1));
			Assert.That(result.Result[0].Name, Is.EqualTo(assetName1));

		}

		[Test]
		public async Task Should_be_able_to_get_balances_by_address()
		{
			var assetName1 = Guid.NewGuid().ToString("N").Substring(0, 10);
			await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName1, 10, 0.001, true);

			// ACT			
			var result = await _assetCmd.GetAddressBalancesAsync(_admin.NodeWallet);

			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var asset = result.Result.FirstOrDefault(x => x.Name == assetName1);
			Assert.That(asset.Qty, Is.EqualTo(10));
		}

		[Test]
		public async Task Should_be_able_to_list_transactions_of_an_asset()
		{
			var assetName1 = Guid.NewGuid().ToString("N").Substring(0, 10);
			await _assetCmd.IssueAssetAsync(_admin.NodeWallet, assetName1, 10, 0.001, true);
			await _assetCmd.SubscribeAsync(assetName1);

			// ACT			
			var result = await _assetCmd.ListAssetTransactionsAsync(assetName1);

			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Assert.That(result.Result.Count, Is.EqualTo(1));
		}

		[Test]
		public async Task Should_be_able_to_get_assetinfo()
		{
			var assetName = "closeasset";

			// ACT			
			var result = await _assetCmd.GetAssetInfoAsync(assetName);

			// Assert
			var assetInfo = result.Result;
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Console.WriteLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented));
			Assert.That(assetInfo.Name, Is.EqualTo("closeasset"));
			Assert.That(assetInfo.Multiple, Is.EqualTo(1));
			Assert.That(assetInfo.Open, Is.EqualTo(false));
			Assert.That(assetInfo.Units, Is.EqualTo(1));
		}

	}
}
