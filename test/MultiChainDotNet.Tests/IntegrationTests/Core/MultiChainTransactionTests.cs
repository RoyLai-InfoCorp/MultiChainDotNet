using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using Newtonsoft.Json;
using NLog;
using NLog.Filters;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
	[TestFixture]
	public class MultiChainTransactionTests : TestBase
	{
		public class TestState
		{
			public string AssetName {get;set;}
		}

		Microsoft.Extensions.Logging.ILogger _logger;
		MultiChainTransactionCommand _txnCmd;
		MultiChainAssetCommand _assetCmd;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services.AddTransient<MultiChainTransactionCommand>();
			services.AddTransient<MultiChainAssetCommand>();
		}
		protected override void ConfigureLogging(ILoggingBuilder logging)
		{
			base.ConfigureLogging(logging);
			logging.AddFilter((provider, category, logLevel) =>
			{
				if (logLevel < Microsoft.Extensions.Logging.LogLevel.Warning && category.Contains("MultiChainDotNet.Core.MultiChainTransaction.MultiChainTransactionCommand"))
					return false;
				return true;
			});
		}

		[SetUp]
		public async Task SetUp()
		{
			_txnCmd = _container.GetRequiredService<MultiChainTransactionCommand>();
			_assetCmd = _container.GetRequiredService<MultiChainAssetCommand>();
			_logger = _container.GetRequiredService<ILogger<MultiChainTransactionTests>>();
			await Task.Delay(2000);
		}

		[Test, Order(10)]
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
			var sendResult = await _assetCmd.SendAssetFromAsync(_admin.NodeWallet, _testUser1.NodeWallet, "openasset", 1);

			// ACT
			var result1 = await _txnCmd.ListAddressTransactions(_admin.NodeWallet);
			var result2 = await _txnCmd.ListAddressTransactions(_testUser1.NodeWallet);

			// ASSERT

			// paid 1 asset + fee
			Assert.That(result1.IsError, Is.False, result1.ExceptionMessage);
			var lastResult1 = result1.Result.Last();
			Assert.That(lastResult1.TxId, Is.EqualTo(sendResult.Result));
			Assert.That(lastResult1.Balance.Amount, Is.LessThan(0));
			Assert.That(lastResult1.Balance.Assets[0].Name, Is.EqualTo("openasset"));
			Assert.That(lastResult1.Balance.Assets[0].Qty, Is.EqualTo(-1));
			_logger.LogInformation(lastResult1.ToJson());

			// received 1 asset
			Assert.That(result2.IsError, Is.False, result2.ExceptionMessage);
			var lastResult2 = result2.Result.Last();
			Assert.That(lastResult2.TxId, Is.EqualTo(sendResult.Result));
			Assert.That(lastResult2.Balance.Amount, Is.EqualTo(0));
			Assert.That(lastResult2.Balance.Assets[0].Name, Is.EqualTo("openasset"));
			Assert.That(lastResult2.Balance.Assets[0].Qty, Is.EqualTo(1));
			_logger.LogInformation(lastResult2.ToJson());

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
				});
			Assert.That(raw.IsError, Is.False, raw.ExceptionMessage);
			var signed = await _txnCmd.SignRawTransactionAsync(raw.Result);
			Assert.That(signed.IsError, Is.False, signed.ExceptionMessage);
			var result = await _txnCmd.SendRawTransactionAsync(signed.Result.Hex);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
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

		[Test, Order(60)]
		public async Task Should_throw_insane_fees_when_pay_without_change()
		{
			//var lockUnspent = await _txnCmd.PrepareLockUnspentFromAsync(_admin.NodeWallet, "", 3000);
			var unspents = await _txnCmd.ListUnspentAsync(_admin.NodeWallet);
			var unspent = unspents.Result.FirstOrDefault(x => x.Amount > 0);
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

		/// <summary>
		/// AppendRawChangeAsync doesn't work with Issue Asset
		/// </summary>
		/// <returns></returns>
		[Test, Order(70)]
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

			await _assetCmd.SendFromAsync(_admin.NodeWallet, _testUser1.NodeWallet, 1000);
			var lockFees = await _txnCmd.PrepareLockUnspentFromAsync(_testUser1.NodeWallet, "", 1000);
			var unspents = await _txnCmd.ListUnspentAsync(_testUser1.NodeWallet);
			var unspent = unspents.Result.FirstOrDefault(x => x.Assets.Any(y => y.Name == assetName));

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

	}
}
