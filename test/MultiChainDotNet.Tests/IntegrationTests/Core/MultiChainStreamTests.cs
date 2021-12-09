// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainStream;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
	[TestFixture]
	public class MultiChainStreamTests : TestBase
	{
		public class TestState
		{
			public Guid Id { get; set; }
			public string Txid { get; set; }
			public string StreamName { get; set; }

			public string Key1 { get; set; }
			public string Key2 { get; set; }

		}

		MultiChainStreamCommand _streamCmd;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddMultiChain()
				;
		}

		[SetUp]
		public async Task SetUp()
		{
			_streamCmd = _container.GetRequiredService<MultiChainStreamCommand>();
		}

		[Test, Order(10)]
		public async Task Should_be_able_to_create_new_stream_and_get_by_name()
		{
			_stateDb.ClearState<TestState>();

			var randomName = Guid.NewGuid().ToString("N").Substring(20);

			// ACT
			var txid = await _streamCmd.CreateStreamAsync(randomName);
			Assert.That(txid.IsError, Is.False, txid.ExceptionMessage);

			// ASSET
			var list = await _streamCmd.ListStreamsAsync(randomName);
			Assert.That(list.IsError, Is.False, list.ExceptionMessage);

			var names = list.Result.Select(x => x.Name).ToArray();
			Assert.That(names, Has.Member(randomName));

			_stateDb.SaveState(new TestState { StreamName = randomName, Txid = txid.Result });
		}

		[Test, Order(20)]
		public async Task Should_throw_error_if_not_subscribed_to_stream()
		{
			var state = _stateDb.GetState<TestState>();

			// ACT
			var result = await _streamCmd.ListStreamItemsAsync(state.StreamName);

			// ASSERT
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_NOT_SUBSCRIBED));

		}


		[Test, Order(30)]
		public async Task Should_not_throw_error_after_subscribed_to_stream()
		{
			var state = _stateDb.GetState<TestState>();

			// ACT
			await _streamCmd.SubscribeAsync(state.StreamName);

			// ASSERT
			var result = await _streamCmd.ListStreamItemsAsync(state.StreamName);
			Assert.That(result.IsError, Is.False);
		}


		[Test, Order(40)]
		public async Task Should_throw_error_stream_with_this_name_not_found()
		{
			var state = _stateDb.GetState<TestState>();
			var randomName = Guid.NewGuid().ToString("N").Substring(20);
			var result = await _streamCmd.ListStreamsAsync(randomName);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_ENTITY_NOT_FOUND));

		}

		[Test, Order(50)]
		public async Task Should_throw_error_stream_already_exists()
		{
			var state = _stateDb.GetState<TestState>();
			var result = await _streamCmd.CreateStreamAsync(state.StreamName);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_DUPLICATE_NAME));
		}

		//--------- streamitems
		[Test, Order(60)]
		public async Task Should_be_able_to_publish_hexadecimal_streamitem()
		{
			var state = _stateDb.GetState<TestState>();

			// ACT
			var key1 = Guid.NewGuid().ToString("N").Substring(20);
			var key2 = Guid.NewGuid().ToString("N").Substring(20);
			var result = await _streamCmd.PublishHexadecimalStreamItemAsync(state.StreamName, new string[] { key1, key2 }, "a1b2c3d4");

			// ASWSERT
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			await _streamCmd.SubscribeAsync(state.StreamName);

			var item = await _streamCmd.GetStreamItemByTxidAsync(state.StreamName, result.Result);
			Assert.That(item.Result.TxId, Is.EqualTo(result.Result));

			var item2 = await _streamCmd.ListStreamItemsByKeyAsync(state.StreamName, key1);
			Assert.That(item2.Result[0].Keys, Has.Member(key1));
			Assert.That(item2.Result[0].Keys, Has.Member(key2));

			state.Key1 = key1;
			state.Key2 = key2;
			_stateDb.SaveState(state);
		}



		[Test, Order(70)]
		public async Task Should_list_all_streamitems()
		{
			var state = _stateDb.GetState<TestState>();

			var result = await _streamCmd.ListStreamItemsAsync(state.StreamName);
			Console.WriteLine(result.ToJson());
		}


		[Test, Order(80)]
		public async Task Should_publish_text_streamitem()
		{
			var state = _stateDb.GetState<TestState>();

			var key1 = Guid.NewGuid().ToString("N").Substring(20);
			var key2 = Guid.NewGuid().ToString("N").Substring(20);

			// ACT
			var result = await _streamCmd.PublishTextStreamItemAsync(state.StreamName, new string[] { key1, key2 }, "Hello World");
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			await _streamCmd.SubscribeAsync(state.StreamName);
			var item = await _streamCmd.GetStreamItemByTxidAsync(state.StreamName, result.Result);
			Assert.That(JObject.FromObject(item.Result.Data).SelectToken("text").ToString(), Is.EqualTo("Hello World"));
		}

		[Test, Order(90)]
		public async Task Should_publish_Json_streamitem()
		{
			var state = _stateDb.GetState<TestState>();

			var key1 = Guid.NewGuid().ToString("N").Substring(20);
			var key2 = Guid.NewGuid().ToString("N").Substring(20);

			// ACT
			var result = await _streamCmd.PublishJsonStreamItemAsync(state.StreamName, new string[] { key1, key2 }, new { Id = Guid.NewGuid(), Greetings = "Hello World" });
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			await _streamCmd.SubscribeAsync(state.StreamName);
			var item = await _streamCmd.GetStreamItemByTxidAsync(state.StreamName, result.Result);
			Assert.That(JObject.FromObject(item.Result.Data).SelectToken("json.Greetings").ToString(), Is.EqualTo("Hello World"));
		}

	}
}
