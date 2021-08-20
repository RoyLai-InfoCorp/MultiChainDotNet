// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainPermissionsManager
	{
		Task<MultiChainResult<bool>> CheckPermissionGrantedAsync(string address, string permission, string entityName = null);
		MultiChainResult<string> GrantPermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null);
		MultiChainResult<string> GrantPermission(string toAddress, string permissions, string entityName = null);
		Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsAsync(string address, string permissionType);
		Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsByAddressAsync(string address);
		Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsByTypeAsync(string permissionType);
		MultiChainResult<string> RevokePermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null);
		MultiChainResult<string> RevokePermission(string toAddress, string permissions, string entityName = null);
	}
}
