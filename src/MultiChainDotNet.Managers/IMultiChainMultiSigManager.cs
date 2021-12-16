// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainMultiSigManager
	{
		string CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, ulong qty);
		string CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, ulong qty, object data);
		string CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, ulong qty);
		string CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, ulong qty, object data);
		string SendMultiSigAsset(IList<string[]> signatures, string signatureSlip, string redeemScript);
		string SendMultiSigAssetAsync(IList<SignerBase> signers, string fromAddress, string toAddress, string assetName, ulong qty, string redeemScript);
		string[] SignMultiSig(SignerBase signer, string signatureSlip, string redeemScript);
		string[] SignMultiSig(string signatureSlip, string redeemScript);
	}
}