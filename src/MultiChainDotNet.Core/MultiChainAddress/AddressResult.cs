using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainAddress
{
	public class AddressResult
	{
		[JsonProperty("address")]
		public string Address { get; set; }

		[JsonProperty("ismine")]
		public bool IsMine { get; set; }
	}
}
