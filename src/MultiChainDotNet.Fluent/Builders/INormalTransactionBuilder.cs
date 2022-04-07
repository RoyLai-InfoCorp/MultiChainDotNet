// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Fluent.Signers;

namespace MultiChainDotNet.Fluent.Builders
{
	public interface INormalTransactionBuilder : ITransactionBuilder
	{
		#region Normal Transaction
		public INormalTransactionBuilder AddRawNormalTransaction(string raw);
		INormalTransactionBuilder AddSigner(SignerBase signer);
		INormalTransactionBuilder Sign();
		string RawSigned();
		string Raw();

		string Send();

		#endregion

	}
}
