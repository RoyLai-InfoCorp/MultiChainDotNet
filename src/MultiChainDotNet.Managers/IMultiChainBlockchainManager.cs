// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.MultiChainBlockchain;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainBlockchainManager
	{
		Task<GetBlockResult> GetCurrentBlock();
	}
}