using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static MultiChainDotNet.Core.MultiChainToken.GetTokenBalancesResult;

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

		public async Task<MultiChainResult<GetNonfungibleAssetInfoResult>> GetNfaInfo(string nfaName)
		{
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));

			return await JsonRpcRequestAsync<GetNonfungibleAssetInfoResult>("getassetinfo", nfaName);
		}

		public async Task<MultiChainResult<IList<ListAssetsResult>>> ListNfa( string address = null)
		{
			// List all available assets
			var assetsResult = await JsonRpcRequestAsync<IList<ListAssetsResult>>("listassets");
			if (assetsResult.IsError) return new MultiChainResult<IList<ListAssetsResult>>(assetsResult.Exception);

			// Return all NFAs
			var nfas = assetsResult.Result.Where(x => x.Fungible == false).ToList();
			if (address is null)
				return new MultiChainResult<IList<ListAssetsResult>>(nfas);

			// Return all NFAs owned by wallet

			// List all assets owned by wallet
			var addressBalancesResult = await JsonRpcRequestAsync<List<GetAddressBalancesResult>>("getaddressbalances", address);
			if (addressBalancesResult.IsError) return new MultiChainResult<IList<ListAssetsResult>>(addressBalancesResult.Exception);
			if (addressBalancesResult.Result.Count==0)
				new MultiChainResult<IList<ListAssetsResult>>(new List<ListAssetsResult>());

			// If address balances is not empty or error
			var ownNfas = nfas.Where(x => addressBalancesResult.Result.Any(y=>y.Name == x.Name)).ToList();
			return new MultiChainResult<IList<ListAssetsResult>>(ownNfas);
		}

		public async Task<MultiChainResult<IList<GetTokenBalanceItem>>> ListNft(string address, string nfaName=null, string tokenId=null)
		{
			// Should not continue if tokenId is provided without nfaName
			if (nfaName is null && tokenId is { })
				throw new MultiChainException(MultiChainErrorCode.NFA_NAME_IS_MISSING);

			// Get the total token balances
			var result = await JsonRpcRequestAsync<GetTokenBalancesResult>("gettokenbalances", address);
			if (result.IsError || result.Result is null)
				return new MultiChainResult<IList<GetTokenBalanceItem>>(result.Exception);
			if (result.Result.Count == 0 || result.Result[address].Count == 0)
				return new MultiChainResult<IList<GetTokenBalanceItem>>(new List<GetTokenBalanceItem>());

			// Get the NFTs from total token balances
			IList<GetTokenBalanceItem> list = null;
			if (nfaName is null)
				return new MultiChainResult<IList<GetTokenBalanceItem>>(result.Result[address].ToList());
			if (tokenId is null)
				return new MultiChainResult<IList<GetTokenBalanceItem>>(result.Result[address].Where(x=>x.NfaName == nfaName).ToList());
			return new MultiChainResult<IList<GetTokenBalanceItem>>(result.Result[address].Where(x => x.NfaName == nfaName && x.Token == tokenId).ToList());
		}

		public Task<bool> WaitUntilNfaIssued(string issuer, string nfaName)
		{
			return TaskHelper.WaitUntilTrue(async () =>
				(await ListNfa(issuer)).Result.Any(x => x.Name == nfaName)
			, 5, 500);
		}

		public Task<bool> WaitUntilNftIssued(string issuer, string nfaName, string tokenId)
		{
			return TaskHelper.WaitUntilTrue(async () =>
				(await ListNft(issuer, nfaName, tokenId)).Result.Count > 0
			, 5, 500);
		}


		public async Task<MultiChainResult<GetTokenBalancesResult>> GetTokenBalancesAsync(string address)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));

			return await JsonRpcRequestAsync<GetTokenBalancesResult>("gettokenbalances", address);
		}

		//public async Task<MultiChainResult<IList<GetTokenBalanceItem>>> ListTokenItems(string address, string nfaName = null, string tokenId = null)
		//{
		//	if (address is null)
		//		throw new ArgumentNullException(nameof(address));

		//	if (nfaName is null && tokenId is { })
		//		throw new MultiChainException(MultiChainErrorCode.NFA_NAME_IS_MISSING);

		//	var emptyResult = new MultiChainResult<IList<GetTokenBalanceItem>>(new List<GetTokenBalanceItem>());

		//	// Check if NFA exists
		//	var addressBalancesResult = await JsonRpcRequestAsync<IList<GetAddressBalancesResult>>("getaddressbalances", address);
		//	if (addressBalancesResult.IsError || addressBalancesResult.Result is null)
		//		return emptyResult;
		//	var nfa = addressBalancesResult.Result.SingleOrDefault(x=>x.Name == nfaName);
		//	if (nfa == null)
		//		return emptyResult;

		//	var result = await JsonRpcRequestAsync<GetTokenBalancesResult>("gettokenbalances", address);
		//	if (result.IsError || result.Result is null || result.Result.Count == 0 || result.Result[address].Count == 0)
		//		return emptyResult;

		//	// Get the NFT
		//	IList<GetTokenBalanceItem> list = null;
		//	if (tokenId is null)
		//		list = result.Result[address].Where(x => x.NfaName == nfaName).ToList();
		//	list = result.Result[address].Where(x => x.NfaName == nfaName && x.Token == tokenId).ToList();
		//	return new MultiChainResult<IList<GetTokenBalanceItem>>(list);
		//}

		public async Task<MultiChainResult<string>> IssueNfaAsync(string address, string nfaName, double amt = 0)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));

			return await JsonRpcRequestAsync<string>("issue", address, new { name = nfaName, fungible = false, open = true }, 0, 1, amt);
		}

		public async Task<MultiChainResult<string>> IssueNfaFromAsync(string from, string to, string nfaName, double amt = 0)
		{
			if (from is null)
				throw new ArgumentNullException(nameof(from));
			if (to is null)
				throw new ArgumentNullException(nameof(to));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));

			return await JsonRpcRequestAsync<string>("issuefrom", from, to, new { name = nfaName, fungible = false, open = true }, 0, 1, amt);
		}

		public async Task<MultiChainResult<string>> IssueNftAsync(string address, string nfaName, string tokenId, int qty = 1)
		{
			if (address is null)
				throw new ArgumentNullException(nameof(address));
			if (nfaName is null)
				throw new ArgumentNullException(nameof(nfaName));
			if (tokenId is null)
				throw new ArgumentNullException(nameof(tokenId));

			return await JsonRpcRequestAsync<string>("issuetoken", address, nfaName, tokenId, qty);
		}

		public async Task<MultiChainResult<string>> IssueNftFromAsync(string from, string to, string nfaName, string tokenId, int qty = 1)
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

		public async Task<MultiChainResult<string>> SendNftAsync(string to, string nfaName, string tokenId, int qty = 1)
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

		public async Task<MultiChainResult<string>> SendNftFromAsync(string from, string to, string nfaName, string tokenId, int qty = 1)
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
