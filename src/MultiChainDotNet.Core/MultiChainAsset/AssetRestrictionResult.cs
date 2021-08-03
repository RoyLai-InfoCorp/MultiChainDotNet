using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainAsset
{
	public class AssetRestrictionResult
	{
		[JsonProperty("send")]
		public bool Send { get; set; }

		[JsonProperty("receive")]
		public bool Receive { get; set; }

		[JsonProperty("issue")]
		public bool Issue { get; set; }
	}
}
