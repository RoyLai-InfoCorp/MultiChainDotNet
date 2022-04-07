// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Managers;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
	public class MultiChainStreamManagerTests : TestBase
	{
		IMultiChainStreamManager _streamManager;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services
				.AddMultiChain()
				.AddTransient<IMultiChainStreamManager, MultiChainStreamManager>();
		}

		[SetUp]
		public void SetUp()
		{
			_streamManager = _container.GetRequiredService<IMultiChainStreamManager>();
		}

		[Test]
		public async Task Should_be_able_to_create_new_stream()
		{
			var streamName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var txid = _streamManager.CreateStreamAsync(streamName);

			// ASSERT
			var found = await _streamManager.GetStreamInfoAsync(streamName);
			Assert.That(found.Name, Is.EqualTo(streamName));
		}

		public class JsonStreamItem
		{
			public int Counter;
		}

		[Test]
		public async Task Should_be_able_to_publish_new_streamitem()
		{
			var streamName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			await _streamManager.CreateStreamAsync(streamName);

			// ACT
			await _streamManager.PublishJsonAsync(streamName, "1", new JsonStreamItem { Counter = 1 });

			// ASSERT
			var found = await _streamManager.GetStreamItemAsync<JsonStreamItem>($"FROM {streamName} WHERE key='1'");
			Assert.That(found.Counter, Is.EqualTo(1));
		}

		[Test, Order(50)]
		public async Task Should_throw_rpc_transaction_rejected_error_when_stream_already_exists()
		{
			var streamName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var result = await _streamManager.CreateStreamAsync(streamName);

			var streamCmd = _container.GetRequiredService<MultiChainStreamCommand>();
			await streamCmd.WaitUntilStreamExists(streamName);

			Func<Task> action = () => _streamManager.CreateStreamAsync(streamName);
			action.Should().ThrowAsync<MultiChainException>().WithMessage($"*RPC DUPLICATE NAME*");
		}

		private async Task<string> CreateNewStream()
		{
			var prefix = Guid.NewGuid().ToString("N").Substring(0, 5);
			var newStream = prefix;
			await _streamManager.CreateStreamAsync(newStream);
			for (int i = 0; i < 10; i++)
			{
				var key = prefix + "-" + i;
				var publishResult = await _streamManager.PublishJsonAsync(newStream, key, new JsonStreamItem { Counter = i });
			}
			return newStream;
		}

		[Test, Order(60)]
		public async Task Should_list_published_Json_streamitems()
		{
			var newStream = await CreateNewStream();

			// Get first item
			var result1 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} ASC");
			Assert.That(result1[0].Counter, Is.EqualTo(0));

			// Get last item
			var result2 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream}");
			Assert.That(result2[0].Counter, Is.EqualTo(9));

		}

		[Test, Order(70)]
		public async Task Should_list_published_Json_streamitems_with_paging()
		{
			var newStream = await CreateNewStream();

			// FROM <newStream> DESC PAGE 0 SIZE 2
			var result7 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} PAGE 0 SIZE 2");
			Assert.That(result7[0].Counter, Is.EqualTo(9));
			Assert.That(result7[1].Counter, Is.EqualTo(8));
			// FROM <newStream> DESC PAGE 1 SIZE 2
			var result8 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} PAGE 1 SIZE 2");
			Assert.That(result8[0].Counter, Is.EqualTo(7));
			Assert.That(result8[1].Counter, Is.EqualTo(6));
			// FROM <newStream> DESC PAGE 2 SIZE 2
			var result9 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} PAGE 2 SIZE 2");
			Assert.That(result9[0].Counter, Is.EqualTo(5));
			Assert.That(result9[1].Counter, Is.EqualTo(4));

			// FROM <newStream> DESC PAGE 0 SIZE 2
			var result10 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} ASC PAGE 0 SIZE 2");
			Assert.That(result10[0].Counter, Is.EqualTo(0));
			Assert.That(result10[1].Counter, Is.EqualTo(1));
		}

		class TestClassA
		{
			public string Name;
		}

	}
}
