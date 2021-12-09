// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Core.MultiChainAddress
{
	public class MultiChainAddressCommand : MultiChainCommandBase
	{
		public MultiChainAddressCommand(ILogger<MultiChainAddressCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogTrace($"Initialized MultiChainAddressCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public MultiChainAddressCommand(ILogger<MultiChainAddressCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogTrace($"Initialized MultiChainAddressCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		private async Task<MultiChainResult<List<string>>> _GetAddressesAsync()
		{
			return await JsonRpcRequestAsync<List<string>>("getaddresses");
		}
		private async Task<MultiChainResult<List<AddressResult>>> _ListAddressesAsync(string address = null)
		{
			return address != null
				? await JsonRpcRequestAsync<List<AddressResult>>("listaddresses", address)
				: await JsonRpcRequestAsync<List<AddressResult>>("listaddresses");
		}


		public async Task<MultiChainResult<List<AddressResult>>> ListAddressesAsync(bool? privateKeyImported = null)
		{
			var result = await _ListAddressesAsync();
			if (result.IsError)
				return result;
			if (privateKeyImported is null)
				return result;

			return new MultiChainResult<List<AddressResult>>
			(
				result.Result.Where(x => x.IsMine == privateKeyImported).ToList()
			);
		}

		public async Task<MultiChainResult<bool>> CheckAddressImportedAsync(string address)
		{
			var result = await _ListAddressesAsync(address);
			if (result.IsError)
				return new MultiChainResult<bool>(result.Exception);

			return new MultiChainResult<bool>(result.Result.Count > 0);

		}

		// Either returns true or fail with error
		public async Task<MultiChainResult<VoidType>> ImportAddressAsync(string address)
		{
			return await JsonRpcRequestAsync<VoidType>("importaddress", address, "", false);
		}

		public string CreateAddressFromPublicKey(string pubkey)
		{
			return MultiChainAddressHelper.GetAddressFromPublicKey(pubkey,
				MultiChainConfiguration.AddressPubkeyhashVersion,
				MultiChainConfiguration.AddressChecksumValue);
		}

		public async Task<MultiChainResult<string>> GetNewAddressAsync()
		{
			// Always force the generation of compressed public key

			var data = MultiChainAddressHelper.GenerateNewAddress(this.MultiChainConfiguration.AddressPubkeyhashVersion, this.MultiChainConfiguration.PrivateKeyVersion, this.MultiChainConfiguration.AddressChecksumValue);
			var result = await JsonRpcRequestAsync<VoidType>("importprivkey", data.Wif);
			if (result.IsError)
				return new MultiChainResult<string>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, $"New address failed to import key. {result.Exception.ToString()}"));

			var addressesResult = await ListAddressesAsync();
			if (result.IsError)
				return new MultiChainResult<string>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, $"Unable to find created address. {result.Exception.ToString()}"));

			var found = addressesResult.Result.FirstOrDefault(x => x.Address == data.Address);
			if (found is null)
				return new MultiChainResult<string>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, "Unable to find created address."));

			return new MultiChainResult<string>(data.Address);
		}

		public async Task<MultiChainResult<CreateMultiSigResult>> CreateMultiSigAsync(int nRequired, string[] pubkeys)
		{
			return await JsonRpcRequestAsync<CreateMultiSigResult>("createmultisig", nRequired, pubkeys);
		}

	}
}
