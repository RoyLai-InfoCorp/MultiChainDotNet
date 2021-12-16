// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainBlockchain;
using MultiChainDotNet.Fluent.Signers;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainBlockchainManager : IMultiChainBlockchainManager
	{
		private readonly ILogger _logger;
		private readonly IMultiChainCommandFactory _commandFactory;
		private readonly MultiChainBlockchainCommand _bcCommand;
		private readonly MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;

		public MultiChainBlockchainManager(ILoggerFactory loggerFactory
			, IMultiChainCommandFactory commandFactory
			, MultiChainConfiguration mcConfig)
		{
			_logger = loggerFactory.CreateLogger<MultiChainBlockchainManager>();
			_commandFactory = commandFactory;
			_bcCommand = _commandFactory.CreateCommand<MultiChainBlockchainCommand>();
			_mcConfig = mcConfig;
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}
		public MultiChainBlockchainManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory commandFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_logger = loggerFactory.CreateLogger<MultiChainBlockchainManager>();
			_commandFactory = commandFactory;
			_bcCommand = _commandFactory.CreateCommand<MultiChainBlockchainCommand>();
			_mcConfig = mcConfig;
			_defaultSigner = signer;
		}

		public async Task<GetBlockResult> GetCurrentBlock()
		{
			var result = await _bcCommand.GetInfoAsync();
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}

			var blockHeight = result.Result.Blocks;
			var result2 = await _bcCommand.GetBlock(blockHeight);
			if (result2.IsError)
			{
				_logger.LogWarning(result2.Exception.ToString());
				throw result2.Exception;
			}
			return result2.Result;
		}

	}
}
