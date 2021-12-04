// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainAddressManager
	{
		MultiChainResult<CreateMultiSigResult> CreateMultiSig(int nRequired, string[] pubkeys);
		Task<MultiChainResult<VoidType>> ImportAddressAsync(string address);
		Task<MultiChainResult<bool>> IsExistAsync(string address);
	}
}
