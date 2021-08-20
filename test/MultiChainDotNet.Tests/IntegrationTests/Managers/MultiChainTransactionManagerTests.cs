using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Managers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
	public class MultiChainTransactionManagerTests : TestCommandFactoryBase
	{
		IMultiChainTransactionManager _mcTxnMgr;
		IMultiChainAssetManager _mcAssetMgr;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services.AddTransient<IMultiChainTransactionManager, MultiChainTransactionManager>();
			services.AddTransient<IMultiChainAssetManager, MultiChainAssetManager>();
		}

		[SetUp]
		public void SetUp()
		{
			_mcTxnMgr = _container.GetRequiredService<IMultiChainTransactionManager>();
			_mcAssetMgr = _container.GetRequiredService<IMultiChainAssetManager>();
		}

		[Test, Order(80)]
		public async Task Should_list_all_asset_transactions()
		{
			var assetName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var issue = _mcAssetMgr.Issue(_admin.NodeWallet, assetName, 1000, true);
			var txid = issue.Result;

			// ACT
			var result = await _mcTxnMgr.ListAllTransactionsByAsset(assetName);

			//ASSERT
			Assert.That(result.IsError, Is.EqualTo(false), result.ExceptionMessage);
			Assert.That(result.Result.Count, Is.EqualTo(1));
			Assert.That(result.Result[0].TxId, Is.EqualTo(txid));

			// PREPARE
			for (int i = 0; i < 30; i++)
			{
				await Task.Delay(1000);
				_mcAssetMgr.SendAsset(_testUser1.NodeWallet, assetName, 1);
			}

			// ACT
			var result1 = await _mcTxnMgr.ListAllTransactionsByAsset(assetName);

			//ASSERT
			Assert.That(result1.IsError, Is.EqualTo(false), result1.ExceptionMessage);
			Assert.That(result1.Result.Count, Is.EqualTo(31));
		}


		[Test, Order(90)]
		public async Task Should_list_all_address_transactions_containing_asset()
		{
			var assetName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var issue = _mcAssetMgr.Issue(_admin.NodeWallet, assetName, 1000, true);
			for (int i = 0; i < 5; i++)
				_mcAssetMgr.SendAsset(_testUser1.NodeWallet, assetName, 1);

			// ACT
			var result1 = await _mcTxnMgr.ListAllTransactionsByAddress(_testUser1.NodeWallet, assetName);

			//ASSERT
			Assert.That(result1.IsError, Is.EqualTo(false), result1.ExceptionMessage);
			Assert.That(result1.Result.Count(), Is.EqualTo(5));
		}

	}
}
