using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
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
			Assert.That(result2.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_TRANSACTION_REJECTED));
			//Assert.That(result2.ErrorCode, Is.EqualTo(MultiChainErrorCode.RPC_DUPLICATE_NAME),result2.ExceptionMessage);
			Assert.That(result2.ExceptionMessage, Contains.Substring("New entity script rejected - entity with this name already exists."), result2.ExceptionMessage);
		}

	}
}
