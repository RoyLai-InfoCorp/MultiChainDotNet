using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainBlockchain;
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
    public class MultiChainBlockchainTests : TestBase
    {
		ILogger _logger;
		MultiChainBlockchainCommand _bcCmd;
		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services.AddTransient<MultiChainBlockchainCommand>();
		}

		[SetUp]
		public void SetUp()
		{
			_bcCmd = _container.GetRequiredService<MultiChainBlockchainCommand>();
			_logger = _container.GetRequiredService<ILogger<MultiChainBlockchainTests>>();
		}

		[Test]
		public async Task Should_get_latest_block_details()
		{
			var chain = await _bcCmd.GetInfoAsync();
			Assert.That(chain.IsError, Is.False, chain.ExceptionMessage);
			_logger.LogInformation(JsonConvert.SerializeObject(chain));

			var block = await _bcCmd.GetBlock(chain.Result.Blocks);
			Assert.That(block.IsError, Is.False, block.ExceptionMessage);
			_logger.LogInformation(JsonConvert.SerializeObject(block));
		}

		[Test]
		public async Task Should_get_last_miner_address()
		{
			var chain = await _bcCmd.GetInfoAsync();
			Assert.That(chain.IsError, Is.False, chain.ExceptionMessage);
			_logger.LogInformation(JsonConvert.SerializeObject(chain));

			var block = await _bcCmd.GetBlock(chain.Result.Blocks);
			Assert.That(block.IsError, Is.False, block.ExceptionMessage);

			_logger.LogInformation(JsonConvert.SerializeObject(block.Result.Miner));
		}



	}
}
