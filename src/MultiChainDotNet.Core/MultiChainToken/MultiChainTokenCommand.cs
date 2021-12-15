using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAsset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainToken
{
    public class MultiChainTokenCommand : MultiChainCommandBase
    {
        public MultiChainTokenCommand(ILogger<MultiChainTokenCommand> logger, MultiChainConfiguration mcConfig): base(logger, mcConfig)
		{
			_logger.LogDebug($"Initialized MultiChainTokenCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}
		public MultiChainTokenCommand(ILogger<MultiChainTokenCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogDebug($"Initialized MultiChainTokenCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		public async Task<MultiChainResult<string>> IssueNonFungibleAssetAsync(string address, string nfaName, double amt=0)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));

			return await JsonRpcRequestAsync<string>("issue", address, new { name = nfaName, fungible = false, open = true }, 0, 1, amt);
		}

		public async Task<MultiChainResult<GetNonfungibleAssetInfoResult>> GetNonfungibleAssetInfo(string nfaName)
		{
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));

			return await JsonRpcRequestAsync<GetNonfungibleAssetInfoResult>("getassetinfo", nfaName);
		}

		public async Task<MultiChainResult<List<GetAddressBalancesResult>>> GetAddressBalancesAsync(string address)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			return await JsonRpcRequestAsync<List<GetAddressBalancesResult>>("getaddressbalances", address);
		}


		public async Task<MultiChainResult<string>> IssueNonFungibleAssetFromAsync(string from, string to, string nfaName, double amt = 0)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));

			return await JsonRpcRequestAsync<string>("issuefrom", from, to, new { name = nfaName, fungible = false, open = true }, 0, 1, amt);
		}


		public async Task<MultiChainResult<string>> IssueTokenAsync(string address, string nfaName, string tokenId, int qty = 1)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));
			if (tokenId is null)
				throw new ArgumentNullException(nameof(tokenId));

			return await JsonRpcRequestAsync<string>("issuetoken", address, nfaName, tokenId, qty);
		}

		public async Task<MultiChainResult<string>> IssueTokenFromAsync(string from, string to, string nfaName, string tokenId, int qty = 1)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));
			if (tokenId is null)
				throw new ArgumentNullException(nameof(tokenId));

			return await JsonRpcRequestAsync<string>("issuetokenfrom", from, to, nfaName, tokenId, qty);
		}


		public async Task<MultiChainResult<GetTokenBalancesResult>> GetTokenBalancesAsync(string address)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			return await JsonRpcRequestAsync<GetTokenBalancesResult>("gettokenbalances", address);
		}

		public async Task<MultiChainResult<string>> SendTokenAsync(string to, string nfaName, string tokenId, int qty = 1)
		{
			if (to is null)
				throw new ArgumentNullException(nameof(to));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));


			return await JsonRpcRequestAsync<string>("send", to, new Dictionary<string, object> { { nfaName, new {
				token = tokenId,
				qty = qty
			} } } );
		}

		public async Task<MultiChainResult<string>> SendTokenFromAsync(string from, string to, string nfaName, string tokenId, int qty = 1)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));


			return await JsonRpcRequestAsync<string>("sendfrom", from, to, new Dictionary<string, object> { { nfaName, new {
				token = tokenId,
				qty = qty
			} } });
		}


	}
}
