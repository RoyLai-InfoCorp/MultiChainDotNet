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

		public async Task<MultiChainResult<GetBlockResult>> GetCurrentBlock()
		{
			var infoResult = await _bcCommand.GetInfoAsync();
			if (infoResult.IsError)
				return new MultiChainResult<GetBlockResult>(infoResult.Exception);

			var blockHeight = infoResult.Result.Blocks;
			return await _bcCommand.GetBlock(blockHeight);
		}

	}
}
