// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainMultiSigManager
	{
		MultiChainResult<string> CreateIssueAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, ulong qty, object data = null);
		MultiChainResult<string> CreateSendAssetSignatureSlipAsync(string fromAddress, string toAddress, string assetName, double qty);
		MultiChainResult<string> SendMultiSigAsset(IList<string[]> signatures, string signatureSlip, string redeemScript);
		MultiChainResult<string> SendMultiSigAssetAsync(IList<SignerBase> signers, string fromAddress, string toAddress, string assetName, double qty, string redeemScript);
		MultiChainResult<string[]> SignMultiSig(SignerBase signer, string signatureSlip, string redeemScript);

	}
}
