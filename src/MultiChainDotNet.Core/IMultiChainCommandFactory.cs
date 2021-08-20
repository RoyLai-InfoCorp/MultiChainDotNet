// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainBlockchain;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainTransaction;

namespace MultiChainDotNet.Core
{
	public interface IMultiChainCommandFactory
	{
		T CreateCommand<T>() where T : MultiChainCommandBase;
	}
}
