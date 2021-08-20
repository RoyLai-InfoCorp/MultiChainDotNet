// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Fluent.Signers;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainManagerFactory
	{
		T CreateInstance<T>(SignerBase signer);
		T CreateInstance<T>();
	}
}
