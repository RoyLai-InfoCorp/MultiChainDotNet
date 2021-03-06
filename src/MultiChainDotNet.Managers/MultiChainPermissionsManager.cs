// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainPermissionsManager : IMultiChainPermissionsManager
	{
		private IServiceProvider _container;
		private ILogger<MultiChainPermissionsManager> _logger;
		private MultiChainConfiguration _mcConfig;
		private SignerBase _defaultSigner;

		public MultiChainPermissionsManager(IServiceProvider container)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainPermissionsManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainPermissionsManager(IServiceProvider container, SignerBase signer)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainPermissionsManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = signer;
		}


		public string GrantPermission(string toAddress, string permissions, string entityName = null)
		{
			return GrantPermission(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, permissions, entityName);
		}

		public string GrantPermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null)
		{
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var txid = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
							.Permit(permissions, entityName)
						.CreateNormalTransaction(txnCmd)
							.AddSigner(signer)
							.Sign()
							.Send()
						;

					return txid;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex.ToString());
					throw;
				}

			}

		}

		public string RevokePermission(string toAddress, string permissions, string entityName = null)
		{
			return RevokePermission(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, permissions, entityName);
		}

		public string RevokePermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null)
		{
			using (var scope = _container.CreateScope())
			{
				var txnCmd = scope.ServiceProvider.GetRequiredService<MultiChainTransactionCommand>();
				try
				{
					var txid = new MultiChainFluent()
						.AddLogger(_logger)
						.From(fromAddress)
						.To(toAddress)
							.Revoke(permissions, entityName)
						.CreateNormalTransaction(txnCmd)
							.AddSigner(signer)
							.Sign()
							.Send()
						;

					return txid;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex.ToString());
					throw;
				}
			}

		}

		public async Task<List<PermissionsResult>> ListPermissionsAsync(string address, string permissionType)
		{
			using (var scope = _container.CreateScope())
			{
				var permCmd = scope.ServiceProvider.GetRequiredService<MultiChainPermissionCommand>();
				var result = await permCmd.ListPermissionsAsync(address, permissionType);
				if (result.IsError)
				{
					_logger.LogWarning(result.Exception.ToString());
					throw result.Exception;
				}
				return result.Result;
			}

		}

		public async Task<List<PermissionsResult>> ListPermissionsByAddressAsync(string address)
		{
			using (var scope = _container.CreateScope())
			{
				var permCmd = scope.ServiceProvider.GetRequiredService<MultiChainPermissionCommand>();
				var result = await permCmd.ListPermissionsByAddressAsync(address);
				if (result.IsError)
				{
					_logger.LogWarning(result.Exception.ToString());
					throw result.Exception;
				}
				return result.Result;
			}

		}

		public async Task<List<PermissionsResult>> ListPermissionsByTypeAsync(string permissionType)
		{
			using (var scope = _container.CreateScope())
			{
				var permCmd = scope.ServiceProvider.GetRequiredService<MultiChainPermissionCommand>();
				var result = await permCmd.ListPermissionsByTypeAsync(permissionType);
				if (result.IsError)
				{
					_logger.LogWarning(result.Exception.ToString());
					throw result.Exception;
				}
				return result.Result;
			}

		}

		public async Task<bool> CheckPermissionGrantedAsync(string address, string permission, string entityName = null)
		{
			using (var scope = _container.CreateScope())
			{
				var permCmd = scope.ServiceProvider.GetRequiredService<MultiChainPermissionCommand>();
				var permissions = permission.Split(',');
				foreach (var perm in permissions)
				{
					var result = await permCmd.CheckPermissionGrantedAsync(address, perm, entityName);
					if (result.IsError)
						throw result.Exception;

					if (!result.Result)
						return false;
				}
				return true;
			}

		}

		public async Task<bool> CheckPendingGrantPermissionAsync(string address, string permission, string entityName = null)
		{
			using (var scope = _container.CreateScope())
			{
				var permCmd = scope.ServiceProvider.GetRequiredService<MultiChainPermissionCommand>();
				var permissions = permission.Split(',');
				foreach (var perm in permissions)
				{
					var result = await permCmd.CheckPendingGrantPermissionAsync(address, perm, entityName);
					if (result.IsError)
						throw result.Exception;

					if (!result.Result)
						return false;
				}
				return true;
			}

		}

	}
}
