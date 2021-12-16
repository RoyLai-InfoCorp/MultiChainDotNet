// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainPermissionsManager
	{
		Task<bool> CheckPermissionGrantedAsync(string address, string permission, string entityName = null);
		string GrantPermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null);
		string GrantPermission(string toAddress, string permissions, string entityName = null);
		Task<List<PermissionsResult>> ListPermissionsAsync(string address, string permissionType);
		Task<List<PermissionsResult>> ListPermissionsByAddressAsync(string address);
		Task<List<PermissionsResult>> ListPermissionsByTypeAsync(string permissionType);
		string RevokePermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null);
		string RevokePermission(string toAddress, string permissions, string entityName = null);
	}
}