using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class StreamRestriction
	{
		[JsonProperty("write")]
		public bool Write { get; set; }

		[JsonProperty("read")]
		public bool Read { get; set; }

		[JsonProperty("onchain")]
		public bool OnChain { get; set; }

		[JsonProperty("offchain")]
		public bool OffChain { get; set; }
	}
}
