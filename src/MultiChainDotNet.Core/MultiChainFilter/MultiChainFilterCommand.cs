// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Core.MultiChainFilter
{
	public class MultiChainFilterCommand : MultiChainCommandBase
	{
		public MultiChainFilterCommand(ILogger<MultiChainFilterCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogTrace($"Initialized MultiChainPermissionCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public MultiChainFilterCommand(ILogger<MultiChainFilterCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogTrace($"Initialized MultiChainPermissionCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public async Task<MultiChainResult<TestFilterResult>> TestTxFilterAsync(string code, string txid = null, FilterOptions options = null)
		{
			if (String.IsNullOrEmpty(code))
				throw new ArgumentNullException(nameof(code));

			var result = await JsonRpcRequestAsync<TestFilterResult>("testtxfilter", options == null ? new object() : options, code);
			if (!String.IsNullOrEmpty(txid))
				result = await JsonRpcRequestAsync<TestFilterResult>("testtxfilter", options == null ? new object() : options, code, txid);

			if (result.IsError)
				return new MultiChainResult<TestFilterResult>(result.Exception);
			if (!result.Result.Compiled)
				return new MultiChainResult<TestFilterResult>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, result.Result.Reason));
			if (!result.Result.Passed)
				return new MultiChainResult<TestFilterResult>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, result.Result.Reason + ". " + result.Result.Callbacks?.ToJson()));
			return result;
		}

		public async Task<MultiChainResult<string>> CreateTxFilterAsync(string filterName, string code, FilterOptions options = null)
		{
			if (filterName is null)
				throw new ArgumentNullException(nameof(filterName));
			if (String.IsNullOrEmpty(code))
				throw new ArgumentNullException(nameof(code));

			return await JsonRpcRequestAsync<string>("create", "txfilter", filterName, options == null ? new object() : options, code);
		}

		public async Task<MultiChainResult<string>> CreateTxFilterAsync(string filterName, string code, string entity)
		{
			if (String.IsNullOrEmpty(filterName))
				throw new ArgumentNullException(nameof(filterName));
			if (String.IsNullOrEmpty(code))
				throw new ArgumentNullException(nameof(code));
			if (String.IsNullOrEmpty(entity))
				throw new ArgumentNullException(nameof(entity));

			return await JsonRpcRequestAsync<string>("create", "txfilter", filterName, entity, code);
		}

		public async Task<MultiChainResult<IList<TxFilterItem>>> ListTxFiltersAsync()
		{
			return await JsonRpcRequestAsync<IList<TxFilterItem>>("listtxfilters");
		}

		public async Task<MultiChainResult<TxFilterItem>> GetTxFilterAsync(string filterName)
		{
			var result = await JsonRpcRequestAsync<IList<TxFilterItem>>("listtxfilters");
			if (result.IsError)
				return new MultiChainResult<TxFilterItem>(result.Exception);
			var filter = result.Result.SingleOrDefault(x => x.Name == filterName);
			return new MultiChainResult<TxFilterItem>(filter);
		}

		public async Task<MultiChainResult<string>> GetFilterCodeAsync(string filterName)
		{
			if (String.IsNullOrEmpty(filterName))
				throw new ArgumentNullException(nameof(filterName));
			return await JsonRpcRequestAsync<string>("getfiltercode", filterName);
		}

		public async Task<MultiChainResult<string>> ApproveFromAsync(string address, string filterName, bool approve)
		{
			if (String.IsNullOrEmpty(filterName))
				throw new ArgumentNullException(nameof(filterName));
			if (String.IsNullOrEmpty(address))
				throw new ArgumentNullException(nameof(address));
			return await JsonRpcRequestAsync<string>("approvefrom", address, filterName, approve);
		}

		public Task<bool> WaitUntilFilterApproved(string filterName, bool approved, int retries = 5, int delay = 500)
		{
			return TaskHelper.WaitUntilTrueAsync(async () =>
			{
				var result = await GetTxFilterAsync(filterName);
				if (result.Result is null)
					return false;
				if (result.Result.Approved != approved)
					return false;
				return true;
			}
		  , retries, delay);
		}


	}
}
