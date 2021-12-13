// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainTransactionManager
	{
		Task<MultiChainResult<List<ListAddressTransactionResult>>> ListTransactionsByAddress(string address, int count = 10, int skip = 0, bool verbose = false);

		Task<MultiChainResult<List<ListAssetTransactionResult>>> ListTransactionsByAsset(string assetName, bool verbose = false, int count = 10, int start = -10, bool localOrdering = false);

		Task<MultiChainResult<string>> GetAnnotationAsync(string assetName, string txid);

		Task<MultiChainResult<string>> GetDeclarationAsync(string txid);

		Task<MultiChainResult<DecodeRawTransactionResult>> DecodeRawTransactionAsync(string txid);

		Task<MultiChainResult<List<ListAssetTransactionResult>>> ListAllTransactionsByAsset(string assetName);
		Task<MultiChainResult<List<ListAddressTransactionResult>>> ListAllTransactionsByAddress(string address, string assetName = null);
	}
}
