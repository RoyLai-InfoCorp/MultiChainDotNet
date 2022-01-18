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
		Task<string> GetDeclarationAsync(string txid);
		Task<List<ListAddressTransactionResult>> ListAllTransactionsByAddress(string address, string assetName = null);
		Task<List<ListAssetTransactionResult>> ListAllTransactionsByAsset(string assetName);
		Task<List<ListAddressTransactionResult>> ListTransactionsByAddress(string address, int count, int skip, bool verbose);
		Task<List<ListAssetTransactionResult>> ListTransactionsByAsset(string assetName, bool verbose = false, int count = 10, int start = -10, bool localOrdering = false);
		(List<TxIdVoutStruct> SelectedUnspents, Dictionary<string, Double> ReturnUnspents) SelectUnspent(List<ListUnspentResult> unspents, UInt64 requiredPayment, string requiredAssetName, Double requiredAssetQty, UInt64 fees = 1000);
		Task<Dictionary<string, double>> ListUnspentBalances(string address);
	}
}