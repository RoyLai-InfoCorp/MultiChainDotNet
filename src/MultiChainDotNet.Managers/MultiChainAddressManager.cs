// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Managers
{
	public class MultiChainAddressManager : IMultiChainAddressManager
	{
		private IServiceProvider _container;
		private ILogger<MultiChainAddressManager> _logger;
		private MultiChainConfiguration _mcConfig;
		private SignerBase _defaultSigner;

		public MultiChainAddressManager(IServiceProvider container)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainAddressManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainAddressManager(IServiceProvider container, SignerBase signer)
		{
			_container = container;
			_logger = container.GetRequiredService<ILogger<MultiChainAddressManager>>();
			_mcConfig = container.GetRequiredService<MultiChainConfiguration>();
			_defaultSigner = signer;
		}

		public async Task ImportAddressAsync(string address)
		{
			using (var scope = _container.CreateScope())
			{
				var addressCmd = scope.ServiceProvider.GetRequiredService<MultiChainAddressCommand>();
				var result = await addressCmd.ImportAddressAsync(address);
				if (result.IsError)
					throw result.Exception;
			}

		}

		//public CreateMultiSigResult CreateMultiSig(int nRequired, string[] pubkeys)
		//{
		//	using (var scope = _container.CreateScope())
		//	{
		//		var addressCmd = scope.ServiceProvider.GetRequiredService<MultiChainAddressCommand>();
		//		var result = Task.Run(async () =>
		//		{
		//			var addr = await addressCmd.CreateMultiSigAsync(nRequired, pubkeys);
		//			await ImportAddressAsync(addr.Result.Address);
		//			return addr;
		//		}).GetAwaiter().GetResult();
		//		if (result.IsError)
		//			throw result.Exception;
		//		return result.Result;
		//	}

		//}

		public async Task<CreateMultiSigResult> CreateMultiSigAsync(int nRequired, string[] pubkeys)
		{
			using (var scope = _container.CreateScope())
			{
				var addressCmd = scope.ServiceProvider.GetRequiredService<MultiChainAddressCommand>();
				var result = await addressCmd.CreateMultiSigAsync(nRequired, pubkeys);
				if (result.IsError)
					throw result.Exception;
				await ImportAddressAsync(result.Result.Address);
				return result.Result;
			}

		}

		public async Task<bool> IsExistAsync(string address)
		{
			using (var scope = _container.CreateScope())
			{
				var addressCmd = scope.ServiceProvider.GetRequiredService<MultiChainAddressCommand>();
				var result = await addressCmd.CheckAddressImportedAsync(address);
				if (result.IsError)
					throw result.Exception;
				return result.Result;
			}
		}

	}
}
