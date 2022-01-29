// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainStreamManager
	{
		Task<string> CreateStreamAsync(SignerBase signer, string fromAddress, string streamName, bool anyoneCanWrite = false);
		Task<string> CreateStreamAsync(string streamName, bool anyoneCanWrite = false);
		Task<StreamsResult> GetStreamInfoAsync(string streamName);
		Task<StreamItemsResult> GetStreamItemAsync(string selectCmd);
		Task<T> GetStreamItemAsync<T>(string selectCmd);
		Task<bool> IsExistAsync(string streamName);
		Task<IList<T>> ListAllStreamItemsAsync<T>(string streamName);
		Task<IList<StreamItemsResult>> ListStreamItemsAsync(string selectCmd, bool verbose = false);
		Task<IList<T>> ListStreamItemsAsync<T>(string selectCmd);
		Task<IList<StreamsResult>> ListStreamsAsync(bool verbose = false);
		Task<IList<StreamsResult>> ListStreamsAsync(string streamName, bool verbose = false);
		Task<string> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string key, object json);
		Task<string> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string[] keys, object json);
		Task<string> PublishJsonAsync(string streamName, string key, object json);
		Task<string> PublishJsonAsync(string streamName, string[] keys, object json);
		Task SubscribeAsync(string streamName);
	}
}