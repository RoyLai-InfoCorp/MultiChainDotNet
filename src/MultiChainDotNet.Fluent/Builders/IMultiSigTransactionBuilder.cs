// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Base;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;

namespace MultiChainDotNet.Fluent.Builders
{
	public interface IMultiSigTransactionBuilder
	{
		#region MultiSig Transaction
		IMultiSigTransactionBuilder AddMultiSigSigner(SignerBase signer);
		IMultiSigTransactionBuilder AddMultiSigSigners(IList<SignerBase> signers);
		IMultiSigTransactionBuilder AddMultiSignatures(IList<string[]> signatures);
		IMultiSigTransactionBuilder AddRawTransaction(string raw);
		IMultiSigTransactionBuilder MultiSign(string redeemScript);
		string[] MultiSignPartial(string raw, string redeemScript, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL);
		string RawSigned();
		string Send();

		#endregion

	}
}
