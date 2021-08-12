﻿using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests.Managers
{
	[TestFixture]
    public class MultiChainStreamManagerTests : TestCommandFactoryBase
    {
		IMultiChainStreamManager _streamManager;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services.AddTransient<IMultiChainStreamManager, MultiChainStreamManager>();
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
			var result = await _streamManager.CreateStreamAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, streamName);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;
			var found = await _streamManager.GetStreamAsync(streamName);
			Assert.That(found.IsError, Is.False, result.ExceptionMessage);
			Assert.That(found.Result.Name, Is.EqualTo(streamName));
		}

		[Test]
		public async Task Should_be_able_to_publish_new_streamitem()
		{
			var streamName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var result = await _streamManager.CreateStreamAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, streamName);

			// ASSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			var txid = result.Result;
			var found = await _streamManager.GetStreamAsync(streamName);
			Assert.That(found.IsError, Is.False, result.ExceptionMessage);
			Assert.That(found.Result.Name, Is.EqualTo(streamName));
		}

		[Test, Order(50)]
		public async Task Should_throw_rpc_transaction_rejected_error_when_stream_already_exists()
		{
			var streamName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
			var result = await _streamManager.CreateStreamAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, streamName);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			var result2 = await _streamManager.CreateStreamAsync(new DefaultSigner(_admin.Ptekey), _admin.NodeWallet, streamName);
			Assert.That(result2.IsError, Is.True);
			Assert.That(result2.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_DUPLICATE_NAME),result2.ExceptionMessage);
		}


		public class JsonStreamItem
		{
			public int Counter;
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
			var result1 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} ASC", false);
			Assert.That(result1.Result[0].Counter, Is.EqualTo(0));

			// Get last item
			var result2 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream}", false);
			Assert.That(result2.Result[0].Counter, Is.EqualTo(9));

		}

		[Test, Order(70)]
		public async Task Should_list_published_Json_streamitems_with_paging()
		{
			var newStream = await CreateNewStream();

			// FROM <newStream> DESC PAGE 0 SIZE 2
			var result7 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} PAGE 0 SIZE 2", false);
			Assert.That(result7.Result[0].Counter, Is.EqualTo(9));
			Assert.That(result7.Result[1].Counter, Is.EqualTo(8));
			// FROM <newStream> DESC PAGE 1 SIZE 2
			var result8 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} PAGE 1 SIZE 2", false);
			Assert.That(result8.Result[0].Counter, Is.EqualTo(7));
			Assert.That(result8.Result[1].Counter, Is.EqualTo(6));
			// FROM <newStream> DESC PAGE 2 SIZE 2
			var result9 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} PAGE 2 SIZE 2", false);
			Assert.That(result9.Result[0].Counter, Is.EqualTo(5));
			Assert.That(result9.Result[1].Counter, Is.EqualTo(4));

			// FROM <newStream> DESC PAGE 0 SIZE 2
			var result10 = await _streamManager.ListStreamItemsAsync<JsonStreamItem>($"FROM {newStream} ASC PAGE 0 SIZE 2", false);
			Assert.That(result10.Result[0].Counter, Is.EqualTo(0));
			Assert.That(result10.Result[1].Counter, Is.EqualTo(1));

			// Fluent Syntax: Query(streamName).FromLast(0).Page(2).Size(2) returns an array of 2
			// Fluent Syntax: Query(streamName).FromLast(0).Next(2).Next(2).Next(2) returns a list of 3 x 2

		}

	}
}
