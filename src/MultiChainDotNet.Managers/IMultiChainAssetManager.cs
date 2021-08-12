using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainAssetManager
	{
		Task<MultiChainResult<string>> PayAsync(SignerBase signer, string fromAddress, string toAddress, UInt64 amt, object data = null);
		Task<MultiChainResult<string>> PayAnnotateAsync(SignerBase signer, string fromAddress, string toAddress, UInt64 amt, object data = null);
		Task<MultiChainResult<string>> PayAsync(string toAddress, UInt64 amt, object data = null);
		Task<MultiChainResult<string>> SendAssetAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null);
		Task<MultiChainResult<string>> SendAnnotateAssetAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null);
		Task<MultiChainResult<string>> SendAssetAsync(string toAddress, string assetName, UInt64 amt, object data = null);

		Task<MultiChainResult<string>> IssueAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, bool canIssueMore = true, object data = null);
		Task<MultiChainResult<string>> IssueAnnotateAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, bool canIssueMore = true, object data = null);
		Task<MultiChainResult<string>> IssueAsync(string toAddress, string assetName, UInt64 amt, bool canIssueMore,object data = null);
		Task<MultiChainResult<string>> IssueMoreAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null);
		Task<MultiChainResult<string>> IssueMoreAsync(string toAddress, string assetName, UInt64 amt, object data = null);
		Task<MultiChainResult<string>> IssueMoreAnnotatedAsync(SignerBase signer, string fromAddress, string toAddress, string assetName, UInt64 amt, object data = null);

		MultiChainResult<string> CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, double qty);
		MultiChainResult<string> CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, UInt64 qty, object data = null);
		Task<MultiChainResult<GetAddressBalancesResult>> GetAssetBalanceByAddressAsync(string address, string assetName = null);
		Task<MultiChainResult<List<GetAddressBalancesResult>>> ListAssetBalancesByAddressAsync(string address);
		Task<MultiChainResult<GetAssetInfoResult>> GetAssetInfoAsync(string assetName);
		Task<MultiChainResult<List<ListAssetsResult>>> ListAssetsAsync(string assetName = "*", bool verbose = false);
		Task<MultiChainResult<List<AssetTransactionsResult>>> ListAssetTransactionsAsync(string assetName);
		MultiChainResult<string> SendMultiSigAssetAsync(IList<SignerBase> signers, string fromAddress, string toAddress, string assetName, double qty, string redeemScript);
		MultiChainResult<string> SendMultiSigAssetAsync(IList<string[]> signatures, string signatureSlip, string redeemScript);
		MultiChainResult<string[]> SignMultiSig(SignerBase signer, string signatureSlip, string redeemScript);
		Task<MultiChainResult<VoidType>> SubscribeAsync(string assetName);
	}
}