// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainPermission
{
	public class MultiChainPermissionCommand : MultiChainCommandBase
	{

		public MultiChainPermissionCommand(ILogger<MultiChainPermissionCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogTrace($"Initialized MultiChainPermissionCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public MultiChainPermissionCommand(ILogger<MultiChainPermissionCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogTrace($"Initialized MultiChainPermissionCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}
		public Task<bool> WaitUntilPermissionGranted(string subject, string permission, int retries=5, int delay=500)
		{
			return TaskHelper.WaitUntilTrueAsync(async () =>
			  (await CheckPermissionGrantedAsync(subject, permission)).Result == true
		  , retries, delay);
		}

		public Task<bool> WaitUntilPermissionRevoked(string subject, string permission, int retries = 5, int delay = 500)
		{
			return TaskHelper.WaitUntilTrueAsync(async () =>
			  (await CheckPermissionGrantedAsync(subject, permission)).Result == false
		  , retries, delay);
		}

		public async Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsAsync(string address, string permissionType,bool verbose=true)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));


			return await JsonRpcRequestAsync<List<PermissionsResult>>("listpermissions", permissionType.ToLower(), address, verbose);
		}

		public async Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsByAddressAsync(string address, bool verbose = true)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			return await JsonRpcRequestAsync<List<PermissionsResult>>("listpermissions", "*", address, verbose);
		}

		public async Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsByTypeAsync(string permissionType, bool verbose = true)
		{
			if (permissionType is null)
				throw new ArgumentNullException(nameof(permissionType));

			return await JsonRpcRequestAsync<List<PermissionsResult>>("listpermissions", permissionType.ToLower(), "*", verbose);
		}

		public async Task<MultiChainResult<bool>> CheckPermissionGrantedAsync(string address, string permission, string entityName = null)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));


			var permissionString = entityName is { } ? $"{entityName}.{permission.ToLower()}" : permission.ToLower();
			var result = await JsonRpcRequestAsync<List<PermissionsResult>>("listpermissions", permissionString, address, false);
			if (result.IsError)
				return new MultiChainResult<bool>(result.Exception);
			var granted = result.Result.Count > 0;
			return new MultiChainResult<bool>(granted);
		}

		public async Task<MultiChainResult<bool>> CheckPendingGrantPermissionAsync(string address, string permission, string entityName = null)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));


			var permissionString = entityName is { } ? $"{entityName}.{permission.ToLower()}" : permission.ToLower();
			var result = await JsonRpcRequestAsync<List<PermissionsResult>>("listpermissions", permissionString, address, true);
			if (result.IsError)
				return new MultiChainResult<bool>(result.Exception);

			var pending = result.Result.Count == 1 && result.Result[0].Pending is { } && result.Result[0].Pending.Count > 0;
			return new MultiChainResult<bool>(pending);
		}

		public async Task<MultiChainResult<string>> GrantPermissionAsync(string address, string permissions, string entityName = null)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));


			if (entityName is { })
				permissions = CreateEntityPermissionType(permissions, entityName);
			return await JsonRpcRequestAsync<string>("grant", address, permissions);
		}

		private string CreateEntityPermissionType(string permissions, string entityName)
		{
			if (permissions is null)
				throw new ArgumentNullException(nameof(permissions));


			var sb = new StringBuilder();
			var perms = permissions.Split(",");
			sb.Append($"{entityName}.{perms[0]}");
			for (int i = 1; i < perms.Length; i++)
			{
				sb.Append($",{entityName}.{perms[i]}");
			}
			return sb.ToString();
		}

		public async Task<MultiChainResult<string>> GrantPermissionFromAsync(string from, string to, string permissions, string entityName = null)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));

			if (entityName is { })
				permissions = CreateEntityPermissionType(permissions, entityName);
			return await JsonRpcRequestAsync<string>("grantfrom", from, to, permissions);
		}


		public async Task<MultiChainResult<string>> RevokePermissionAsync(string address, string permissions, string entityName = null)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));


			if (entityName is { })
				permissions = CreateEntityPermissionType(permissions, entityName);
			return await JsonRpcRequestAsync<string>("revoke", address, permissions);
		}

		public async Task<MultiChainResult<string>> RevokePermissionFromAsync(string from, string to, string permissions, string entityName = null)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));


			if (entityName is { })
				permissions = CreateEntityPermissionType(permissions, entityName);
			return await JsonRpcRequestAsync<string>("revokefrom", from, to, permissions);
		}


	}
}
