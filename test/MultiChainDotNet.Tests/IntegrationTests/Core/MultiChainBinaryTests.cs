// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainBinary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
    public class MultiChainBinaryTests : TestBase
	{
		private MultiChainBinaryCommand _mcBinCmd;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddMultiChain();
		}

		[SetUp]
		public async Task SetUp()
		{
			_mcBinCmd = _container.GetRequiredService<MultiChainBinaryCommand>();
		}

		[Test]
		public async Task should_be_able_to_create_binary_cache()
		{
			// ACT
			var result = await _mcBinCmd.CreateBinaryCacheAsync();

			// ASSERT
			result.IsError.Should().BeFalse(result.ExceptionMessage);
			Console.WriteLine(result.Result);
			result.Result.Should().NotBeNull();

			// File should be created in /.multichain/chain1/cache/{binID} on node1
		}

		[Test]
		public async Task should_be_able_to_append_binary_cache()
		{
			var binId = (await _mcBinCmd.CreateBinaryCacheAsync()).Result;

			// ACT
			var result = await _mcBinCmd.AppendBinaryCacheAsync(binId,"0123456789abcdef");

			// ASSERT
			Console.WriteLine("binID:" + binId);
			result.IsError.Should().BeFalse(result.ExceptionMessage);
			Console.WriteLine(result.Result);
			result.Result.Should().NotBe(0);

			// File should be created in /.multichain/chain1/cache/{binID} on node1
		}

		[Test]
		public async Task should_be_able_to_delete_binary_cache()
		{
			var binId = (await _mcBinCmd.CreateBinaryCacheAsync()).Result;

			// ACT
			var result = await _mcBinCmd.DeleteBinaryCacheAsync(binId);

			// ASSERT
			Console.WriteLine("binID:" + binId);
			result.IsError.Should().BeFalse(result.ExceptionMessage);
		}



	}
}
