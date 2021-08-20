// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Builders;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		public MultiChainResult<string> GrantPermission(string toAddress, string permissions, string entityName = null)
		{
			return GrantPermission(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, permissions, entityName);
		}

		public MultiChainResult<string> GrantPermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null)
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

				return new MultiChainResult<string>(txid);
			}
			catch(Exception ex)
			{
				return new MultiChainResult<string>(ex);
			}
		}

		public MultiChainResult<string> RevokePermission(string toAddress, string permissions, string entityName = null)
		{
			return RevokePermission(_defaultSigner, _mcConfig.Node.NodeWallet, toAddress, permissions, entityName);
		}

		public MultiChainResult<string> RevokePermission(SignerBase signer, string fromAddress, string toAddress, string permissions, string entityName = null)
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

				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				return new MultiChainResult<string>(ex);
			}
		}

		public async Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsAsync(string address, string permissionType)
		{
			return await _permCmd.ListPermissionsAsync(address, permissionType);
		}

		public async Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsByAddressAsync(string address)
		{
			return await _permCmd.ListPermissionsByAddressAsync(address);
		}

		public async Task<MultiChainResult<List<PermissionsResult>>> ListPermissionsByTypeAsync(string permissionType)
		{
			return await _permCmd.ListPermissionsByTypeAsync(permissionType);
		}

		public async Task<MultiChainResult<bool>> CheckPermissionGrantedAsync(string address, string permission, string entityName = null)
		{
			var permissions = permission.Split(',');
			foreach (var perm in permissions)
			{
				var result = await _permCmd.CheckPermissionGrantedAsync(address, perm, entityName);
				if (result.IsError)
					return result;
				if (!result.Result)
					return new MultiChainResult<bool>(false);
			}
			return new MultiChainResult<bool>(true);
		}
	}
}
