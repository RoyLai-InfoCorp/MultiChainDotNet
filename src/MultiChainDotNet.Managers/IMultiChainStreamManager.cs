// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainStreamManager
	{
		Task<MultiChainResult<bool>> IsExist(string streamName);
		MultiChainResult<string> CreateStream(SignerBase signer, string fromAddress, string streamName, bool anyoneCanWrite = false);
		MultiChainResult<string> CreateStream(string streamName, bool anyoneCanWrite = false);
		Task<MultiChainResult<StreamsResult>> GetStreamAsync(string streamName);
		Task<MultiChainResult<StreamItemsResult>> GetStreamItemAsync(string selectCmd);
		Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(bool verbose = false);
		Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(string streamName, bool verbose = false);
		Task<MultiChainResult<string>> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string key, object json);
		Task<MultiChainResult<string>> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string[] keys, object json);
		Task<MultiChainResult<string>> PublishJsonAsync(string streamName, string key, object json);
		Task<MultiChainResult<string>> PublishJsonAsync(string streamName, string[] keys, object json);
		Task<MultiChainResult<IList<StreamItemsResult>>> ListStreamItemsAsync(string selectCmd, bool verbose = false);
		Task<MultiChainResult<List<T>>> ListStreamItemsAsync<T>(string selectCmd);
		Task<MultiChainResult<VoidType>> SubscribeAsync(string streamName);
		Task<MultiChainResult<T>> GetStreamItemAsync<T>(string selectCmd);
		Task<MultiChainResult<List<T>>> ListAllStreamItemsAsync<T>(string streamName);
	}
}
