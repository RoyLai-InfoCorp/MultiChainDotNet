// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainVariable
{
    public class MultiChainVariableCommand : MultiChainCommandBase
    {
		public MultiChainVariableCommand(ILogger<MultiChainVariableCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogTrace($"Initialized MultiChainVariableCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public MultiChainVariableCommand(ILogger<MultiChainVariableCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogTrace($"Initialized MultiChainVariableCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		/// <summary>
		/// eg. create variable foo
		/// output:
		/// txid
		/// </summary>
		public async Task<MultiChainResult<string>> CreateVariableAsync(string variableName)
		{
			return await JsonRpcRequestAsync<string>("create","variable", variableName);
		}

		/// <summary>
		/// eg. setvariablevalue foo bar
		/// output:
		/// txid
		/// </summary>
		public async Task<MultiChainResult<string>> SetVariableValueAsync(string variableName, object variableValue)
		{
			return await JsonRpcRequestAsync<string>("setvariablevalue", variableName,variableValue);
		}

		/// <summary>
		/// eg. getvariablevalue foo
		/// output:
		/// bar
		/// </summary>
		public async Task<MultiChainResult<T>> GetVariableValueAsync<T>(string variableName)
		{
			return await JsonRpcRequestAsync<T>("getvariablevalue", variableName);
		}
    }
}
