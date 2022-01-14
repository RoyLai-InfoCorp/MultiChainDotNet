// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

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
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;
		private IMultiChainCommandFactory _cmdFactory;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;
		private MultiChainPermissionCommand _permCmd;
		MultiChainTransactionCommand _txnCmd;

		public MultiChainPermissionsManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory commandFactory,
			MultiChainConfiguration mcConfig)
		{
			_loggerFactory = loggerFactory;
			_cmdFactory = commandFactory;
			_mcConfig = mcConfig;
			_logger = loggerFactory.CreateLogger<MultiChainPermissionsManager>();
			_permCmd = _cmdFactory.CreateCommand<MultiChainPermissionCommand>();
			_txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainPermissionsManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory commandFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_loggerFactory = loggerFactory;
			_cmdFactory = commandFactory;
			_mcConfig = mcConfig;
			_logger = loggerFactory.CreateLogger<MultiChainStreamManager>();
			_permCmd = _cmdFactory.CreateCommand<MultiChainPermissionCommand>();
			_txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_defaultSigner = signer;
		}

		public string GrantPermission(string toAddress, string permissions, string entityName = null)
		{
			return GrantPermission(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, permissions, entityName);
		}

		public string GrantPermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null)
		{
			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.Permit(permissions, entityName)
					.CreateNormalTransaction(_txnCmd)
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

		public string RevokePermission(string toAddress, string permissions, string entityName = null)
		{
			return RevokePermission(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, permissions, entityName);
		}

		public string RevokePermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null)
		{
			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.To(toAddress)
						.Revoke(permissions, entityName)
					.CreateNormalTransaction(_txnCmd)
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

		public async Task<List<PermissionsResult>> ListPermissionsAsync(string address, string permissionType)
		{
			var result = await _permCmd.ListPermissionsAsync(address, permissionType);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}

		public async Task<List<PermissionsResult>> ListPermissionsByAddressAsync(string address)
		{
			var result = await _permCmd.ListPermissionsByAddressAsync(address);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}

		public async Task<List<PermissionsResult>> ListPermissionsByTypeAsync(string permissionType)
		{
			var result = await _permCmd.ListPermissionsByTypeAsync(permissionType);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}

		public async Task<bool> CheckPermissionGrantedAsync(string address, string permission, string entityName = null)
		{
			var permissions = permission.Split(',');
			foreach (var perm in permissions)
			{
				var result = await _permCmd.CheckPermissionGrantedAsync(address, perm, entityName);
				if (result.IsError)
					throw result.Exception;

				if (!result.Result)
					return false;
			}
			return true;
		}

		public async Task<bool> CheckPendingGrantPermissionAsync(string address, string permission, string entityName = null)
		{
			var permissions = permission.Split(',');
			foreach (var perm in permissions)
			{
				var result = await _permCmd.CheckPendingGrantPermissionAsync(address, perm, entityName);
				if (result.IsError)
					throw result.Exception;

				if (!result.Result)
					return false;
			}
			return true;
		}

	}
}
