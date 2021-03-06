// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class MultiChainStreamCommand : MultiChainCommandBase
	{
		public MultiChainStreamCommand(ILogger<MultiChainStreamCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogTrace($"Initialized MultiChainStreamCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public MultiChainStreamCommand(ILogger<MultiChainStreamCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogTrace($"Initialized MultiChainStreamCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public async Task<MultiChainResult<VoidType>> SubscribeAsync(string streamName, bool rescan = false)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<VoidType>("subscribe", streamName, rescan);
		}

		public Task<bool> WaitUntilStreamExists(string streamName)
		{
			return TaskHelper.WaitUntilTrueAsync(async () =>
			{
				var streams = await ListStreamsAsync();
				if (streams.Result == null)
					return false;
				return streams.Result.Any(x => x.Name == streamName);
			},
			5,
			500);
		}

		#region streams
		public async Task<MultiChainResult<string>> CreateStreamAsync(string streamName, bool isOpen = true)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<string>("create", "stream", streamName, isOpen);
		}

		public async Task<MultiChainResult<StreamsResult>> GetStreamInfoAsync(string streamName, bool verbose = false)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<StreamsResult>("getstreaminfo", streamName, verbose);
		}

		public async Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(bool verbose = false)
		{
			return await JsonRpcRequestAsync<List<StreamsResult>>("liststreams", "*", verbose);
		}


		public async Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(string streamName, bool verbose = true)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<List<StreamsResult>>("liststreams", streamName, verbose);
		}

		#endregion

		#region stream items
		public async Task<MultiChainResult<IList<StreamItemsResult>>> ListStreamItemsAsync(string streamName, bool verbose = false, int count = 10, int startFrom = -10)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<IList<StreamItemsResult>>("liststreamitems", streamName, verbose, count, startFrom);
		}

		public async Task<MultiChainResult<IList<StreamItemsResult>>> ListStreamItemsByKeyAsync(string streamName, string key, bool verbose = false, int count = 10, int startFrom = -10)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<IList<StreamItemsResult>>("liststreamkeyitems", streamName, key, verbose, count, startFrom);
		}

		public async Task<MultiChainResult<IList<StreamItemsResult>>> ListStreamItemsByPublisherAsync(string streamName, string address, bool verbose = false, int count = 10, int startFrom = -10)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<IList<StreamItemsResult>>("liststreampublisheritems", streamName, address, verbose, count, startFrom);
		}


		public async Task<MultiChainResult<string>> PublishHexadecimalStreamItemAsync(string streamName, string[] key, string hexadecimal)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<string>("publish", streamName, key, hexadecimal);
		}

		public async Task<MultiChainResult<string>> PublishTextStreamItemAsync(string streamName, string[] key, string text, string options = null)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			if (string.IsNullOrEmpty(options))
				return await JsonRpcRequestAsync<string>("publish", streamName, key, new { text = text });
			return await JsonRpcRequestAsync<string>("publish", streamName, key, new { text = text }, options);
		}

		public async Task<MultiChainResult<string>> PublishJsonStreamItemAsync<T>(string streamName, StreamItem<T> streamItem)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<string>("publish", streamName, streamItem.Keys, new { json = streamItem.Data });
		}
		public async Task<MultiChainResult<string>> PublishBinaryCacheStreamItemAsync(string streamName, string[] key, string binId, string options = null)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));
			if (string.IsNullOrEmpty(options))
				return await JsonRpcRequestAsync<string>("publish", streamName, key, new { cache = binId });
			return await JsonRpcRequestAsync<string>("publish", streamName, key, new { cache = binId }, options);
		}


		public async Task<MultiChainResult<string>> PublishJsonStreamItemAsync(string streamName, string[] key, object payload, string options = null)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));
			if (string.IsNullOrEmpty(options))
				return await JsonRpcRequestAsync<string>("publish", streamName, key, new { json = payload });
			return await JsonRpcRequestAsync<string>("publish", streamName, key, new { json = payload }, options);
		}

		public async Task<MultiChainResult<StreamItemsResult>> GetStreamItemByTxidAsync(string streamName, string txId)
		{
			if (streamName is null)
				throw new ArgumentNullException(nameof(streamName));

			return await JsonRpcRequestAsync<StreamItemsResult>("getstreamitem", streamName, txId);
		}

		public Task<MultiChainResult<string>> GetTxOutDataAsync(string txid, int vout)
		{
			return JsonRpcRequestAsync<string>("gettxoutdata", txid, vout);
		}

		#endregion

	}
}
