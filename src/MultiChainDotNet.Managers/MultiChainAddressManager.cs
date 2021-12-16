// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Fluent.Signers;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Managers
{
	public class MultiChainAddressManager : IMultiChainAddressManager
	{
		MultiChainAddressCommand _addressCmd;

		public MultiChainAddressManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory commandFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_addressCmd = commandFactory.CreateCommand<MultiChainAddressCommand>();
		}

		public MultiChainAddressManager(IMultiChainCommandFactory commandFactory)
		{
			_addressCmd = commandFactory.CreateCommand<MultiChainAddressCommand>();
		}

		public async Task ImportAddressAsync(string address)
		{
			var result = await _addressCmd.ImportAddressAsync(address);
			if (result.IsError)
				throw result.Exception;
		}

		public CreateMultiSigResult CreateMultiSig(int nRequired, string[] pubkeys)
		{
			var result = Task.Run(async () =>
			{
				var addr = await _addressCmd.CreateMultiSigAsync(nRequired, pubkeys);
				await ImportAddressAsync(addr.Result.Address);
				return addr;
			}).GetAwaiter().GetResult();
			if (result.IsError)
				throw result.Exception;
			return result.Result;
		}

		public async Task<bool> IsExistAsync(string address)
		{
			var result = await _addressCmd.CheckAddressImportedAsync(address);
			if (result.IsError)
				throw result.Exception;
			return result.Result;
		}

	}
}
