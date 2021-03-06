using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiChainDotNet.Core.MultiChainToken.GetTokenBalancesResult;

namespace MultiChainDotNet.Core.MultiChainToken
{
	// first type argumen is balance owner address
    public class GetTokenBalancesResult : Dictionary<string, List<GetTokenBalanceItem>>
	{
		public class GetTokenBalanceItem
		{
			[JsonProperty("asset")]
			public string NfaName { get; set; }
			[JsonProperty("assetref")]
			public string AssetRef { get; set; }
			[JsonProperty("token")]
			public string Token { get; set; }
			[JsonProperty("qty")]
			public double Qty { get; set; }
		}
		
    }
}
