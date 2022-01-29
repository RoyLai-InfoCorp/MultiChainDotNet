// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
	public class MultiChainTokenManagerTests : TestBase
	{
		IMultiChainTokenManager _tokenManager;
		IMultiChainTransactionManager _txnManager;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddMultiChain()
				.AddTransient<IMultiChainTokenManager,MultiChainTokenManager>()
				.AddTransient<IMultiChainTransactionManager, MultiChainTransactionManager>()
				.AddTransient<IMultiChainAddressManager, MultiChainAddressManager>()
				.AddSingleton<SignerBase>(_ => new DefaultSigner(_admin.Ptekey))
				;
		}

		[SetUp]
		public void Setup()
		{
			_tokenManager = _container.GetRequiredService<IMultiChainTokenManager>();
			_txnManager = _container.GetRequiredService<IMultiChainTransactionManager>();
		}

		[Test, Order(20)]
		public async Task should_issue_and_send_non_fungible_token()
		{
			//Prepare
			var nfaName = RandomName();

			// ACT 1
			await _tokenManager.IssueNonfungibleAssetAsync(_admin.NodeWallet, nfaName);
			_tokenManager.IssueToken(_admin.NodeWallet, nfaName, "nft1", 1);

			// ASSERT

			// Can be found on blockchain
			var info = await _tokenManager.GetNonfungibleAssetInfoAsync(nfaName);
			Console.WriteLine("Info:" + info.ToJson());
			info.Should().NotBeNull();

			// Can be found in wallet
			var list = await _tokenManager.ListNftByAddressAsync(_admin.NodeWallet, nfaName);
			list.Count.Should().BeGreaterThan(0);

			// ACT 2
			_tokenManager.SendToken(_testUser1.NodeWallet, nfaName, "nft1");
			list = await _tokenManager.ListNftByAddressAsync(_testUser1.NodeWallet, nfaName);
			list.Count.Should().BeGreaterThan(0);


		}


	}
}
