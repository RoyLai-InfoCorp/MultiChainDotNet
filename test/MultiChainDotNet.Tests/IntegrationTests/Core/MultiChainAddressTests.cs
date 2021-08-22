// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAddress;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
	[TestFixture]
    public class MultiChainAddressTests : TestBase
    {
		MultiChainAddressCommand _addrCmd;
		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddMultiChain();
		}

		[SetUp]
		public void SetUp()
		{
			_addrCmd = _container.GetRequiredService<MultiChainAddressCommand>();
		}

		[Test]
        public async Task Should_list_all_addresses()
		{
			var result = await _addrCmd.ListAddressesAsync();

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

		[Test]
		public async Task Should_list_all_addresses_ismine()
		{
			var result = await _addrCmd.ListAddressesAsync(true);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Assert.That(result.Result.Count, Is.GreaterThan(5));
		}

		[Test]
		public async Task Should_import_address_without_result()
		{
			var result = await _addrCmd.ImportAddressAsync("1ZbhobahYUb5TfKXLhfXDE67u4GvTcHUpR6KxU");

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var addresses = (await _addrCmd.ListAddressesAsync()).Result.Select(x=>x.Address);
			Assert.That(addresses, Contains.Item("1ZbhobahYUb5TfKXLhfXDE67u4GvTcHUpR6KxU"));
		}

		[Test]
		public async Task Should_get_new_address()
		{
			var result = await _addrCmd.GetNewAddressAsync();

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Assert.That(result.Result, Is.TypeOf(typeof(string)));
			var addresses = (await _addrCmd.ListAddressesAsync()).Result.Select(x => x.Address);
			Assert.That(addresses, Contains.Item(result.Result));
		}

		[Test]
		public async Task Should_get_multisig_address()
		{
			var result = await _addrCmd.CreateMultiSigAsync(2,new string[] { _relayer1.NodeWallet, _relayer2.NodeWallet, _relayer3.NodeWallet });

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Assert.That(result.Result.Address, Is.EqualTo("4JwrgyiN5SoWHRRpvxhwFd1cDRTd8tRyhEV6TW"));
			Assert.That(result.Result.RedeemScript, Is.EqualTo("522103cbb355bd0f558b892113dff5f45c847ef948219673f784970beb5fc532effe8021039b3e46c0d89ae930bcc8d7b45c7e149d14bd69e45cfaec84baf328779d3813612102ba5946e3e0fa9c1da6ba38bdc9f12c75d7c36572bcda80db6feca1b302671f0853ae"));
		}

	}
}
