// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.MultiChainTransaction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainTransactionManager
	{
		Task<DecodeRawTransactionResult> DecodeRawTransactionAsync(string txid);
		Task<string> GetAnnotationAsync(string assetName, string txid);
		Task<string> GetAttachmentAsync(string txid);
		Task<List<ListAddressTransactionResult>> ListAllTransactionsByAddressAsync(string address, string assetName = null);
		Task<List<ListAssetTransactionResult>> ListAllTransactionsByAssetAsync(string assetName);
		Task<List<ListAddressTransactionResult>> ListTransactionsByAddressAsync(string address, int count, int skip, bool verbose);
		Task<List<ListAssetTransactionResult>> ListTransactionsByAssetAsync(string assetName, bool verbose = false, int count = 10, int start = -10, bool localOrdering = false);
		(List<TxIdVoutStruct> SelectedUnspents, Dictionary<string, Double> ReturnUnspents) SelectUnspent(List<ListUnspentResult> unspents, UInt64 requiredPayment, string requiredAssetName, Double requiredAssetQty, UInt64 fees = 1000);
		Task<Dictionary<string, double>> ListUnspentBalancesAsync(string address);
	}
}