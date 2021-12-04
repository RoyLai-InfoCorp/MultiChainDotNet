// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainAssetManager
	{
		Task<bool> IsExist(string assetName);
		MultiChainResult<string> Pay(SignerBase signer, string fromAddress, string toAddress, UInt64 amt, object data = null);
		MultiChainResult<string> Pay(string toAddress, UInt64 amt, object data = null);

		MultiChainResult<string> PayAnnotate(string toAddress, UInt64 amt, object annotation);
		MultiChainResult<string> PayAnnotate(SignerBase signer, string fromAddress, string toAddress, UInt64 amt, object annotation);

		MultiChainResult<string> SendAsset(string toAddress, string assetName, UInt64 amt, object data = null);
		MultiChainResult<string> SendAsset(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null);

		Task<MultiChainResult<string>> SendAnnotateAssetAsync(string toAddress, string assetName, UInt64 amt, object annotation);
		Task<MultiChainResult<string>> SendAnnotateAssetAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object annotation);

		MultiChainResult<string> Issue(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, bool canIssueMore = true, object data = null);
		MultiChainResult<string> Issue(string toAddress, string assetName, UInt64 amt, bool canIssueMore,object data = null);

		MultiChainResult<string> IssueAnnotate(string toAddress, string assetName, UInt64 amt, bool canIssueMore, object annotation);
		MultiChainResult<string> IssueAnnotate(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, bool canIssueMore, object annotation);

		MultiChainResult<string> IssueMore(string toAddress, string assetName, UInt64 amt, object data = null);
		MultiChainResult<string> IssueMore(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null);

		MultiChainResult<string> IssueMoreAnnotated(string toAddress, string assetName, UInt64 amt, object annotation);
		MultiChainResult<string> IssueMoreAnnotated(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object annotation);


		Task<MultiChainResult<GetAddressBalancesResult>> GetAssetBalanceByAddressAsync(string address, string assetName = null);
		Task<MultiChainResult<List<GetAddressBalancesResult>>> ListAssetBalancesByAddressAsync(string address);
		Task<MultiChainResult<GetAssetInfoResult>> GetAssetInfoAsync(string assetName);
		Task<MultiChainResult<List<ListAssetsResult>>> ListAssetsAsync(string assetName = "*", bool verbose = false);
		Task<MultiChainResult<List<AssetTransactionsResult>>> ListAssetTransactionsAsync(string assetName);
		

		Task<MultiChainResult<VoidType>> SubscribeAsync(string assetName);
	}
}
