using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainAddressManager : IMultiChainAddressManager
	{
		MultiChainAddressCommand _addressCmd;

		public MultiChainAddressManager(IMultiChainCommandFactory commandFactory)
		{
			_addressCmd = commandFactory.CreateCommand<MultiChainAddressCommand>();
		}

		public async Task<MultiChainResult<VoidType>> ImportAddressAsync(string address)
		{
			return await _addressCmd.ImportAddressAsync(address);
		}

		public async Task<MultiChainResult<CreateMultiSigResult>> CreateMultiSigAsync(int nRequired, string[] pubkeys)
		{
			var result = await _addressCmd.CreateMultiSigAsync(nRequired, pubkeys);
			if (result.IsError)
				return result;
			await ImportAddressAsync(result.Result.Address);
			return result;
		}

		public async Task<MultiChainResult<bool>> IsExistAsync(string address)
		{
			return await _addressCmd.CheckAddressImportedAsync(address);
		}

	}
}
