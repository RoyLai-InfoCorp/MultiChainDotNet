// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.Utils;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
	[TestFixture]
	public class MultiChainPermissionTests : TestBase
	{
		MultiChainPermissionCommand _permCmd;
		MultiChainAssetCommand _mcAssetCmd;
		MultiChainAddressCommand _addrCmd;

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
			logging.AddFilter((provider, category, logLevel) =>
			{
				if (logLevel < Microsoft.Extensions.Logging.LogLevel.Warning && category.Contains("MultiChainDotNet.Core.MultiChainAddress.MultiChainAddressCommand"))
					return false;
				if (logLevel < Microsoft.Extensions.Logging.LogLevel.Warning && category.Contains("MultiChainDotNet.Core.MultiChainAsset.MultiChainAssetCommand"))
					return false;
				return true;
			});
		}

		[SetUp]
		public async Task SetUp()
		{
			_mcAssetCmd = _container.GetRequiredService<MultiChainAssetCommand>();
			_addrCmd = _container.GetRequiredService<MultiChainAddressCommand>();
			_permCmd = _container.GetRequiredService<MultiChainPermissionCommand>();

			////https://www.generacodice.com/en/articolo/2900697/add,-enable-and-disable-nlog-loggers-programmatically

			//// Ignore logs from MultiChainAddressCommand and MultiChainAssetCommand
			//var rule = LogManager.Configuration.FindRuleByName("ChattyInfo");
			//rule.Filters.Add(new ConditionBasedFilter()
			//{
			//	Condition = "equals('${logger}','MultiChainDotNet.Core.MultiChainAddress.MultiChainAddressCommand')",
			//	Action = FilterResult.Ignore
			//});
			//rule.Filters.Add(new ConditionBasedFilter()
			//{
			//	Condition = "equals('${logger}','MultiChainDotNet.Core.MultiChainAsset.MultiChainAssetCommand')",
			//	Action = FilterResult.Ignore
			//});
			//LogManager.ReconfigExistingLoggers();

			await Task.Delay(2000);

		}

		[Test, Order(10)]
		public async Task Should_list_all_permissions()
		{
			var result = await _permCmd.ListPermissionsByTypeAsync("issue");
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Console.WriteLine(JsonConvert.SerializeObject(result.Result));

			result = await _permCmd.ListPermissionsByAddressAsync(_admin.NodeWallet);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Console.WriteLine(JsonConvert.SerializeObject(result.Result));
		}

		[Test, Order(20)]
		public async Task Should_not_be_able_to_grant_issue_permission_by_1_admin()
		{
			var newAddr = (await _addrCmd.GetNewAddressAsync()).Result;

			// ACT
			await _permCmd.GrantPermissionFromAsync(_relayer1.NodeWallet, newAddr, "issue");

			// ASSERT
			var checkPermission = await _permCmd.CheckPermissionGrantedAsync(newAddr, "issue");
			Assert.That(checkPermission.Result, Is.False);
		}




		[Test, Order(30)]
		public async Task Should_be_able_to_grant_issue_permission_by_2_admins()
		{
			await _mcAssetCmd.SendAsync(_relayer1.NodeWallet, 1000);
			await _mcAssetCmd.SendAsync(_relayer2.NodeWallet, 1000);
			var newAddr = (await _addrCmd.GetNewAddressAsync()).Result;

			// ACT: Relayer1 grant permission
			await GrantPermissionFromNode(_relayer1,newAddr,"issue");

			// ACT: Relayer2 grant permission
			await GrantPermissionFromNode(_relayer2, newAddr, "issue");

			// ASSERT
			await _permCmd.WaitUntilPermissionGranted(newAddr, "issue");
			var checkPermission = await _permCmd.CheckPermissionGrantedAsync(newAddr, "issue");
			Assert.That(checkPermission.Result, Is.True);
		}

		[Test, Order(40)]
		public async Task Should_not_be_able_to_revoke_issue_permission_by_1_admin()
		{
			await _mcAssetCmd.SendAsync(_relayer1.NodeWallet, 1000);
			await _mcAssetCmd.SendAsync(_relayer2.NodeWallet, 1000);
			var newAddr = (await _addrCmd.GetNewAddressAsync()).Result;
			await GrantPermissionFromNode(_admin, newAddr, "issue");
			await GrantPermissionFromNode(_relayer1, newAddr, "issue");
			await GrantPermissionFromNode(_relayer2, newAddr, "issue");
			var wait = await _permCmd.WaitUntilPermissionGranted(newAddr, "issue");
			wait.Should().BeTrue();

			// ACT
			await RevokePermissionFromNode(_relayer1, newAddr, "issue");

			// ASSERT
			var checkPermission = await _permCmd.CheckPermissionGrantedAsync(newAddr, "issue");
			Assert.That(checkPermission.Result, Is.True);
		}

		[Test, Order(50)]
		public async Task Should_be_able_to_revoke_issue_permission_by_2_admin()
		{
			await _mcAssetCmd.SendAsync(_relayer1.NodeWallet, 1000);
			await _mcAssetCmd.SendAsync(_relayer2.NodeWallet, 1000);
			var newAddr = (await _addrCmd.GetNewAddressAsync()).Result;
			await GrantPermissionFromNode(_admin, newAddr, "issue");
			await GrantPermissionFromNode(_relayer1, newAddr, "issue");
			await GrantPermissionFromNode(_relayer2, newAddr, "issue");
			var wait = await _permCmd.WaitUntilPermissionGranted(newAddr, "issue");
			wait.Should().BeTrue();

			// ACT
			await RevokePermissionFromNode(_relayer1, newAddr, "issue");
			await RevokePermissionFromNode(_relayer2, newAddr, "issue");

			// ASSERT
			await _permCmd.WaitUntilPermissionRevoked(newAddr, "issue");
			var checkPermission = await _permCmd.CheckPermissionGrantedAsync(newAddr, "issue");
			Assert.That(checkPermission.Result, Is.False);
		}

	}
}
