// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

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
		IMultiSigTransactionBuilder AddRawMultiSignatureTransaction(string raw);
		IMultiSigTransactionBuilder MultiSign(string redeemScript);
		string[] MultiSignPartial(string raw, string redeemScript, BitCoinConstants.HashTypeEnum hashType = BitCoinConstants.HashTypeEnum.SIGHASH_ALL);
		string RawSigned();
		string Send();

		#endregion

	}
}
