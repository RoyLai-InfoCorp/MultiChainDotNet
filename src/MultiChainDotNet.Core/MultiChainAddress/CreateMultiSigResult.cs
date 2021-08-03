using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainAddress
{
	public class CreateMultiSigResult
	{
		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("redeemScript")]
		public string RedeemScript { get; set; }
	}
}
