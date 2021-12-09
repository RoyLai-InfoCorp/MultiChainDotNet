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

		public async Task<MultiChainResult<VoidType>> ImportAddressAsync(string address)
		{
			return await _addressCmd.ImportAddressAsync(address);
		}

		public MultiChainResult<CreateMultiSigResult> CreateMultiSig(int nRequired, string[] pubkeys)
		{
			var result = Task.Run(async () =>
			{
				return await _addressCmd.CreateMultiSigAsync(nRequired, pubkeys);
			}).GetAwaiter().GetResult();
			if (result.IsError)
				return result;

			Task.Run(async () =>
			{
				await ImportAddressAsync(result.Result.Address);
			}).GetAwaiter().GetResult();

			return result;
		}

		public async Task<MultiChainResult<bool>> IsExistAsync(string address)
		{
			return await _addressCmd.CheckAddressImportedAsync(address);
		}

	}
}
