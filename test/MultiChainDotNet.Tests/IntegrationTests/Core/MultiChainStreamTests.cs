// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainBinary;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
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
		MultiChainBinaryCommand _mcBinCmd;

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
			_mcBinCmd = _container.GetRequiredService<MultiChainBinaryCommand>();
		}

		[Test, Order(10)]
		public async Task T0610_should_be_able_to_create_new_stream_and_get_by_name()
		{
			_stateDb.ClearState<TestState>();

			var randomName = RandomName();

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

		[Test, Order(20),Ignore("")]
		public async Task T0620_should_throw_error_if_not_subscribed_to_stream()
		{
			var state = _stateDb.GetState<TestState>();

			// ACT
			var result = await _streamCmd.ListStreamItemsAsync(state.StreamName);

			// ASSERT
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_NOT_SUBSCRIBED));

		}


		[Test, Order(30)]
		public async Task T0630_should_not_throw_error_after_subscribed_to_stream()
		{
			var state = _stateDb.GetState<TestState>();

			// ACT
			await _streamCmd.SubscribeAsync(state.StreamName);

			// ASSERT
			var result = await _streamCmd.ListStreamItemsAsync(state.StreamName);
			Assert.That(result.IsError, Is.False);
		}


		[Test, Order(40)]
		public async Task T0640_throw_error_stream_with_this_name_not_found()
		{
			var state = _stateDb.GetState<TestState>();
			var randomName = RandomName();
			var result = await _streamCmd.ListStreamsAsync(randomName);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_ENTITY_NOT_FOUND));

		}

		[Test, Order(50)]
		public async Task T0650_throw_error_stream_already_exists()
		{
			var state = _stateDb.GetState<TestState>();
			var result = await _streamCmd.CreateStreamAsync(state.StreamName);
			Assert.That(result.IsError, Is.True);
			Assert.That(result.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_DUPLICATE_NAME));
		}

		//--------- streamitems
		[Test, Order(60)]
		public async Task T0660_be_able_to_publish_hexadecimal_streamitem()
		{
			var state = _stateDb.GetState<TestState>();
			await _streamCmd.WaitUntilStreamExists(state.StreamName);

			// ACT
			var key1 = RandomName();
			var key2 = RandomName();
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
		public async Task T0670_list_all_streamitems()
		{
			var state = _stateDb.GetState<TestState>();

			var result = await _streamCmd.ListStreamItemsAsync(state.StreamName);
			Console.WriteLine(result.ToJson());
		}


		[Test, Order(80)]
		public async Task T0680_publish_text_streamitem()
		{
			var state = _stateDb.GetState<TestState>();
			await _streamCmd.WaitUntilStreamExists(state.StreamName);

			var key1 = RandomName();
			var key2 = RandomName();

			// ACT
			var result = await _streamCmd.PublishTextStreamItemAsync(state.StreamName, new string[] { key1, key2 }, "Hello World");
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			await _streamCmd.SubscribeAsync(state.StreamName);
			var item = await _streamCmd.GetStreamItemByTxidAsync(state.StreamName, result.Result);
			Assert.That(JObject.FromObject(item.Result.Data).SelectToken("text").ToString(), Is.EqualTo("Hello World"));
		}

		[Test, Order(90)]
		public async Task T0690_publish_Json_streamitem()
		{
			var state = _stateDb.GetState<TestState>();
			await _streamCmd.WaitUntilStreamExists(state.StreamName);

			var key1 = RandomName();
			var key2 = RandomName();

			// ACT
			var result = await _streamCmd.PublishJsonStreamItemAsync(state.StreamName, new string[] { key1, key2 }, new { Id = Guid.NewGuid(), Greetings = "Hello World" });
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			await _streamCmd.SubscribeAsync(state.StreamName);
			var item = await _streamCmd.GetStreamItemByTxidAsync(state.StreamName, result.Result);
			Assert.That(JObject.FromObject(item.Result.Data).SelectToken("json.Greetings").ToString(), Is.EqualTo("Hello World"));
		}

		[Test, Order(90)]
		public async Task T0692_publish_Json_streamitem_offchain()
		{
			var randomName = RandomName();

			// PREPARE
			await _streamCmd.CreateStreamAsync(randomName);
			await _streamCmd.WaitUntilStreamExists(randomName);
			var key1 = RandomName();

			// ACT
			var result = await _streamCmd.PublishJsonStreamItemAsync(randomName, new string[] { key1 }, new { Id = Guid.NewGuid(), Greetings = "Hello World" },"offchain");
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			// ASSERT: seed node
			await _streamCmd.SubscribeAsync(randomName);
			var item = await _streamCmd.GetStreamItemByTxidAsync(randomName, result.Result);
			Assert.That(JObject.FromObject(item.Result.Data).SelectToken("json.Greetings").ToString(), Is.EqualTo("Hello World"));
			// Offchain data should be found in .multichain/chain1/chunks/data/stream-{randomName}

			// ASSERT: relayer1
			var http = _container.GetRequiredService<IHttpClientFactory>().CreateClient(_relayer1.NodeName);
			var relayer1Cmd = new MultiChainStreamCommand(NullLogger<MultiChainStreamCommand>.Instance, _mcConfig, http);
			await relayer1Cmd.SubscribeAsync(randomName);
			var wait = await TaskHelper.WaitUntilTrue(async () => (await relayer1Cmd.GetStreamItemByTxidAsync(randomName, result.Result))?.Result is { });
			wait.Should().BeTrue();
			var item2 = await relayer1Cmd.GetStreamItemByTxidAsync(randomName, result.Result);
			Assert.That(JObject.FromObject(item2.Result.Data).SelectToken("json.Greetings").ToString(), Is.EqualTo("Hello World"));
			// Offchain data should be found in .multichain/chain1/chunks/data/stream-{randomName}

		}

		[Test, Order(90)]
		public async Task T0694_publish_Json_BinaryCache_offchain()
		{
			var randomName = RandomName();

			// PREPARE
			await _streamCmd.CreateStreamAsync(randomName);
			await _streamCmd.WaitUntilStreamExists(randomName);
			var binId = (await _mcBinCmd.CreateBinaryCache()).Result;
			Console.WriteLine("bin-cache:"+binId);
			var data = File.ReadAllText("image.dat");
			await _mcBinCmd.AppendBinaryCache(binId, data);

			// ACT
			var key1 = RandomName();
			var result = await _streamCmd.PublishBinaryCacheStreamItemAsync(randomName, new string[] { key1 },binId,"offchain");
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			// ASSERT: on node1
			await _streamCmd.SubscribeAsync(randomName);
			var item = await _streamCmd.GetStreamItemByTxidAsync(randomName, result.Result);
			var size = int.Parse(((JObject)item.Result.Data).SelectToken("size").ToString());
			var txid = ((JObject)item.Result.Data).SelectToken("txid").ToString();
			var vout = int.Parse(((JObject)item.Result.Data).SelectToken("vout").ToString());
			Console.WriteLine($"txid:{txid} vout:{vout} size:{size}");
			size.Should().Be(2354570);

			// Get result from blockchain
			var txoutResult = await _streamCmd.GetTxOutData(txid, vout);
			txoutResult.Result.Hex2Bytes().Length.Should().Be(2354570);

			await _mcBinCmd.TxoutToBinaryCache(binId, txid, vout, 2354570, 0);
			// File should be created in /.multichain/chain1/cache/{binID} on node1

			// ASSERT: relayer1
			var http = _container.GetRequiredService<IHttpClientFactory>().CreateClient(_relayer1.NodeName);
			var relayer1Cmd = new MultiChainStreamCommand(NullLogger<MultiChainStreamCommand>.Instance, _mcConfig, http);

			// Get result from blockchain
			var txoutResult2 = await relayer1Cmd.GetTxOutData(txid, vout);
			txoutResult2.Result.Hex2Bytes().Length.Should().Be(2354570);

			// Download result into binary cache
			var binCmd2 = new MultiChainBinaryCommand(NullLogger<MultiChainBinaryCommand>.Instance, _mcConfig, http);
			var binId2 = (await binCmd2.CreateBinaryCache()).Result;
			var result2 = await binCmd2.TxoutToBinaryCache(binId2, txid, vout, 2354570, 0);
			if (result2.IsError)
				throw result2.Exception;
			// File should be created in /.multichain/chain1/cache/{binID} on relayer1

		}

	}
}
