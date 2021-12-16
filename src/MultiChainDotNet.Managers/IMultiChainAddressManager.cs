// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.MultiChainAddress;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainAddressManager
	{
		CreateMultiSigResult CreateMultiSig(int nRequired, string[] pubkeys);
		Task ImportAddressAsync(string address);
		Task<bool> IsExistAsync(string address);
	}
}