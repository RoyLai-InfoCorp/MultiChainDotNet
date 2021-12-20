// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainToken;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
	[TestFixture]
	public class MultiChainTransactionTests : TestBase
	{
		public class TestState
		{
			public string AssetName { get; set; }
		}

		Microsoft.Extensions.Logging.ILogger _logger;
		private MultiChainPermissionCommand _permCmd;
		private MultiChainAddressCommand _addrCmd;
		MultiChainTransactionCommand _txnCmd;
		MultiChainAssetCommand _assetCmd;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddMultiChain()
				;
		}

		protected override void ConfigureLogging(ILoggingBuilder logging)
		{
			base.ConfigureLogging(logging);
			//logging.AddFilter((provider, category, logLevel) =>
			//{
			//	if (logLevel < LogLevel.Warning && category.Contains("MultiChainDotNet.Core.MultiChainTransaction.MultiChainTransactionCommand"))
			//		return false;
			//	return true;
			//});
		}

		[SetUp]
		public async Task SetUp()
		{
			//var factory = _container.GetRequiredService<IMultiChainCommandFactory>();
			//_txnCmd = factory.CreateCommand<MultiChainTransactionCommand>();
			//_assetCmd = factory.CreateCommand<MultiChainAssetCommand>();

			_txnCmd = _container.GetRequiredService<MultiChainTransactionCommand>();
			_assetCmd = _container.GetRequiredService<MultiChainAssetCommand>();
			_logger = _container.GetRequiredService<ILogger<MultiChainTransactionTests>>();
			_permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();
			_addrCmd = _container.GetRequiredService<MultiChainAddressCommand>();

		}

		[Test, Order(5)]
		public async Task should_send_payment()
		{
			var txid = await _txnCmd.CreateRawSendFromAsync(
			  _admin.NodeWallet,
			  new Dictionary<string, object>
			  {
				  {
					_testUser1.NodeWallet,
					new Dictionary<string, object>{ { "", 1000 } }
				  }
			  },
			  new object[] { },
			  "send");

			if (txid.IsError) throw txid.Exception;
			txid.Result.Should().NotBeNullOrEmpty();
		}

		[Test, Order(10),Ignore("")]
		public async Task should_list_address_transaction_for_payment()
		{
			var sendResult = await _assetCmd.SendFromAsync(_admin.NodeWallet, _testUser1.NodeWallet, 100);

			// ACT
			var result1 = await _txnCmd.ListAddressTransactions(_admin.NodeWallet);
			var result2 = await _txnCmd.ListAddressTransactions(_testUser1.NodeWallet);

			// ASSERT

			// paid 100 + fee
			Assert.That(result1.IsError, Is.False, result1.ExceptionMessage);
			var lastResult1 = result1.Result.Last();
			Assert.That(lastResult1.TxId, Is.EqualTo(sendResult.Result));
			Assert.That(lastResult1.Balance.Amount, Is.LessThan(-100));
			_logger.LogInformation(lastResult1.ToJson());

			// received 100
			Assert.That(result2.IsError, Is.False, result2.ExceptionMessage);
			var lastResult2 = result2.Result.Last();
			Assert.That(lastResult2.TxId, Is.EqualTo(sendResult.Result));
			Assert.That(lastResult2.Balance.Amount, Is.EqualTo(100));
			_logger.LogInformation(lastResult2.ToJson());

		}

		[Test, Order(20)]
		public async Task should_list_address_transaction_for_assets()
		{
			var newSender = (await _addrCmd.GetNewAddressAsync()).Result;
			var newRecipient = (await _addrCmd.GetNewAddressAsync()).Result;
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 10);
			await _assetCmd.IssueAssetAsync(newSender, assetName, 10, 1, true);
			await _permCmd.GrantPermissionAsync(newSender, "send");
			await _permCmd.GrantPermissionAsync(newRecipient, "receive");
			await FundWallet(newSender);
			var sendResult = await _assetCmd.SendAssetFromAsync(newSender, newRecipient, assetName, 1);

			// ACT
			var result1 = await _txnCmd.ListAddressTransactions(newSender);
			var result2 = await _txnCmd.ListAddressTransactions(newRecipient);

			// ASSERT

			// paid 1 asset + fee
			Assert.That(result1.IsError, Is.False, result1.ExceptionMessage);
			var lastResult1 = result1.Result.Last();
			_logger.LogInformation(lastResult1.ToJson());
			Assert.That(lastResult1.TxId, Is.EqualTo(sendResult.Result));
			Assert.That(lastResult1.Balance.Assets[0].Name, Is.EqualTo(assetName));
			Assert.That(lastResult1.Balance.Assets[0].Qty, Is.EqualTo(-1));

			// received 1 asset
			Assert.That(result2.IsError, Is.False, result2.ExceptionMessage);
			var lastResult2 = result2.Result.Last();
			_logger.LogInformation(lastResult2.ToJson());
			Assert.That(lastResult2.TxId, Is.EqualTo(sendResult.Result));
			Assert.That(lastResult2.Balance.Amount, Is.EqualTo(0));
			Assert.That(lastResult2.Balance.Assets[0].Name, Is.EqualTo(assetName));
			Assert.That(lastResult2.Balance.Assets[0].Qty, Is.EqualTo(1));

		}

		[Test, Order(30)]
		public async Task Should_return_list_of_unspent_utxo()
		{
			var result = await _txnCmd.ListUnspentAsync();
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			_logger.LogInformation(result.Result.ToJson());
		}

		[Test, Order(40)]
		public async Task Should_issue_asset_with_createrawsendfrom()
		{
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			var raw = await _txnCmd.CreateRawSendFromAsync(_admin.NodeWallet,
				new Dictionary<string, object>
				{
					{
						_admin.NodeWallet,
						new
						{
							issue = new { raw = 1 }
						}
					}
				},
				new object[] {
					new
					{
						create = "asset",
						name = assetName,
						multiple = 1,
						open = true
					}
				}, 
				"send");
			Assert.That(raw.IsError, Is.False, raw.ExceptionMessage);
			//var signed = await _txnCmd.SignRawTransactionAsync(raw.Result);
			//Assert.That(signed.IsError, Is.False, signed.ExceptionMessage);
			//var result = await _txnCmd.SendRawTransactionAsync(signed.Result.Hex);
			//Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

		[Test, Order(43)]
		public async Task Should_issue_non_fungible_asset_with_createrawsendfrom()
		{
			// ACT
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 6);
			Console.WriteLine(nfaName);
			var raw = await _txnCmd.CreateRawSendFromAsync(
				_admin.NodeWallet,
				new Dictionary<string, object>
				{
					{
						_admin.NodeWallet,
						new Dictionary<string, object>{ 
							{ "", 6000 },
							{ "issue", new { raw = 0} }
						}
					}
				},
				new object[] {
					new
					{
						create = "asset",
						name = nfaName,
						fungible = false,
						open = true
					}
				},
				"send");
			raw.IsError.Should().BeFalse(raw.ExceptionMessage);

			// ASSERT

			var tokenCmd = _container.GetRequiredService<MultiChainTokenCommand>();

			// Cna be found on blockchain
			var info = await tokenCmd.GetNfaInfo(nfaName);
			Console.WriteLine("Info:" + info.Result.ToJson());
			info.IsError.Should().BeFalse();

			// Can be found in wallet
			var nfas = await tokenCmd.ListNfa(_admin.NodeWallet);
			nfas.Result.Any(x => x.Name == nfaName).Should().BeTrue();
		}

		[Test, Order(46)]
		public async Task Should_issue_token_with_createrawsendfrom()
		{
			var tokenCmd = _container.GetRequiredService<MultiChainTokenCommand>();
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 6);
			Console.WriteLine(nfaName);
			var result = await tokenCmd.IssueNfaAsync(_admin.NodeWallet,nfaName);
			if (result.IsError) throw result.Exception;
			var issued = await tokenCmd.WaitUntilNfaIssued(_admin.NodeWallet, nfaName);
			issued.Should().BeTrue();

			var perm = _container.GetRequiredService<MultiChainPermissionCommand>();
			await perm.GrantPermissionAsync(_admin.NodeWallet, $"{nfaName}.issue");
			await perm.WaitUntilPermissionGranted(_admin.NodeWallet, $"{nfaName}.issue");

			// ACT
			var raw = await _txnCmd.CreateRawSendFromAsync(
				_admin.NodeWallet,
				new Dictionary<string, object>
				{
					{
						_admin.NodeWallet, 
						new Dictionary<string,object>
						{
							{ "", 6000 },
							{ "issuetoken",new
								{
									asset = nfaName,
									token = "nft1",
									qty = 1
								}
							}
						}
					}
				},
				new object[] { },
				"send");

			if (raw.IsError) throw raw.Exception;

			// WAIT
			var wait = await TaskHelper.WaitUntilTrue(async () => {
				var bal = await tokenCmd.GetTokenBalancesAsync(_admin.NodeWallet);
				if (bal.IsError)
					throw bal.Exception;
				return bal.Result[_admin.NodeWallet].Any(x => x.NfaName == nfaName);
			}
			, 5, 500);

			// ASSERT
			var bal = await tokenCmd.GetTokenBalancesAsync(_admin.NodeWallet);
			bal.Result[_admin.NodeWallet].Where(x => x.NfaName == nfaName).Count().Should().Be(1);
			bal.Result[_admin.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");
		}

		[Test, Order(46)]
		public async Task Should_send_token_with_createrawsendfrom()
		{
			var tokenCmd = _container.GetRequiredService<MultiChainTokenCommand>();
			var nfaName = Guid.NewGuid().ToString("N").Substring(0, 6);
			Console.WriteLine(nfaName);
			await tokenCmd.IssueNfaAsync(_admin.NodeWallet, nfaName);
			var perm = _container.GetRequiredService<MultiChainPermissionCommand>();
			await perm.GrantPermissionAsync(_admin.NodeWallet, $"{nfaName}.issue");
			await tokenCmd.IssueNftAsync(_admin.NodeWallet, nfaName, "nft1");

			// ACT
			var raw = await _txnCmd.CreateRawSendFromAsync(
				_admin.NodeWallet,
				new Dictionary<string, object>
				{
					{
						_testUser1.NodeWallet,
						new Dictionary<string,object>
						{
							{
								nfaName,
								new {
									token = "nft1",
									qty = 1
								}
							}
						}
					}
				},
				new object[] { },
				"send");
			if (raw.IsError)
				throw raw.Exception;

			// ASSERT

			var bal = await tokenCmd.GetTokenBalancesAsync(_testUser1.NodeWallet);
			bal.Result[_testUser1.NodeWallet].Where(x => x.NfaName == nfaName).Count().Should().Be(1);
			bal.Result[_testUser1.NodeWallet].SingleOrDefault(x => x.NfaName == nfaName).Token.Should().Be("nft1");
		}




		[Test, Order(50)]
		public async Task Should_throw_insufficient_priority_when_using_unspent_with_sufficient_balance()
		{
			var unspents = await _txnCmd.ListUnspentAsync(_admin.NodeWallet);
			var unspent = unspents.Result.FirstOrDefault(x => x.Amount == 0);
			var raw = await _txnCmd.CreateRawTransactionAsync(unspent.TxId, unspent.Vout,
				new Dictionary<string, object>
				{
					{
						_testUser1.NodeWallet,
						new Dictionary<string, object>
						{
							{ "", 1 }
						}
					}
				});
			var signed = await _txnCmd.SignRawTransactionAsync(raw.Result);
			var result = await _txnCmd.SendRawTransactionAsync(signed.Result.Hex);
			Assert.That(result.IsError, Is.True, result.ExceptionMessage);
			Assert.That(result.ExceptionMessage, Contains.Substring("insufficient priority"));
		}

		/// <summary>
		/// AppendRawChangeAsync doesn't work with Issue Asset
		/// </summary>
		/// <returns></returns>
		[Test, Order(70), Ignore("")]
		public async Task Should_throw_unconfirmed_issue_transaction_in_input_using_appendrawchange()
		{
			var unspents = await _txnCmd.ListUnspentAsync(_admin.NodeWallet);
			var unspent = unspents.Result.First(x => x.Amount > 0);
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);
			var raw = await _txnCmd.CreateRawTransactionAsync(
				new List<TxIdVoutStruct> { new TxIdVoutStruct { TxId = unspent.TxId, Vout = unspent.Vout } },
				new Dictionary<string, object>
				{
					{
						_admin.NodeWallet,
						new
						{
							issue = new { raw = 1 }
						}
					}
				},
				new object[] {
					new
					{
						create = "asset",
						name = assetName,
						multiple = 1,
						open = true
					}
				});
			Assert.That(raw.IsError, Is.False, raw.ExceptionMessage);
			await _txnCmd.LockUnspentAsync(false, unspent.TxId, unspent.Vout);
			var withrawchange = await _txnCmd.AppendRawChangeAsync(raw.Result, _admin.NodeWallet);
			await _txnCmd.LockUnspentAsync(true, unspent.TxId, unspent.Vout);
			Assert.That(withrawchange.IsError, Is.True);
			Assert.That(withrawchange.ExceptionMessage, Contains.Substring("Unconfirmed issue transaction in input"));
		}

		[Test, Order(80)]
		public async Task Should_pay_using_createrawtransaction()
		{
			var lockUnspent = await _txnCmd.PrepareLockUnspentFromAsync(_admin.NodeWallet, "", 3000);
			var raw = await _txnCmd.CreateRawTransactionAsync(
				lockUnspent.Result.TxId,
				lockUnspent.Result.Vout,
				new Dictionary<string, object>
				{
					{
						_testUser1.NodeWallet,
						new Dictionary<string, object>
						{
							{ "", 2000 }
						}
					}
				});
			var withrawchange = await _txnCmd.AppendRawChangeAsync(raw.Result, _admin.NodeWallet);
			var signed = await _txnCmd.SignRawTransactionAsync(withrawchange.Result);
			var result = await _txnCmd.SendRawTransactionAsync(signed.Result.Hex);

			// ASSERT
			var decode = await _txnCmd.DecodeRawTransactionAsync(withrawchange.Result);
			_logger.LogInformation(JsonConvert.SerializeObject(decode.Result));
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

		[Test, Order(90)]
		public async Task Should_issue_asset_to_self_using_createrawtransaction()
		{
			_stateDb.ClearState<TestState>();
			var assetName = Guid.NewGuid().ToString("N").Substring(0, 6);

			var lockUnspent = await _txnCmd.PrepareLockUnspentFromAsync(_admin.NodeWallet, "", 1000);
			var raw = await _txnCmd.CreateRawTransactionAsync(
				lockUnspent.Result.TxId,
				lockUnspent.Result.Vout,
				new Dictionary<string, object>
				{
					{
						_admin.NodeWallet,
						new Dictionary<string,object>
						{
							{ "issue",new { raw = 10} }
						}
					}
				},
				new object[] {
					new
					{
						create = "asset",
						name = assetName,
						multiple = 1,
						open = true
					}
				});
			var signed = await _txnCmd.SignRawTransactionAsync(raw.Result);
			var result = await _txnCmd.SendRawTransactionAsync(signed.Result.Hex);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var state = new TestState { AssetName = assetName };
			_stateDb.SaveState(state);
		}

		[Test, Order(100)]
		public async Task Should_send_asset_with_inlinemetadata_using_createrawtransaction()
		{
			var state = _stateDb.GetState<TestState>();
			var assetName = state.AssetName;

			var lockFees = await _txnCmd.PrepareLockUnspentFromAsync(_admin.NodeWallet, "", 1000);
			var lockAsset = await _txnCmd.PrepareLockUnspentFromAsync(_admin.NodeWallet, assetName, 1);
			var payload = new { DestinationChain = 1, DestinationAddress = "0xa3A5eC6ACEC6Ad6A92FFB1b30865B6A2929AE5f8" };
			var raw = await _txnCmd.CreateRawTransactionAsync(
				new List<TxIdVoutStruct> {
					new TxIdVoutStruct{ TxId = lockFees.Result.TxId, Vout = lockFees.Result.Vout },
					new TxIdVoutStruct{ TxId = lockAsset.Result.TxId, Vout = lockAsset.Result.Vout }
				},
				new Dictionary<string, object>
				{
					{
						_testUser1.NodeWallet,
						new Dictionary<string, object>
						{
							{ assetName, 1},
							{ "data", new { json = payload } }
						}
					}
				});
			var signed = await _txnCmd.SignRawTransactionAsync(raw.Result);
			var result = await _txnCmd.SendRawTransactionAsync(signed.Result.Hex);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

		[Test, Order(110)]
		public async Task Should_send_asset_with_txout_containing_metadata_using_createrawtransaction()
		{
			var state = _stateDb.GetState<TestState>();
			var assetName = state.AssetName;
			var sendresult = await _assetCmd.SendFromAsync(_admin.NodeWallet, _testUser1.NodeWallet, 1000);
			Assert.That(sendresult.IsError, Is.False, sendresult.ExceptionMessage);
			var lockFees = await _txnCmd.PrepareLockUnspentFromAsync(_testUser1.NodeWallet, "", 1000);

			ListUnspentResult unspent = null;
			while (unspent == null)
			{
				var unspents = await _txnCmd.ListUnspentAsync(_testUser1.NodeWallet);
				unspent = unspents.Result.FirstOrDefault(x => x.Assets.Any(y => y.Name == assetName));
				await Task.Delay(2000);
			}

			var raw = await _txnCmd.CreateRawTransactionAsync(
				new List<TxIdVoutStruct> {
					new TxIdVoutStruct{ TxId = lockFees.Result.TxId, Vout = lockFees.Result.Vout },
					new TxIdVoutStruct{ TxId = unspent.TxId, Vout = unspent.Vout }
				},
				new Dictionary<string, object>
				{
					{
						_admin.NodeWallet,
						new Dictionary<string, object>
						{
							{ assetName, 1}
						}
					}
				});
			var signed = await _txnCmd.SignRawTransactionAsync(raw.Result);
			var result = await _txnCmd.SendRawTransactionAsync(signed.Result.Hex);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

		[Test, Order(120)]
		public async Task Should_list_transactions_by_asset()
		{
			await _assetCmd.SubscribeAsync("openasset");
			var result = await _txnCmd.ListAssetTransactions("openasset");
			Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

		[Test, Order(130)]
		public async Task Should_list_transactions_by_address()
		{
			var result = await _txnCmd.ListAddressTransactions(_admin.NodeWallet);
			Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

		[Test, Order(140)]
		public async Task Should_list_transaction_by_address_in_the_correct_order()
		{
			var result1 = await _txnCmd.ListAddressTransactions(_admin.NodeWallet);
			var list1 = result1.Result.Select(x => x.Confirmations);
			Console.WriteLine("ListAddressTransactions Default:");
			Console.WriteLine(JsonConvert.SerializeObject(list1, Formatting.Indented));

			var result2 = await _txnCmd.ListAddressTransactions(_admin.NodeWallet, 1);
			var list2 = result2.Result.Select(x => x.Confirmations);
			Console.WriteLine("ListAddressTransactions 1 count:");
			Console.WriteLine(JsonConvert.SerializeObject(list2, Formatting.Indented));

			var result3 = await _txnCmd.ListAddressTransactions(_admin.NodeWallet, 20);
			var list3 = result3.Result.Select(x => x.Confirmations);
			Console.WriteLine("ListAddressTransactions 20 count:");
			Console.WriteLine(JsonConvert.SerializeObject(list3, Formatting.Indented));
		}

		[Test, Order(160),Ignore("")]
		public async Task Should_throw_insane_fees_when_pay_without_change()
		{
			//var lockUnspent = await _txnCmd.PrepareLockUnspentFromAsync(_admin.NodeWallet, "", 3000);
			ListUnspentResult unspent = null;
			while (unspent == null)
			{
				var unspents = await _txnCmd.ListUnspentAsync(_admin.NodeWallet);
				unspent = unspents.Result.FirstOrDefault(x => x.Amount > 3000);
				await Task.Delay(2000);
			}

			var raw = await _txnCmd.CreateRawTransactionAsync(unspent.TxId, unspent.Vout,
				new Dictionary<string, object>
				{
					{
						_testUser1.NodeWallet,
						new Dictionary<string, object>
						{
							{ "", 1 }
						}
					}
				});
			var signed = await _txnCmd.SignRawTransactionAsync(raw.Result);
			var result = await _txnCmd.SendRawTransactionAsync(signed.Result.Hex);
			Assert.That(result.IsError, Is.True, result.ExceptionMessage);
			Assert.That(result.ExceptionMessage, Contains.Substring("Insane fees"));

		}


	}
}
