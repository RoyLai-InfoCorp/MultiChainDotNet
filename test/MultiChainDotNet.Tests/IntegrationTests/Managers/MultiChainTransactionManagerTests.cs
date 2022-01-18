// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Managers;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
	public class MultiChainTransactionManagerTests : TestBase
	{
		IMultiChainTransactionManager _mcTxnMgr;
		IMultiChainAssetManager _mcAssetMgr;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services
				.AddMultiChain()
				.AddTransient<IMultiChainTransactionManager, MultiChainTransactionManager>()
				.AddTransient<IMultiChainAssetManager, MultiChainAssetManager>();
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
			var txid = _mcAssetMgr.Issue(_admin.NodeWallet, assetName, 1000, true);

			// ACT
			var result = await _mcTxnMgr.ListAllTransactionsByAsset(assetName);

			//ASSERT
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0].TxId, Is.EqualTo(txid));

			// PREPARE
			for (int i = 0; i < 10; i++)
			{
				await Task.Delay(1000);
				_mcAssetMgr.SendAsset(_testUser1.NodeWallet, assetName, 1);
			}

			// ACT
			var result1 = await _mcTxnMgr.ListAllTransactionsByAsset(assetName);

			//ASSERT
			Assert.That(result1.Count, Is.EqualTo(11));
		}


		[Test, Order(90)]
		public async Task Should_list_all_address_transactions_containing_asset()
		{
			var assetName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var issue = _mcAssetMgr.Issue(_admin.NodeWallet, assetName, 1000, true);
			for (int i = 0; i < 5; i++)
			{
				await Task.Delay(1000);
				_mcAssetMgr.SendAsset(_testUser1.NodeWallet, assetName, 1);
			}

			// ACT
			var result1 = await _mcTxnMgr.ListAllTransactionsByAddress(_testUser1.NodeWallet, assetName);

			//ASSERT
			Assert.That(result1.Count(), Is.EqualTo(5));
		}

	}
}
