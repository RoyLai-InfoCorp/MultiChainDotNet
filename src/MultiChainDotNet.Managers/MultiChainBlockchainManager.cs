// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainBlockchain;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainBlockchainManager : IMultiChainBlockchainManager
	{
		private IServiceProvider _container;
		private ILogger<MultiChainBlockchainManager> _logger;
		private MultiChainConfiguration _mcConfig;
		private SignerBase _defaultSigner;

		public MultiChainBlockchainManager(IServiceProvider container)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainBlockchainManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainBlockchainManager(IServiceProvider container, SignerBase signer)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainBlockchainManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = signer;
		}


		public async Task<GetBlockResult> GetCurrentBlock()
		{
			using (var scope = _container.CreateScope())
			{
				var bcCommand = scope.ServiceProvider.GetRequiredService<MultiChainBlockchainCommand>();
				var result = await bcCommand.GetInfoAsync();
				if (result.IsError)
				{
					_logger.LogWarning(result.Exception.ToString());
					throw result.Exception;
				}

				var blockHeight = result.Result.Blocks;
				var result2 = await bcCommand.GetBlockAsync(blockHeight);
				if (result2.IsError)
				{
					_logger.LogWarning(result2.Exception.ToString());
					throw result2.Exception;
				}
				return result2.Result;

			}

		}

	}
}
