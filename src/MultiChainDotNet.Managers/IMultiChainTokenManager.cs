﻿using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainToken;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainTokenManager
	{
		Task<GetAssetInfoResult> GetNonfungibleAssetInfo(string assetName);
		Task<bool> IsExist(string assetName);
		Task<string> IssueNonfungibleAsset(SignerBase signer, string fromAddress, string toAddress, string nfaName, object data = null);
		Task<string> IssueNonfungibleAsset(string toAddress, string nfaName, object data = null);
		string IssueToken(string toAddress, string nfaName, string tokenId, int qty, object annotation = null);
		string IssueTokenAnnotate(SignerBase signer, string fromAddress, string toAddress, string nfaName, string tokenId, int qty, object annotation = null);
		Task<IList<GetTokenBalancesResult.GetTokenBalanceItem>> ListTokensAsync(string address, string nfaName);
		Task<string> SendAnnotateTokenAsync(SignerBase signer, string fromAddress, string toAddress, string nfaName, string tokenId, int qty = 1, object annotation = null);
		Task<string> SendAnnotateTokenAsync(string toAddress, string nfaName, string tokenId, int qty = 1, object annotation = null);
		string SendToken(SignerBase signer, string fromAddress, string toAddress, string nfaName, string tokenId, int qty = 1, object data = null);
		string SendToken(string toAddress, string nfaName, string token, int qty = 1, object data = null);
		Task SubscribeAsync(string assetName);
	}
}