using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainFilter;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainTransaction;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.IntegrationTests.Core
{
	public class MultiChainFilterTests : TestBase
	{
		private MultiChainStreamCommand _streamCmd;
		MultiChainFilterCommand _filterCmd;
		private MultiChainAssetCommand _assetCmd;
		private MultiChainTransactionCommand _txnCmd;

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddMultiChain();
		}

		[SetUp]
		public async Task SetUp()
		{
			_streamCmd = _container.GetRequiredService<MultiChainStreamCommand>();
			_filterCmd = _container.GetRequiredService<MultiChainFilterCommand>();
			_assetCmd = _container.GetRequiredService<MultiChainAssetCommand>();
			_txnCmd = _container.GetRequiredService<MultiChainTransactionCommand>();
		}

		[Test]
		public async Task should_be_able_to_create_txfilter()
		{
			var result = await _filterCmd.CreateTxFilterAsync("filter1", "function filtertransaction() { var tx=getfiltertransaction(); if (tx.create) return 'Stream creation temporarily disabled';}");
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
			Console.WriteLine(result.ToJson());
		}

		[Test]
		public async Task should_be_able_to_listtxfilter()
		{
			var list = await _filterCmd.ListTxFiltersAsync();
			Assert.That(list.Result[0].Name, Is.EqualTo("filter1"));
			Console.WriteLine(list.ToJson());
		}

		[Test]
		public async Task should_be_able_to_getfiltercode()
		{
			var result = await _filterCmd.GetFilterCodeAsync("filter1");
			Console.WriteLine(result.ToJson());
		}

		[Test]
		public async Task should_be_able_to_approveFilter()
		{
			var filterCmd = new MultiChainFilterCommand(NullLogger<MultiChainFilterCommand>.Instance, _mcConfig, CreateHttpClient(_admin));
			var result = await filterCmd.ApproveFromAsync(_admin.NodeWallet, "filter1", true);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage); filterCmd = new MultiChainFilterCommand(NullLogger<MultiChainFilterCommand>.Instance, _mcConfig, CreateHttpClient(_admin));

			filterCmd = new MultiChainFilterCommand(NullLogger<MultiChainFilterCommand>.Instance, _mcConfig, CreateHttpClient(_relayer1));
			result = await filterCmd.ApproveFromAsync(_relayer1.NodeWallet, "filter1", true);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			await filterCmd.WaitUntilFilterApproved("filter1", true);

			var streamCmd = new MultiChainStreamCommand(NullLogger<MultiChainStreamCommand>.Instance, _mcConfig, CreateHttpClient(_admin));
			var randomName = RandomName();
			var result2 = await streamCmd.CreateStreamAsync(randomName);
			Assert.That(result2.IsError, Is.True);
			Assert.That(result2.ExceptionMessage, Contains.Substring("Stream creation temporarily disabled"));
		}

		[Test]
		public async Task should_be_able_to_disapproveFilter()
		{
			var filterCmd = new MultiChainFilterCommand(NullLogger<MultiChainFilterCommand>.Instance, _mcConfig, CreateHttpClient(_admin));
			var result = await filterCmd.ApproveFromAsync(_admin.NodeWallet, "filter1", false);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage); filterCmd = new MultiChainFilterCommand(NullLogger<MultiChainFilterCommand>.Instance, _mcConfig, CreateHttpClient(_admin));

			filterCmd = new MultiChainFilterCommand(NullLogger<MultiChainFilterCommand>.Instance, _mcConfig, CreateHttpClient(_relayer1));
			result = await filterCmd.ApproveFromAsync(_relayer1.NodeWallet, "filter1", false);
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);

			await filterCmd.WaitUntilFilterApproved("filter1", false);

			var streamCmd = new MultiChainStreamCommand(NullLogger<MultiChainStreamCommand>.Instance, _mcConfig, CreateHttpClient(_admin));
			var randomName = RandomName();
			var result2 = await streamCmd.CreateStreamAsync(randomName);
			Assert.That(result2.IsError, Is.False, result2.ExceptionMessage);
		}

		[Test]
		public async Task should_be_able_to_test_complex_filter()
		{
			// Create asset
			var assetName = RandomName();
			var result1 = await _assetCmd.IssueAssetFromAsync(_admin.NodeWallet, _admin.NodeWallet, assetName, 1000, 1, true);
			if (result1.IsError)
				throw new Exception(result1.ExceptionMessage);

			// Create stream
			var streamName = RandomName();
			var result2 = await _streamCmd.CreateStreamAsync(streamName);
			if (result2.IsError)
				throw new Exception(result1.ExceptionMessage);
			await _streamCmd.WaitUntilStreamExists(streamName);

			// Publish stream with asset
			var result3 = await _txnCmd.CreateRawSendFromAsync(
			  _admin.NodeWallet,
			  new Dictionary<string, object>
			  {
				{
					_admin.NodeWallet,
					new Dictionary<string, object>{ { assetName, 1 } }
				}
			  },
			  new object[] {
					new Dictionary<string, object>
					{
						{ "for", streamName },
						{ "key", "11111"},
						{ "data", new { json = new { name = "test", description = "hello world" } } }
					}
			  },
			  "send");
			if (result3.IsError) throw result3.Exception;
			Console.WriteLine("txid=" + result3.Result);

			var filterCmd = new MultiChainFilterCommand(NullLogger<MultiChainFilterCommand>.Instance, _mcConfig, CreateHttpClient(_admin));
			var result = await filterCmd.TestTxFilterAsync("function filtertransaction(){" +
			"const tx=getfiltertransaction();" +
			$"const found = tx.vout[0].assets.find(x=>x.name==='{assetName}');" +
			$"if (!found) return 'Asset not found';" +
			"}", result3.Result);
			Console.WriteLine(result.ToJson());
			Assert.That(result.IsError, Is.False, result.ExceptionMessage);
		}

	}
}
