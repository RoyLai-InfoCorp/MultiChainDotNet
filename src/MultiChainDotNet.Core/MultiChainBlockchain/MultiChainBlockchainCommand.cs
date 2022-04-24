// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using System;
using System.Collections.Generic;
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

		public async Task<MultiChainResult<IList<GetPeerInfoResult>>> GetPeerInfoAsync()
		{
			return await JsonRpcRequestAsync<IList<GetPeerInfoResult>>("getpeerinfo");
		}

		public async Task<MultiChainResult<GetBlockResult>> GetBlockAsync(string blockHash)
		{
			return await JsonRpcRequestAsync<GetBlockResult>("getblock", blockHash);
		}

		public async Task<MultiChainResult<GetBlockResult>> GetBlockAsync(UInt64 height)
		{
			return await JsonRpcRequestAsync<GetBlockResult>("getblock", height);
		}

		public async Task<MultiChainResult<GetBlockResult>> GetLastBlockInfoAsync(UInt64 skip = 0)
		{
			if (skip == 0)
				return await JsonRpcRequestAsync<GetBlockResult>("getlastblockinfo");
			return await JsonRpcRequestAsync<GetBlockResult>("getlastblockinfo", skip);
		}

		public async Task<MultiChainResult<IList<GetBlockResult>>> ListBlocksAsync(UInt64 blockFrom, UInt64 blockTo, bool verbose = false)
		{
			if (verbose)
				return await JsonRpcRequestAsync<IList<GetBlockResult>>("listblocks", $"{blockFrom}-{blockTo}");
			return await JsonRpcRequestAsync<IList<GetBlockResult>>("listblocks", $"{blockFrom}-{blockTo}", verbose);
		}

	}
}
