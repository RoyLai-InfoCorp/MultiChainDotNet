using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainStreamManager
	{
		Task<MultiChainResult<string>> CreateStreamAsync(SignerBase signer, string fromAddress, string streamName, bool anyoneCanWrite = false);
		Task<MultiChainResult<string>> CreateStreamAsync(string streamName, bool anyoneCanWrite = false);
		Task<MultiChainResult<StreamsResult>> GetStreamAsync(string streamName);
		Task<MultiChainResult<StreamItemsResult>> GetStreamItemAsync(string selectCmd);
		Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(bool verbose = false);
		Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(string streamName, bool verbose = false);
		Task<MultiChainResult<string>> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string key, object json);
		Task<MultiChainResult<string>> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string[] keys, object json);
		Task<MultiChainResult<string>> PublishJsonAsync(string streamName, string key, object json);
		Task<MultiChainResult<string>> PublishJsonAsync(string streamName, string[] keys, object json);
		Task<MultiChainResult<IList<StreamItemsResult>>> ListStreamItemsAsync(string selectCmd, bool verbose=false);
		Task<MultiChainResult<List<T>>> ListStreamItemsAsync<T>(string selectCmd);
		Task<MultiChainResult<VoidType>> SubscribeAsync(string streamName);
		Task<MultiChainResult<T>> GetStreamItemAsync<T>(string selectCmd);
	}
}