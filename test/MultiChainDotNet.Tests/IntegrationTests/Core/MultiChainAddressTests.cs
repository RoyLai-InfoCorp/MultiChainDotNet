// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAddress;
using NUnit.Framework;
using System.Linq;
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
			Assert.That(result.Result.Count, Is.GreaterThan(2));
		}

		[Test]
		public async Task Should_import_address_without_result()
		{
			var result = await _addrCmd.ImportAddressAsync("1ZbhobahYUb5TfKXLhfXDE67u4GvTcHUpR6KxU");

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var addresses = (await _addrCmd.ListAddressesAsync()).Result.Select(x => x.Address);
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
			var result = await _addrCmd.CreateMultiSigAsync(2, new string[] { _admin.NodeWallet, _testUser1.NodeWallet, _testUser2.NodeWallet });

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Assert.That(result.Result.Address, Is.EqualTo("4GKcwh98aehAmS4WECmfm288xQsiziVvXBZpVg"));
			Assert.That(result.Result.RedeemScript, Is.EqualTo("522103013ffb59769ea760da19bcc6a22bcb7b0e4a4a1ff64e862916af2703758b8fa021035014ec0528333a9c90b62393e14fa43808d74726f365394150b898ef84c3da8221038d35e81f60acd6d1d89ac29db000445e0fdf42b5bccb8d5ba1234d6d2add3fe853ae"));
		}

	}
}
