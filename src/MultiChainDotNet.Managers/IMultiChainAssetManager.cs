// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainAssetManager
	{
		Task<GetAddressBalancesResult> GetAssetBalanceByAddressAsync(string address, string assetName = null);
		Task<GetAssetInfoResult> GetAssetInfoAsync(string assetName);
		Task<bool> IsExist(string assetName);
		string Issue(SignerBase signer, string fromAddress, string toAddress, string assetName, ulong units, bool canIssueMore = true, object data = null);
		string Issue(string toAddress, string assetName, ulong units, bool canIssueMore = true, object data = null);
		string IssueAnnotate(SignerBase signer, string fromAddress, string toAddress, string assetName, ulong units, bool canIssueMore, object annotation);
		string IssueAnnotate(string toAddress, string assetName, ulong units, bool canIssueMore, object annotation);
		string IssueMore(SignerBase signer, string fromAddress, string toAddress, string assetName, ulong units, object data = null);
		string IssueMore(string toAddress, string assetName, ulong units, object data = null);
		string IssueMoreAnnotated(SignerBase signer, string fromAddress, string toAddress, string assetName, ulong units, object annotation);
		string IssueMoreAnnotated(string toAddress, string assetName, ulong units, object annotation);
		Task<List<GetAddressBalancesResult>> ListAssetBalancesByAddressAsync(string address);
		Task<List<ListAssetsResult>> ListAssetsAsync(string assetName = "*", bool verbose = false);
		Task<List<AssetTransactionsResult>> ListAssetTransactionsAsync(string assetName);
		string Pay(SignerBase signer, string fromAddress, string toAddress, ulong units, object data = null);
		string Pay(string toAddress, ulong units, object data = null);
		string PayAnnotate(SignerBase signer, string fromAddress, string toAddress, ulong units, object annotation);
		string PayAnnotate(string toAddress, ulong units, object annotation);
		Task<string> SendAnnotateAssetAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, ulong units, object annotation);
		Task<string> SendAnnotateAssetAsync(string toAddress, string assetName, ulong units, object annotation);
		string SendAsset(SignerBase signer, string fromAddress, string toAddress, string assetName, ulong units, object data = null);
		string SendAsset(string toAddress, string assetName, ulong units, object data = null);
		Task SubscribeAsync(string assetName);
	}
}