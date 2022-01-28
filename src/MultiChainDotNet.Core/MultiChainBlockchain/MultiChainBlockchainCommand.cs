// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainBlockchain
{
	public class MultiChainBlockchainCommand : MultiChainCommandBase
	{
		public MultiChainBlockchainCommand(ILogger<MultiChainBlockchainCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogTrace($"Initialized MultiChainPermissionCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public MultiChainBlockchainCommand(ILogger<MultiChainBlockchainCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogTrace($"Initialized MultiChainPermissionCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public async Task<MultiChainResult<GetInfoResult>> GetInfoAsync()
		{
			return await JsonRpcRequestAsync<GetInfoResult>("getinfo");
		}

		public async Task<MultiChainResult<GetBlockResult>> GetBlockAsync(string blockHash)
		{
			return await JsonRpcRequestAsync<GetBlockResult>("getblock", blockHash);
		}

		public async Task<MultiChainResult<GetBlockResult>> GetBlockAsync(UInt64 height)
		{
			return await JsonRpcRequestAsync<GetBlockResult>("getblock", height);
		}

	}
}
