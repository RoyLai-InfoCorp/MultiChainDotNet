using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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

		public async Task<MultiChainResult<VoidType>> SubscribeAsync(string streamName)
		{
			return await JsonRpcRequestAsync<VoidType>("subscribe", streamName, false);
		}

		#region streams
		public async Task<MultiChainResult<string>> CreateStreamAsync(string streamName, bool isOpen = true)
		{
			return await JsonRpcRequestAsync<string>("create", "stream", streamName, isOpen);
		}

		public async Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(bool verbose = false)
		{
			return await JsonRpcRequestAsync<List<StreamsResult>>("liststreams", "*", verbose);
		}


		public async Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(string streamName, bool verbose = true)
		{
			return await JsonRpcRequestAsync<List<StreamsResult>>("liststreams", streamName, verbose);
		}

		#endregion

		#region stream items
		public async Task<MultiChainResult<IList<StreamItemsResult>>> ListStreamItemsAsync(string streamName, bool verbose = false, int count = 10, int startFrom = -10)
		{
			return await JsonRpcRequestAsync<IList<StreamItemsResult>>("liststreamitems", streamName, verbose, count, startFrom);
		}

		public async Task<MultiChainResult<IList<StreamItemsResult>>> ListStreamItemsByKeyAsync(string streamName, string key, bool verbose = false, int count = 10, int startFrom = -10)
		{
			return await JsonRpcRequestAsync<IList<StreamItemsResult>>("liststreamkeyitems", streamName, key, verbose, count, startFrom);
		}

		public async Task<MultiChainResult<IList<StreamItemsResult>>> ListStreamItemsByPublisherAsync(string streamName, string address, bool verbose = false, int count = 10, int startFrom = -10)
		{
			return await JsonRpcRequestAsync<IList<StreamItemsResult>>("liststreampublisheritems", streamName, address, verbose, count, startFrom);
		}


		public async Task<MultiChainResult<string>> PublishHexadecimalStreamItemAsync(string streamName, string[] key, string hexadecimal)
		{
			return await JsonRpcRequestAsync<string>("publish", streamName, key, hexadecimal);
		}

		public async Task<MultiChainResult<string>> PublishTextStreamItemAsync(string streamName, string[] key, string text)
		{
			return await JsonRpcRequestAsync<string>("publish", streamName, key, new { text = text });
		}

		public async Task<MultiChainResult<string>> PublishJsonStreamItemAsync<T>(string streamName, StreamItem<T> streamItem)
		{
			return await JsonRpcRequestAsync<string>("publish", streamName, streamItem.Keys, new { json = streamItem.Data });
		}

		public async Task<MultiChainResult<string>> PublishJsonStreamItemAsync(string streamName, string[] key, object payload)
		{
			return await JsonRpcRequestAsync<string>("publish", streamName, key, new { json = payload });
		}

		public async Task<MultiChainResult<StreamItemsResult>> GetStreamItemByTxidAsync(string streamName, string txId)
		{
			return await JsonRpcRequestAsync<StreamItemsResult>("getstreamitem", streamName, txId);
		}


		#endregion

	}
}
