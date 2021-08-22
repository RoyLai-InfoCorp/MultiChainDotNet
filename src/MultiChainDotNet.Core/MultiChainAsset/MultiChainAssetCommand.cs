// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainAsset
{
	public class MultiChainAssetCommand : MultiChainCommandBase
	{
		public MultiChainAssetCommand(ILogger<MultiChainAssetCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogDebug($"Initialized MultiChainAssetCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public MultiChainAssetCommand(ILogger<MultiChainAssetCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogDebug($"Initialized MultiChainAssetCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public async Task<MultiChainResult<string>> IssueAssetAsync(string address, string assetName, double qty, double smallestUnit, bool open, double amt = 0)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));


			return await JsonRpcRequestAsync<string>("issue", address, new { name = assetName, open = open }, qty, smallestUnit, amt);
		}

		public async Task<MultiChainResult<string>> IssueAssetFromAsync(string from, string to, string assetName, double qty, double smallestUnit, bool open, double amt = 0)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));


			return await JsonRpcRequestAsync<string>("issuefrom", from, to, new { name = assetName, open = open }, qty, smallestUnit, amt);
		}


		public async Task<MultiChainResult<string>> IssueMoreAssetAsync(string address, string assetName, double qty, double amt = 0)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));


			return await JsonRpcRequestAsync<string>("issuemore", address, assetName, qty, amt);
		}
		public async Task<MultiChainResult<string>> IssueMoreAssetFromAsync(string from, string to, string assetName, double qty, double amt = 0)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));


			return await JsonRpcRequestAsync<string>("issuemorefrom", from, to, assetName, qty, amt);
		}


		public async Task<MultiChainResult<VoidType>> SubscribeAsync(string assetName)
		{
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));

			return await JsonRpcRequestAsync<VoidType>("subscribe", assetName, false);
		}

		/// <summary>
		/// Data sent with this transaction is line - means data and amt spent are encoded in the same vout.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="amt"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public async Task<MultiChainResult<string>> SendAsync(string address, double amt, string assetName = null, object data = null)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			if (assetName is null)
			{
				if (data is null)
					return await JsonRpcRequestAsync<string>("send", address, amt);
				return await JsonRpcRequestAsync<string>("send", address, new Dictionary<string, object> { { "", amt }, { "data", new { json = data } } });
			}
			if (data is null)
				return await JsonRpcRequestAsync<string>("send", address, new Dictionary<string, object> { { assetName, amt } });
			return await JsonRpcRequestAsync<string>("send", address, new Dictionary<string, object> { { assetName, amt }, { "data", new { json = data } } });
		}

		public async Task<MultiChainResult<string>> SendFromAsync(string from, string to, double amt, string assetName = null, object data = null)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));

			if (assetName is null)
			{
				if (data is null)
					return await JsonRpcRequestAsync<string>("sendfrom", from, to, amt);
				return await JsonRpcRequestAsync<string>("sendfrom", from, to, new Dictionary<string, object> { { "", amt }, { "data", new { json = data } } });
			}
			if (data is null)
				return await JsonRpcRequestAsync<string>("sendfrom", from, to, new Dictionary<string, object> { { assetName, amt } });
			return await JsonRpcRequestAsync<string>("sendfrom", from, to, new Dictionary<string, object> { { assetName, amt }, { "data", new { json = data } } });
		}



		/// <summary>
		/// Data sent with this transaction is not inline - means data and amt spent are encoded in different vout.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="amt"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public async Task<MultiChainResult<string>> SendWithDataAsync(string address, double amt, object data)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			return await JsonRpcRequestAsync<string>("sendwithdata", address, amt, new { json = data } );
		}

		public async Task<MultiChainResult<string>> SendWithDataFromAsync(string from, string to, double amt, object data)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));

			return await JsonRpcRequestAsync<string>("sendwithdatafrom", from, to, amt, new { json = data });
		}



		public async Task<MultiChainResult<List<ListAssetsResult>>> ListAssetsAsync(string assetName = "*", bool verbose = false)
		{
			return await JsonRpcRequestAsync<List<ListAssetsResult>>("listassets", assetName, verbose);
		}

		public async Task<MultiChainResult<List<GetAddressBalancesResult>>> GetAddressBalancesAsync(string address)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			return await JsonRpcRequestAsync<List<GetAddressBalancesResult>>("getaddressbalances", address);
		}


		public async Task<MultiChainResult<List<AssetTransactionsResult>>> ListAssetTransactionsAsync(string assetName)
		{
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));

			return await JsonRpcRequestAsync<List<AssetTransactionsResult>>("listassettransactions", assetName);
		}

		public async Task<MultiChainResult<GetAssetInfoResult>> GetAssetInfoAsync(string assetName)
		{
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));

			return await JsonRpcRequestAsync<GetAssetInfoResult>("getassetinfo", assetName);
		}

		public async Task<MultiChainResult<string>> SendAssetFromAsync(string from, string to, string assetName, double qty)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));
			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));


			return await JsonRpcRequestAsync<string>("sendassetfrom", from, to, assetName, qty);
		}

		public async Task<MultiChainResult<string>> SendAssetAsync(string address, string assetName, double qty)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			if (assetName is null)
				throw new ArgumentNullException(nameof(assetName));

			return await JsonRpcRequestAsync<string>("sendasset", address, assetName, qty);
		}

	}
}
