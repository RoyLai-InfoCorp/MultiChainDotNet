// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;

namespace MultiChainDotNet.Core
{
	public interface IMultiChainCommandFactory
	{
		T CreateCommand<T>() where T : MultiChainCommandBase;
	}
}
