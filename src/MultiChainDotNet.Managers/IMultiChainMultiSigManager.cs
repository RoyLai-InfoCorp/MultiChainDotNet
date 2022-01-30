// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainMultiSigManager
	{
		string CreateIssueAssetSignatureSlip(string fromAddress, string toAddress, string assetName, ulong qty);
		string CreateIssueAssetSignatureSlip(string fromAddress, string toAddress, string assetName, ulong qty, object data);
		string CreateSendAssetSignatureSlip(string fromAddress, string toAddress, string assetName, ulong qty);
		string CreateSendAssetSignatureSlip(string fromAddress, string toAddress, string assetName, ulong qty, object data);
		string SendMultiSigAsset(IList<string[]> signatures, string signatureSlip, string redeemScript);
		string SendMultiSigAsset(IList<SignerBase> signers, string fromAddress, string toAddress, string assetName, ulong qty, string redeemScript);
		string[] SignMultiSig(SignerBase signer, string signatureSlip, string redeemScript);
		string[] SignMultiSig(string signatureSlip, string redeemScript);
		string CreateSignedMultiSigAssetTransactionAsync(IList<string[]> signatures, string signatureSlip, string redeemScript);
	}
}