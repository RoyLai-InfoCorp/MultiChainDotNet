using Newtonsoft.Json;

namespace MultiChainDotNet.Core.Base
{
	public class MultiChainApiMethod
	{
		[JsonProperty("method")]
		public string Method { get; set; }
		[JsonProperty("chain_name")]
		public string ChainName { get; set; }
		[JsonProperty("params")]
		public object[] Params { get; set; }
	}
}
